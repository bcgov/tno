using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.FileMonitor.Config;
using TNO.Core.Exceptions;

namespace TNO.Services.FileMonitor;

/// <summary>
/// FileMonitorAction class, performs the newspaper ingestion action.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class FileMonitorAction : IngestAction<FileMonitorOptions>
{
    #region Properties
    /// <summary>
    /// get - The logger for the command action.
    /// </summary>
    public ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileMonitorAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileMonitorAction(IApiService api, IOptions<FileMonitorOptions> options, ILogger<FileMonitorAction> logger) : base(api, options)
    {
        this.Logger = logger;
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
    /// <exception cref="InvalidOperationException"></exception>
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);

        var dir = GetOutputPath(manager.Ingest);

        if (String.IsNullOrEmpty(dir))
        {
            throw new InvalidOperationException($"No import directory defined for ingest '{manager.Ingest.Name}'");
        }

        await FetchFilesFromRemoteAsync(manager);
        var format = manager.Ingest.GetConfigurationValue(Fields.FileFormat);
        var selfPublished = manager.Ingest.GetConfigurationValue<bool>(Fields.SelfPublished);
        var sources = selfPublished ? new Dictionary<string, string>() : GetSourcesForIngest(manager.Ingest);

        format = !String.IsNullOrEmpty(format) ? format : "xml";

        this.Logger.LogDebug("Parsing files for '{name}'", manager.Ingest.Name);
        switch (format.ToLower())
        {
            case "xml":
                await GetXmlArticlesAsync(dir, manager, sources);
                break;
            case "fms":
                await GetFmsArticlesAsync(dir, manager, sources);
                break;
            default: throw new InvalidOperationException($"Invalid import file format defined for '{manager.Ingest.Name}'");
        };
    }

    /// <summary>
    /// Retrieve files for this connection from the remote server.
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public async Task FetchFilesFromRemoteAsync(IIngestServiceActionManager manager)
    {
        // Extract the source connection configuration settings.
        var remotePath = manager.Ingest.SourceConnection?.GetConfigurationValue("path");
        var username = manager.Ingest.SourceConnection?.GetConfigurationValue("username");
        var keyFileName = manager.Ingest.SourceConnection?.GetConfigurationValue("keyFileName") ?? "";
        var hostname = manager.Ingest.SourceConnection?.GetConfigurationValue("hostname");
        var password = manager.Ingest.SourceConnection?.GetConfigurationValue("password");
        if (String.IsNullOrWhiteSpace(hostname)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'hostname'.");
        if (String.IsNullOrWhiteSpace(username)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'username'.");
        if (String.IsNullOrWhiteSpace(keyFileName) && String.IsNullOrWhiteSpace(password)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' one of 'keyFileName' or 'password' required in source connection.");
        if (String.IsNullOrWhiteSpace(remotePath)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'path'.");

        // The ingest configuration may have a different path than the root connection path.
        remotePath = remotePath.CombineWith(manager.Ingest.GetConfigurationValue("path")?.MakeRelativePath() ?? "");
        AuthenticationMethod? authMethod;
        if (!String.IsNullOrWhiteSpace(password))
        {
            authMethod = new PasswordAuthenticationMethod(username, password);
        }
        else if (!String.IsNullOrWhiteSpace(keyFileName))
        {
            var sshKeyFile = this.Options.PrivateKeysPath.CombineWith(keyFileName);
            if (File.Exists(sshKeyFile))
            {
                var keyFile = new PrivateKeyFile(sshKeyFile);
                var keyFiles = new[] { keyFile };
                authMethod = new PrivateKeyAuthenticationMethod(username, keyFiles);
            }
            else
            {
                throw new ConfigurationException($"SSH Private key file does not exist: {sshKeyFile}");
            }
        }
        else throw new ConfigurationException("Data location connection settings are missing");

        var connectionInfo = new ConnectionInfo(hostname, username, authMethod);
        await FetchFiles(connectionInfo, remotePath, manager);
    }

    /// <summary>
    /// Fetch all files matching the ingest's file pattern from the remote sftp host.
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <param name="remotePath"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task FetchFiles(ConnectionInfo connectionInfo, string remotePath, IIngestServiceActionManager manager)
    {
        // Extract the ingest configuration settings
        var code = manager.Ingest.Source?.Code ?? "";
        var filePattern = String.IsNullOrWhiteSpace(manager.Ingest.GetConfigurationValue("filePattern")) ? code : manager.Ingest.GetConfigurationValue("filePattern");
        if (String.IsNullOrWhiteSpace(filePattern)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' is missing a 'filePattern'.");
        filePattern = filePattern.Replace("<date>", $"{GetDateTimeForTimeZone(manager.Ingest, DateTime.Now.AddDays(manager.Ingest.GetConfigurationValue<double>("dateOffset"))):yyyyMMdd}");
        var match = new Regex(filePattern);
        using var client = new SftpClient(connectionInfo);
        client.Connect();

        remotePath = remotePath.Replace("~/", $"{client.WorkingDirectory}/");
        var files = await FetchFileListingAsync(client, remotePath);
        files = files.Where(f => match.Match(f.Name).Success);
        this.Logger.LogDebug("{count} files were discovered that match the filter '{filter}'.", files.Count(), filePattern);

        foreach (var file in files)
        {
            await CopyFileAsync(client, manager.Ingest, remotePath.CombineWith(file.Name));
        }

        client.Disconnect();
    }

    /// <summary>
    /// Fetch a list of file names from the remote data source based on configuration.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task<IEnumerable<SftpFile>> FetchFileListingAsync(SftpClient client, string path)
    {
        this.Logger.LogDebug("Requesting files at this path '{path}'", path);
        // TODO: Fetch file from source data location.  Only continue if the image exists.
        return await Task.Factory.FromAsync<IEnumerable<SftpFile>>((callback, obj) => client.BeginListDirectory(path, callback, obj), client.EndListDirectory, null);
    }

    /// <summary>
    /// Copy a file for processing from the remote server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="ingest"></param>
    /// <param name="pathToFile"></param>
    /// <returns></returns>
    private async Task CopyFileAsync(SftpClient client, IngestModel ingest, string pathToFile)
    {
        var outputPath = GetOutputPath(ingest);
        var fileName = Path.GetFileName(pathToFile);
        var outputFile = outputPath.CombineWith(fileName);

        if (!File.Exists(outputFile))
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            using var saveFile = File.OpenWrite(outputFile);
            await Task.Factory.FromAsync(client.BeginDownloadFile(pathToFile, saveFile), client.EndDownloadFile);

            this.Logger.LogDebug("File copied '{file}'", pathToFile);
        }
        else
        {
            this.Logger.LogDebug("File already exists '{file}'", pathToFile);
        }
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetOutputPath(IngestModel ingest)
    {
        return this.Options.VolumePath.CombineWith(ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "", $"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/");
    }

    /// <summary>
    /// Get the path to the source file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetInputPath(IngestModel ingest)
    {
        return (ingest.SourceConnection?.GetConfigurationValue("path") ?? "").CombineWith(ingest.GetConfigurationValue("path")?.MakeRelativePath() ?? "", $"{GetDateTimeForTimeZone(ingest):yyyy/MM/dd}/");
    }

    /// <summary>
    /// Iterate through the list of articles and import content into api.
    /// Checks if a content reference has already been created for each item before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private async Task ImportArticleAsync(IIngestServiceActionManager manager, SourceContent item)
    {
        try
        {
            // Fetch content reference.
            var reference = await this.Api.FindContentReferenceAsync(manager.Ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{manager.Ingest.Name}' is missing source code."), item.Uid);
            var sendMessage = true;

            if (reference == null)
            {
                reference = await AddContentReferenceAsync(manager.Ingest, item);
            }
            else if ((reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddHours(1) < DateTime.UtcNow) ||
                    (reference.Status != (int)WorkflowStatus.InProgress && reference.UpdatedOn != item.PublishedOn))
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
                var result = await this.Api.SendMessageAsync(manager.Ingest.Topic, item);

                // Update content reference with Kafka response.
                await UpdateContentReferenceAsync(reference, item);
            }
        }
        catch (Exception ex)
        {
            // Reached limit return to ingest manager.
            if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                throw;

            this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
            await manager.RecordFailureAsync();
        }
    }

    /// <summary>
    /// Send AJAX request to api to add content reference.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(IngestModel ingest, SourceContent item)
    {
        // Add a content reference record.
        var model = new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = item.Uid,
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress,
            PublishedOn = item.PublishedOn
        };

        return await this.Api.AddContentReferenceAsync(model);
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
    /// Each of these keys maps to an entry in the ingest's Connection string which contains the tag/attribute name. If the
    /// value is stored in a tag, the Connection string entry contains the name of the tag, e.g., for the GLOBE ingest
    /// the story content is in the body.content tag. For the BCNG ingest it is in the story tag, so the configuration
    /// entries for those two items are:
    ///
    ///     "story": "body.content"
    ///     "story": "story"
    ///
    /// respectively. If the value is stored in a tag's attribute, the Connection string value will have the following format:
    ///
    ///     tag-name!attribute-name
    ///
    /// For the GLOBE ingest the publication date is stored in the date.publication attribute of the pubdata tag for each
    /// story. This results in the following entry for "date" in the configuration string for GLOBE:
    ///
    ///     "date": "pubdata!date.publication"
    ///
    /// </summary>
    /// <param name="story"></param>
    /// <param name="key"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string GetXmlData(XmlElement story, string key, IngestModel ingest)
    {
        var value = ingest.GetConfigurationValue(key);
        var result = "";

        if (String.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Ingest configuration value '{key}' is not defined for {ingest.Name}.");
        }
        else
        {
            var comps = value.Split('!');
            if (comps.Length == 2) // Tag and attribute name present
            {
                XmlNodeList nodeList = story.GetElementsByTagName(comps[0]);

                if (nodeList is not null && nodeList.Count > 0 && nodeList[0]?.Attributes?.Count > 0)
                {
                    result = nodeList[0]!.Attributes![comps[1]]?.Value ?? "";
                }
                else
                {
                    throw new InvalidOperationException($"No node/attribute pair in story XML matching '{comps[0]}/{comps[1]}' for ingest '{ingest.Name}.");
                }
            }
            else
            if (comps.Length == 1) // Tag name only
            {
                try
                {
                    XmlNodeList nodeList = story.GetElementsByTagName(value);
                    result = nodeList is not null && nodeList.Count > 0 ? nodeList[0]!.InnerText : "";
                }
                catch (Exception e)
                {
                    this.Logger.LogWarning(e, "Error extracting node '{key}' for ingest {name}.", key, ingest.Name);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get a news item value (e.g. headline, published date, author) from an FMS story. The keys for the list of supported values
    /// is in the Fields class. Each of these keys maps to an entry in the ingest's Connection string which contains the field name.
    /// Fields are extracted using regular expressions. Each field, which can include multiple lines, is terminated with the string "&lt;break&gt;".
    /// A field is not guaranteed to be present for a particular story, in which case the empty string is returned.
    /// </summary>
    /// <param name="story"></param>
    /// <param name="key"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetFmsData(string story, string key, IngestModel ingest)
    {
        var value = ingest.GetConfigurationValue(key);

        if (String.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Ingest configuration value '{key}' is not defined for {ingest.Name}.");
        }
        else
        {
            var matchStr = value + "(.+?)<break>";
            var matches = Regex.Matches(story, matchStr, RegexOptions.Singleline);

            // Make sure each paragraph of a story is on a single line and that paragraphs are delimited by "<p>".
            if (matches.Count > 0 && matches[0].Groups.Count > 0)
            {
                return matches[0].Groups[1].Value.Replace("\n", " ").Replace("\r", "").Replace("|", "<p>").Trim();
            }
            else
            {
                return "";
            }
        }
    }

    /// <summary>
    /// Get a separate XML document for each file in dir and return a list of SourceContent records, one for each story in the
    /// list of files. Each XML document will include one or more stories. None of the XML formats we import contain the language.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="manager"></param>
    /// <param name="sources"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    private async Task GetXmlArticlesAsync(string dir, IIngestServiceActionManager manager, Dictionary<string, string> sources)
    {
        var ingest = manager.Ingest;
        var fileList = Directory.GetFiles(dir);

        foreach (string path in fileList)
        {
            try
            {
                // Extract a list of stories from the current document.
                XmlDocument? document = GetValidXmlDocument(path, ingest);
                if (document != null)
                {
                    XmlNodeList elementList = document.GetElementsByTagName(ingest.GetConfigurationValue(Fields.Item));

                    // Iterate over the list of stories and add a new item to the articles list for each story.
                    foreach (XmlElement story in elementList)
                    {
                        var paperName = GetXmlData(story, Fields.PaperName, ingest);
                        var code = GetItemSourceCode(ingest, paperName, sources);

                        if (!string.IsNullOrEmpty(code)) // This is a valid newspaper source
                        {
                            var productId = await GetProductIdAsync(ingest, code, sources);
                            var item = new SourceContent(
                                this.Options.DataLocation,
                                GetItemSourceCode(ingest, paperName, sources),
                                ContentType.PrintContent,
                                productId,
                                GetXmlData(story, Fields.Id, ingest),
                                GetXmlData(story, Fields.Headline, ingest),
                                GetXmlData(story, Fields.Summary, ingest),
                                GetXmlData(story, Fields.Story, ingest),
                                GetPublishedOn(GetXmlData(story, Fields.Date, ingest), ingest))
                            {
                                Page = GetXmlData(story, Fields.Page, ingest),
                                Section = GetXmlData(story, Fields.Section, ingest),
                                Language = ingest.GetConfigurationValue("language"),
                                Authors = GetAuthorList(GetXmlData(story, Fields.Author, ingest)),
                            };

                            await ImportArticleAsync(manager, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"File contents at '{path}' is invalid.", ex);
            }
        }
    }

    /// <summary>
    /// Get a separate FMS document for each file in dir and return a list of SourceContent records, one for each story in the
    /// list of files. Each document will include one or more stories. Story/field extraction is accomplished using Regex.
    /// FMS files contain multiple tags in the !@LKW section which are loaded into SourceContent.tags. There is no comparable
    /// data in any of the XML file formats.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="manager"></param>
    /// <param name="sources"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    private async Task GetFmsArticlesAsync(string dir, IIngestServiceActionManager manager, Dictionary<string, string> sources)
    {
        var ingest = manager.Ingest;
        var fileList = Directory.GetFiles(dir);

        // Iterate over the files in the list and process the stories they contain.
        foreach (string path in fileList)
        {
            try
            {
                // Extract a list of stories from the current document. Replace the story delimiter with a string that
                // facilitates the extraction of stories using regular expressions in single-line mode.
                string? document = ReadFileContents(path, ingest);
                if (document != null)
                {
                    var doc = document.Replace(ingest.GetConfigurationValue(Fields.Item), Fields.FmsStoryDelim) + Fields.FmsEofFlag;
                    var matches = Regex.Matches(doc, "<story>(.+?)</story>", RegexOptions.Singleline);
                    var code = "";

                    // Iterate over the list of stories and add a new item to the articles list for each story.
                    foreach (Match story in matches.Cast<Match>())
                    {
                        var preFiltered = story.Groups[1].Value;

                        // Single line mode prevents matching on "\n\n", so replace this with a meaningful field delimiter.
                        var filtered = preFiltered.Replace("\n\n", Fields.FmsFieldDelim);

                        var papername = GetFmsData(filtered, Fields.PaperName, ingest);
                        code = string.IsNullOrEmpty(code) ? GetItemSourceCode(ingest, papername, sources) : code;

                        if (!string.IsNullOrEmpty(code)) // This is a valid newspaper source
                        {
                            var productId = await GetProductIdAsync(ingest, code, sources);
                            var item = new SourceContent(
                                this.Options.DataLocation,
                                code,
                                ContentType.PrintContent,
                                productId,
                                GetFmsData(filtered, Fields.Id, ingest),
                                GetFmsData(filtered, Fields.Headline, ingest),
                                GetFmsData(filtered, Fields.Summary, ingest),
                                GetFmsData(preFiltered + "<break>", Fields.Story, ingest),
                                GetPublishedOn(GetFmsData(filtered, Fields.Date, ingest), ingest))
                            {
                                Page = GetFmsData(filtered, Fields.Page, ingest),
                                Section = GetFmsData(filtered, Fields.Section, ingest),
                                Language = ingest.GetConfigurationValue("language"),

                                Tags = GetTagList(filtered, ingest),
                                Authors = GetAuthorList(GetFmsData(filtered, Fields.Author, ingest))
                            };

                            await ImportArticleAsync(manager, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"File contents for ingest '{ingest.Name}' is invalid.", ex);
            }
        }
    }

    /// <summary>
    /// Get the "productId" value for the current source. If the ingest is selfPublished (relates to only one publication) the sources
    /// dictionary will be empty and the productId for the ingest will be returned. If sources is not empty the source for the "code"
    /// parameter will be retrieved and if the source's override productId is not null it will be returned.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="code"></param>
    /// <param name="sources"></param>
    /// <returns></returns>
    private async Task<int> GetProductIdAsync(IngestModel ingest, string code, Dictionary<string, string> sources)
    {
        if (sources.Count == 0) // Self published
        {
            return ingest.ProductId;
        }
        else
        {
            var source = await this.Api.GetSourceForCodeAsync(code);
            if (source == null)
            {
                return ingest.ProductId;
            }
            else
            {
                return source.ProductId ?? ingest.ProductId;
            }
        }
    }

    /// <summary>
    /// Get the "code" value for the current paper. If the ingest is selfPublished (relates to only one publication) the sources
    /// dictionary will be empty and the code for the ingest will be returned. If sources is not empty it will contain a list
    /// of paper names and their corresponding codes. In this case the code for the item is retrieved using the paperName
    /// parameter as the key to the dictionary.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="paperName"></param>
    /// <param name="sources"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string GetItemSourceCode(IngestModel ingest, string paperName, Dictionary<string, string> sources)
    {
        if (sources.Count == 0) // Self published
        {
            return ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code.");
        }
        else
        {
            if (sources.TryGetValue(paperName, out string? code))
            {
                return code ?? "";
            }
            else
            {
                var defaultSource = ingest.GetConfigurationValue("defaultSource");
                if (string.IsNullOrEmpty(defaultSource))
                {
                    this.Logger.LogWarning("Paper '{paperName}' not in configuration string for ingest, and there is no default source for '{ingest.Name}'.", paperName, ingest.Name);
                }
                return defaultSource;
            }
        }
    }

    /// <summary>
    /// Get a dictionary containing key/value pairs of the form "papername=code" extracted from the ingest record's
    /// sources attribute.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    private static Dictionary<string, string> GetSourcesForIngest(IngestModel ingest)
    {
        var sources = new Dictionary<string, string>();
        var sourcesStr = ingest.GetConfigurationValue(Fields.Sources);
        var sourcesArr = sourcesStr.Split("&");

        foreach (var source in sourcesArr)
        {
            var pair = source.Split('=');

            if (pair.Length == 2)
            {
                sources.Add(pair[0], pair[1]);
            }
            else
            {
                throw new ConfigurationException($"Invalid source value in ingest configuration. Source '{source}' for ingest '{ingest.Name}'");
            }
        }

        return sources;
    }
    #endregion

    #region Support Methods
    /// <summary>
    /// Get a valid XmlDocument representing the contents of the fileName parameter. Apply fixes to the XML text
    /// in the file based on the shortcomings of the ingest. Fixes are indicated by Connection string entries.
    /// The following fixes are supported:
    ///
    ///     "addParent" - The BCNG files lack a single parent tag so one is inserted.
    ///     "escapeContent" - BCNG stories contain parsing errors, so they are stored as CDATA.
    ///
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private XmlDocument? GetValidXmlDocument(string filePath, IngestModel ingest)
    {
        var xmlTxt = ReadFileContents(filePath, ingest);

        try
        {
            if (xmlTxt != null)
            {
                // BCNG files have multiple top-level objects which need to be wrapped in a single pair of tags.
                var addParent = ingest.GetConfigurationValue<bool>(Fields.AddParent);
                if (addParent)
                {
                    xmlTxt = FixParentTag(xmlTxt);
                }

                // BCNG stories contain invalid XHTML content which must be escaped to parse the document.
                var escapeContent = ingest.GetConfigurationValue<bool>(Fields.EscapeContent);
                if (escapeContent)
                {
                    xmlTxt = StoryToCdata(xmlTxt, ingest);
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlTxt);
                return xmlDoc;
            }
            else
            {
                return null;
            }
        }
        catch (System.Xml.XmlException e)
        {
            this.Logger.LogError(e, "Failed to ingest item from file '{path}'", filePath);
            throw e;
        }
    }

    /// <summary>
    /// Get the contents of the file at the location indicated by filePath as a string.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private string? ReadFileContents(string filePath, IngestModel ingest)
    {
        this.Logger.LogDebug("Reading file '{file}' for ingest '{name}'", filePath, ingest.Name);
        var sr = new System.IO.StreamReader(filePath);
        var contents = sr.ReadToEnd();
        sr.Close();

        if (String.IsNullOrWhiteSpace(contents))
        {
            this.Logger.LogWarning("Missing file contents at '{path}'", filePath);
            return null;
        }
        else
        {
            return contents;
        }
    }

    /// <summary>
    /// Insert a document tag after the opening xml tag of the file and a closing document tag at the end.
    /// This is necessary for BCNG documents that have multiple BCNG parent tags, which breaks the parser.
    /// </summary>
    /// <param name="xmlTxt"></param>
    /// <returns></returns>
    private static string FixParentTag(string xmlTxt)
    {
        var pos = xmlTxt.IndexOf("?>");
        return String.Concat(xmlTxt.Insert(pos + 2, "\n<document>\n"), "\n</document>\n");
    }

    /// <summary>
    /// Wrap the story content for all stories in xmlTxt with opening and closing CDATA tags.
    /// </summary>
    /// <param name="xmlTxt"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static string StoryToCdata(string xmlTxt, IngestModel ingest)
    {
        var storyTag = ingest.GetConfigurationValue(Fields.Story);

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
    /// Generate a list of Author objects from the story. Current ingests only support one author entry.
    /// No email or url values are provided for authors in any of the files we import. These may be included
    /// in the story text, but they are not easily extracted. There is no standard list format from which
    /// multiple authors can be extracted, so if there is more than one author all names are included in the
    /// same Author() object.
    /// </summary>
    /// <param name="author"></param>
    /// <returns></returns>
    private static List<Author> GetAuthorList(string author)
    {
        var authors = new List<Author>
            {
                new Author(author.Trim(), "", "")
            };
        return authors;
    }

    /// <summary>
    /// Get the published date for the story in Universal Time.
    /// </summary>
    /// <param name="dateStr"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static DateTime GetPublishedOn(string dateStr, IngestModel ingest)
    {
        var dateFmt = ingest.GetConfigurationValue(Fields.DateFmt);
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
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static List<TNO.Kafka.Models.Tag> GetTagList(string filtered, IngestModel ingest)
    {
        var tags = GetFmsData(filtered, Fields.Tags, ingest);
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
