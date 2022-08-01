using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Diagnostics.Tracing;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Reflection.Metadata;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Http;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Kafka;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.Filemonitor.Config;

namespace TNO.Services.Filemonitor;

/// <summary>
/// FilemonitorAction class, performs the newspaper ingestion action.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class FilemonitorAction : IngestAction<FilemonitorOptions>
{
    #region Variables
    private readonly IHttpRequestClient _httpClient;
    private readonly IKafkaMessenger _kafka;
    private readonly ILogger _logger;
    private readonly IApiService _api;
    private readonly IOptions<FilemonitorOptions> _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilemonitorAction, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="api"></param>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FilemonitorAction(IHttpRequestClient httpClient, IApiService api, IKafkaMessenger kafka, IOptions<FilemonitorOptions> options, ILogger<FilemonitorAction> logger) : base(api, options)
    {
        _httpClient = httpClient;
        _kafka = kafka;
        _logger = logger;
        _api = api;
        _options = options;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync<T>(IDataSourceIngestManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug($"Performing ingestion service action for data source '{manager.DataSource.Code}'");
        var dir = Path.Join(_options.Value.ImportRoot, GetConnectionValue(manager.DataSource, Fields.ImportDir));
        List<SourceContent> articles = new List<SourceContent>();

        if (String.IsNullOrEmpty(dir))
        {
            throw new InvalidOperationException($"No import directory defined for data source '{manager.DataSource.Code}'");
        }

        var format = GetConnectionValue(manager.DataSource, Fields.FileFormat);
        format = !String.IsNullOrEmpty(format) ? format : "xml";

        switch (format)
        {
            case "xml":
                articles = await GetXmlArticlesAsync(dir, manager);
                break;
            case "fms":
                articles = await GetFmsArticlesAsync(dir, manager);
                break;
            default:
                throw new InvalidOperationException($"Invalid import file format defined for '{manager.DataSource.Code}'");
        }

        await ImportArticlesAsync(manager, articles);
    }

    /// <summary>
    /// Iterate through the list of articles and import content into api.
    /// Checks if a content reference has already been created for each item before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="articles"></param>
    /// <returns></returns>
    private async Task ImportArticlesAsync(IDataSourceIngestManager manager, List<SourceContent> articles)
    {
        foreach (var item in articles)
        {
            try
            {
                // Fetch content reference.
                var reference = await this.Api.FindContentReferenceAsync(manager.DataSource.Code, item.Uid);
                var sendMessage = true;

                if (reference == null)
                {
                    reference = await AddContentReferenceAsync(manager.DataSource, item);
                }
                else if ((reference.WorkflowStatus == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddHours(1) < DateTime.UtcNow) ||
                        (reference.WorkflowStatus != (int)WorkflowStatus.InProgress && reference.UpdatedOn != item.PublishedOn))
                {
                    // TODO: verify a hash of the content to ensure it has changed. This may be slow
                    // however, but would ensure the content was physically updated.

                    // If another process has it in progress only attempt to do an import if it's
                    // more than an hour old. Assumption is that it is stuck.
                    reference = await UpdateContentReferenceAsync(reference, item);
                    sendMessage = false;
                }
                else sendMessage = false;

                // Only send a message to Kafka if this process added/updated the content reference.
                if (sendMessage && reference != null)
                {
                    // Send item to Kafka.
                    var result = await _kafka.SendMessageAsync(manager.DataSource.Topic, item);

                    // Update content reference with Kafka response.
                    if (result != null)
                        await UpdateContentReferenceAsync(reference, result);
                }
            }
            catch (Exception ex)
            {
                // Reached limit return to data source manager.
                if (manager.DataSource.FailedAttempts + 1 >= manager.DataSource.RetryLimit)
                    throw;

                _logger.LogError(ex, "Failed to ingest item for data source '{Code}'", manager.DataSource.Code);
                await manager.RecordFailureAsync();
            }
        }
    }

    /// <summary>
    /// Send AJAX request to api to add content reference.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(DataSourceModel dataSource, SourceContent item)
    {
        // Add a content reference record.
        var model = new ContentReferenceModel()
        {
            Source = dataSource.Code,
            Uid = item.Uid,
            Topic = dataSource.Topic,
            WorkflowStatus = (int)WorkflowStatus.InProgress,
            PublishedOn = item.PublishedOn
        };

        return await this.Api.AddContentReferenceAsync(model);
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// This content reference has been successfully recieved by Kafka.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference, DeliveryResult<string, SourceContent> result)
    {
        reference.Offset = result.Offset;
        reference.Partition = result.Partition;
        reference.WorkflowStatus = (int)WorkflowStatus.Received;
        return await this.Api.UpdateContentReferenceAsync(reference);
    }

    /// <summary>
    /// Send AJAX request to api to update content reference.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel reference, SourceContent item)
    {
        reference.PublishedOn = item.PublishedOn != DateTime.MinValue ? item.PublishedOn : null;
        //reference.SourceUpdateOn = item.LastUpdatedTime != DateTime.MinValue ? item.LastUpdatedTime.UtcDateTime : null;
        return await this.Api.UpdateContentReferenceAsync(reference);
    }

    /// <summary>
    /// Get a news item value (e.g. headline, published date, author) from an XML story. Different publications store these values
    /// in different tags/attributes in the XML document. The keys for the list of supported values is in the Fields class.
    ///
    /// Each of these keys maps to an entry in the data source's Connection string which contains the tag/attribute name. If the
    /// value is stored in a tag, the Connection string entry contains the name of the tag, e.g., for the GLOBE data source
    /// the story content is in the body.content tag. For the BCNG data source it is in the story tag, so the connection
    /// entries for those two items are:
    ///
    ///     "story": "body.content"
    ///     "story": "story"
    ///
    /// respectively. If the value is stored in a tag's attribute, the Connection string value will have the following format:
    ///
    ///     tag-name!attribute-name
    ///
    /// For the GLOBE data source the publication date is stored in the date.publication attribute of the pubdata tag for each
    /// story. This results in the following entry for "date" in the connection string for GLOBE:
    ///
    ///     "date": "pubdata!date.publication"
    ///
    /// </summary>
    /// <param name="story"></param>
    /// <param name="key"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private string GetXmlData(XmlElement story, string key, DataSourceModel dataSource)
    {
        var value = GetConnectionValue(dataSource, key);
        var result = "";

        if (!String.IsNullOrEmpty(value))
        {
            var comps = value.Split('!');
            if (comps.Count() == 2) // Tag and attribute name present
            {
                XmlNodeList nodeList = story.GetElementsByTagName(comps[0]);

                if (nodeList is not null && nodeList.Count > 0 && nodeList[0]!.Attributes is not null && nodeList![0]!.Attributes!.Count > 0)
                {
                    result = nodeList![0]!.Attributes![comps[1]!]!.Value;
                }
                else
                {
                    throw new InvalidOperationException($"No node/attribute pair in story XML matching '{comps[0]}/{comps[1]}' for data source '{dataSource.Code}.");
                }
            }
            else
            if (comps.Count() == 1) // Tag name only
            {
                try
                {
                    XmlNodeList nodeList = story.GetElementsByTagName(value);
                    result = nodeList is not null && nodeList.Count > 0 ? nodeList[0]!.InnerText : "";
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, $"Error extracting node '{key}' for data source {dataSource.Code}.");
                }
            }
        }
        else
        {
            throw new InvalidOperationException($"Data source connection value '{key}' is not defined for {dataSource.Code}.");
        }

        return result;
    }

    /// <summary>
    /// Get a news item value (e.g. headline, published date, author) from an FMS story. The keys for the list of supported values
    /// is in the Fields class. Each of these keys maps to an entry in the data source's Connection string which contains the field name.
    /// Fields are extracted using regular expressions. Each field, which can include multiple lines, is terminated with the string "&lt;break&gt;".
    /// A field is not guaranteed to be present for a particlular story, in which case "Undefined" is returned.
    /// </summary>
    /// <param name="story"></param>
    /// <param name="key"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private string GetFmsData(string story, string key, DataSourceModel dataSource)
    {
        var value = GetConnectionValue(dataSource, key);

        if (!String.IsNullOrEmpty(value))
        {
            var matchStr = value + "(.+?)<break>";
            var matches = Regex.Matches(story, matchStr, RegexOptions.Singleline);

            // Make sure each paragraph of a story is on a single line and that paragraphcs are delimited by "<p>".
            if (matches.Count > 0 && matches[0].Groups.Count > 0)
            {
                return matches[0].Groups[1].Value.Replace("\n", " ").Replace("\r", "").Replace("|", "<p>").Trim();
            }
            else
            {
                return "Undefined";
            }
        }
        else
        {
            throw new InvalidOperationException($"Data source connection value '{key}' is not defined for {dataSource.Code}.");
        }
    }

    /// <summary>
    /// Get a separate XML document for each file in dir and return a list of SourceContent records, one for each story in the
    /// list of files. Each XML document will include one or more stories. None of the XML formats we import contain the language.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task<List<SourceContent>> GetXmlArticlesAsync(string dir, IDataSourceIngestManager manager) // should this be async?
    {
        var articles = new List<SourceContent>();
        var dataSource = manager.DataSource;
        var xmlDocs = GetXmlFileContentList(dir, dataSource);

        foreach (KeyValuePair<string, XmlDocument> document in xmlDocs)
        {
            try
            {
                // Extract a list of stories from the current document.
                XmlNodeList elementList = document.Value.GetElementsByTagName(GetConnectionValue(dataSource, Fields.Item));

                // Iterate over the list of stories and add a new item to the articles list for each story.
                foreach (XmlElement story in elementList)
                {
                    var item = new SourceContent();
                    var papername = GetXmlData(story, Fields.Papername, dataSource);
                    item.Source = await GetItemSourceCodeAsync(dataSource, papername);
                    item.Title = GetXmlData(story, Fields.Headline, dataSource);
                    item.Uid = GetXmlData(story, Fields.Id, dataSource);
                    item.Summary = GetXmlData(story, Fields.Story, dataSource);
                    item.FilePath = document.Key;
                    item.Abstract = GetXmlData(story, Fields.Summary, dataSource);
                    item.Page = GetXmlData(story, Fields.Page, dataSource);
                    item.Section = GetXmlData(story, Fields.Section, dataSource);

                    item.Authors = GetAuthorList(GetXmlData(story, Fields.Author, dataSource));
                    item.PublishedOn = GetPublishedOn(GetXmlData(story, Fields.Date, dataSource), dataSource);

                    articles.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"File contents for data source '{dataSource.Code}' is invalid.");
                throw;
            }
        }

        return articles;
    }

    /// <summary>
    /// Get a separate FMS document for each file in dir and return a list of SourceContent records, one for each story in the
    /// list of files. Each document will include one or more stories. Story/field extraction is accomplished using Regex.
    /// FMS files contain multiple tags in the !@LKW section which are loaded into SourceContent.tags. There is no comparable
    /// data in any of the XML file formats.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task<List<SourceContent>> GetFmsArticlesAsync(string dir, IDataSourceIngestManager manager)
    {
        var articles = new List<SourceContent>();
        var dataSource = manager.DataSource;
        var fmsDocs = GetFmsFileContentList(dir, dataSource);

        // Iterate over the files in the list and process the stories they contain.
        foreach (KeyValuePair<string, string> document in fmsDocs)
        {
            try
            {
                // Extract a list of stories from the current document. Replace the story delimiter with a string that
                // facilitates the extraction of stories using regular expressions in single-line mode.
                var doc = document.Value.Replace(GetConnectionValue(dataSource, Fields.Item), Fields.FmsStoryDelim) + Fields.FmsEofFlag;
                var matches = Regex.Matches(doc, "<story>(.+?)</story>", RegexOptions.Singleline);
                var source = "";

                // Iterate over the list of stories and add a new item to the articles list for each story.
                foreach (Match story in matches)
                {
                    var item = new SourceContent();
                    var preFiltered = story.Groups[1].Value;

                    // Single line mode prevents matching on "\n\n", so replace this with a meaningful field delimiter.
                    var filtered = preFiltered.Replace("\n\n", Fields.FmsFieldDelim);

                    var papername = GetFmsData(filtered, Fields.Papername, dataSource);
                    source = string.IsNullOrEmpty(source) ? await GetItemSourceCodeAsync(dataSource, papername) : source;
                    item.Source = source;
                    item.Title = GetFmsData(filtered, Fields.Headline, dataSource);
                    item.Uid = GetFmsData(filtered, Fields.Id, dataSource);
                    item.Summary = GetFmsData(preFiltered + "<break>", Fields.Story, dataSource);
                    item.Language = GetFmsData(filtered, Fields.Lang, dataSource);
                    item.FilePath = document.Key;
                    item.Abstract = GetFmsData(filtered, Fields.Summary, dataSource);
                    item.Page = GetFmsData(filtered, Fields.Page, dataSource);
                    item.Section = GetFmsData(filtered, Fields.Section, dataSource);

                    item.Tags = GetTagList(filtered, dataSource);
                    item.Authors = GetAuthorList(GetFmsData(filtered, Fields.Author, dataSource));
                    item.PublishedOn = GetPublishedOn(GetFmsData(filtered, Fields.Date, dataSource), dataSource);

                    articles.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"File contents for data source '{dataSource.Code}' is invalid.");
                throw;
            }
        }

        return articles;
    }

    /// <summary>
    /// Get the data source's "code" value. If the data source is selfPublished (relates to only one publication) the code
    /// of the data source is returned. If the content string attribute "selfPublished" is false, the actual data source for
    /// the publication is retrieved by the value of the paperName parameter.
    ///
    /// The name of each paper is not guaranteed to be unique, so we may have to define multiple data sources for the same
    /// publication, one for each unique name.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="paperName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<string> GetItemSourceCodeAsync(DataSourceModel dataSource, string paperName)
    {
        // The default for selfPublished is false, so all self-published papers must have a value in connection string.
        var selfPublished = GetConnectionValue(dataSource, Fields.SelfPublished);
        selfPublished = !String.IsNullOrEmpty(selfPublished) ? selfPublished : "true";

        if (Boolean.Parse(selfPublished))
        {
            return dataSource.Code;
        }
        else
        {
            var childDataSource = await _api.GetDataSourceByNameAsync(paperName);
            return childDataSource != null ? childDataSource.Code : "No code found for: " + paperName;
        }
    }

    /// <summary>
    /// Get a value from the data source Connection string. If the key does not exist, return the empty string.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetConnectionValue(DataSourceModel dataSource, string key)
    {
        if (!dataSource.Connection.TryGetValue(key, out object? element)) return "";

        var value = (JsonElement)element;
        if (value.ValueKind == JsonValueKind.String)
        {
            var result = value.GetString() ?? throw new InvalidOperationException($"Data source connection '{key}' cannot be null, empty or whitespace.");
            return result;
        }

        throw new InvalidOperationException($"Data source connection '{key}' is not a valid string value");
    }

    /// <summary>
    /// Get a list of XML files from the import directory and return a Dictionary of XmlDocument objects (one per file) keyed by
    /// the file path of each file. Calls GetValidXmlDocument() to apply fixes where the publisher provides invalid XML content.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private Dictionary<string, XmlDocument> GetXmlFileContentList(string dir, DataSourceModel dataSource)
    {
        var fileContentList = new Dictionary<string, XmlDocument>();
        var fileList = Directory.GetFiles(dir);

        foreach (string filePath in fileList)
        {
            var validDocument = GetValidXmlDocument(filePath, dataSource);
            fileContentList.Add(filePath, validDocument);
        }

        return fileContentList;
    }

    /// <summary>
    /// Get a list of FMS files from the import directory and return a Dictionary of strings (one per file) keyed by
    /// the file path of each file.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private Dictionary<string, string> GetFmsFileContentList(string dir, DataSourceModel dataSource)
    {
        var fileContentList = new Dictionary<string, string>();
        var fileList = Directory.GetFiles(dir);

        foreach (string filePath in fileList)
        {
            var document = ReadFileContents(filePath);
            fileContentList.Add(filePath, document);
        }

        return fileContentList;
    }
    #endregion

    #region Support Methods
    /// <summary>
    /// Get a valid XmlDocument representing the contents of the fileName parameter. Apply fixes to the XML text
    /// in the file based on the shortcomings of the data source. Fixes are indicated by Connection string entries.
    /// The following fixes are supported:
    ///
    ///     "addParent" - The BCNG files lack a single parent tag so one is inserted.
    ///     "escapeContent" - BCNG stories contain parsing errors, so they are stored as CDATA.
    ///
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private XmlDocument GetValidXmlDocument(string filePath, DataSourceModel dataSource)
    {
        var xmlDoc = new XmlDocument();
        var xmlTxt = ReadFileContents(filePath);

        // BCNG files have multiple top-level objects which need to be wrapped in a single pair of tags.
        var addParent = GetConnectionValue(dataSource, Fields.AddParent);
        if (!String.IsNullOrEmpty(addParent))
        {
            if (Boolean.Parse(addParent))
            {
                xmlTxt = FixParentTag(xmlTxt);
            }
        }

        // BCNG stories contain invalid XHTML content which must be escaped to parse the document.
        var escapeContent = GetConnectionValue(dataSource, Fields.EscapeContent);
        if (!String.IsNullOrEmpty(escapeContent))
        {
            if (Boolean.Parse(escapeContent))
            {
                xmlTxt = StoryToCdata(xmlTxt, dataSource);
            }
        }

        try
        {
            xmlDoc.LoadXml(xmlTxt);
        }
        catch (System.Xml.XmlException e)
        {
            _logger.LogError(e, $"Failed to ingest item from file '{filePath}'");
            throw e;
        }

        return xmlDoc;
    }

    /// <summary>
    /// Get the contents of the file at the location indicated by filePath as a string.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string ReadFileContents(string filePath)
    {
        System.IO.StreamReader sr = new System.IO.StreamReader(filePath);
        var contents = sr.ReadToEnd();
        sr.Close();

        return contents;
    }

    /// <summary>
    /// Insert a document tag after the opening xml tag of the file and a closing document tag at the end.
    /// This is necessary for BCNG documents that have multiple bcng parent tags, which breaks the parser.
    /// </summary>
    /// <param name="xmlTxt"></param>
    /// <returns></returns>
    private string FixParentTag(string xmlTxt)
    {
        var pos = xmlTxt.IndexOf("?>");
        return String.Concat(xmlTxt.Insert(pos + 2, "\n<document>\n"), "\n</document>\n");
    }

    /// <summary>
    /// Wrap the story content for all stories in xmlTxt with opening and closing CDATA tags.
    /// </summary>
    /// <param name="xmlTxt"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private string StoryToCdata(string xmlTxt, DataSourceModel dataSource)
    {
        var storyTag = GetConnectionValue(dataSource, Fields.Story);

        if (!String.IsNullOrEmpty(storyTag))
        {
            var findStr = "<" + storyTag + ">";
            var replaceStr = findStr + "<![CDATA[";
            xmlTxt = xmlTxt.Replace(findStr, replaceStr);

            findStr = "</" + storyTag + ">";
            replaceStr = "]]>" + findStr;
            xmlTxt = xmlTxt.Replace(findStr, replaceStr);
        }

        return xmlTxt;
    }

    /// <summary>
    /// Generate a list of Author objects from the story. Current data sources only support one author entry.
    /// No email or url values are provided for authors in any of the files we import. These may be included
    /// in the story text, but they are not easily extracted. There is no standard list format from which
    /// multiple authors can be extracted, so if there is more than one author all names are included in the
    /// same Author() object.
    /// </summary>
    /// <param name="author"></param>
    /// <returns></returns>
    private List<Author> GetAuthorList(string author)
    {
        var authors = new List<Author>();
        authors.Add(new Author(author.Trim(), "", ""));
        return authors;
    }

    /// <summary>
    /// Get the published date for the story in Universal Time.
    /// </summary>
    /// <param name="dateStr"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private DateTime GetPublishedOn(string dateStr, DataSourceModel dataSource)
    {
        var dateFmt = GetConnectionValue(dataSource, Fields.DateFmt);
        if (!String.IsNullOrEmpty(dateFmt))
        {
            var dateTime = DateTime.ParseExact(dateStr, dateFmt, CultureInfo.InvariantCulture);
            return dateTime.ToUniversalTime();
        }
        else
        {
            return DateTime.Now;
        }
    }

    /// <summary>
    /// Get the list of tags for this story. Currently only supported for FMS files.
    /// </summary>
    /// <param name="filtered"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private List<TNO.Kafka.Models.Tag> GetTagList(string filtered, DataSourceModel dataSource)
    {
        var tags = GetFmsData(filtered, Fields.Tags, dataSource);
        var tagArray = tags.Split(',');
        var tagList = new List<TNO.Kafka.Models.Tag>();
        foreach (string tag in tagArray)
        {
            tagList.Add(new TNO.Kafka.Models.Tag(tag, ""));
        }

        return tagList;
    }
    #endregion
}
