using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.FileMonitor.Config;
using Ude;

namespace TNO.Services.FileMonitor;

/// <summary>
/// FileMonitorAction class, performs the newspaper ingestion action.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public class FileMonitorAction : IngestAction<FileMonitorOptions>
{
    #region Variables
    // Regex to find the story closing tag.
    private static readonly Regex _storyTag = new Regex(@"</story\>[\t\s\n]*(<date>[0-9]{2}\-[0-9]{2}\-[0-9]{4}</date>)[\t\s\n]*<page>.*</page>[\t\s\n]*</bcng>[\t\s\n]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _storyBody = new Regex(@"<story>(?<body>.*)</story>.*?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileMonitorAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileMonitorAction(IApiService api, IOptions<FileMonitorOptions> options, ILogger<FileMonitorAction> logger) : base(api, options, logger)
    {
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
    public override async Task<ServiceActionResult> PerformActionAsync<T>(IIngestActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);

        // This ingest has just begun running.
        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);

        var dir = GetOutputPath(manager.Ingest);

        if (String.IsNullOrEmpty(dir)) throw new InvalidOperationException($"No import directory defined for ingest '{manager.Ingest.Name}'");

        await FetchFilesFromRemoteAsync(manager);
        var format = manager.Ingest.GetConfigurationValue(Fields.FileFormat);
        var sources = GetSourcesForIngest(manager.Ingest);

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
        }

        this.Logger.LogDebug("Ingestion service action complete for ingest '{name}'", manager.Ingest.Name);

        return ServiceActionResult.Success;
    }

    /// <summary>
    /// Retrieve files for this connection from the remote server.
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public async Task FetchFilesFromRemoteAsync(IIngestActionManager manager)
    {
        // Extract the source connection configuration settings.
        var remotePath = manager.Ingest.SourceConnection?.GetConfigurationValue("path") ?? "";
        var username = manager.Ingest.SourceConnection?.GetConfigurationValue("username");
        var keyFileName = manager.Ingest.SourceConnection?.GetConfigurationValue("keyFileName");
        var hostname = manager.Ingest.SourceConnection?.GetConfigurationValue("hostname");
        var password = manager.Ingest.SourceConnection?.GetConfigurationValue("password");
        var port = manager.Ingest.SourceConnection?.GetConfigurationValue<int?>("port");
        if (String.IsNullOrWhiteSpace(hostname)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'hostname'.");

        // The ingest configuration may have a different path than the root connection path.
        remotePath = remotePath.CombineWith(manager.Ingest.GetConfigurationValue("path")?.MakeRelativePath() ?? "");
        AuthenticationMethod? authMethod = null;
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

        var connectionInfo = port.HasValue ? new ConnectionInfo(hostname, port.Value, username, authMethod) : new ConnectionInfo(hostname, username, authMethod);
        await FetchFiles(connectionInfo, remotePath, manager);
    }

    /// <summary>
    /// Fetch all files matching the ingest's file pattern from the remote sftp host.
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <param name="remotePath"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    private async Task FetchFiles(ConnectionInfo connectionInfo, string remotePath, IIngestActionManager manager)
    {
        // Extract the ingest configuration settings
        var code = manager.Ingest.Source?.Code ?? "";
        var filePattern = String.IsNullOrWhiteSpace(manager.Ingest.GetConfigurationValue("filePattern")) ? code : manager.Ingest.GetConfigurationValue("filePattern");
        if (String.IsNullOrWhiteSpace(filePattern)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' is missing a 'filePattern'.");
        var offset = manager.Ingest.GetConfigurationValue<double>("dateOffset");
        var offsetDate = DateTime.Now.AddDays(offset);
        var date = GetDateTimeForTimeZone(manager.Ingest, offsetDate);
        filePattern = filePattern.Replace("<date>", $"{date:yyyyMMdd}");
        var match = new Regex(filePattern);
        using var client = new SftpClient(connectionInfo);
        try
        {
            client.Connect();

            remotePath = remotePath.Replace("~/", $"{client.WorkingDirectory}/");
            var files = await FetchFileListingAsync(client, remotePath);
            files = files.Where(f => match.Match(f.Name).Success);
            this.Logger.LogDebug("{count} files were discovered that match the filter '{filter}'.", files.Count(), filePattern);

            foreach (var file in files)
            {
                await CopyFileAsync(client, manager.Ingest, remotePath.CombineWith(file.Name));
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed at {class}.FetchFiles", nameof(FileMonitorAction));
            throw;
        }
        finally
        {
            if (client.IsConnected) client.Disconnect();
        }
    }

    /// <summary>
    /// Fetch a list of file names from the remote data source based on configuration.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ISftpFile>> FetchFileListingAsync(SftpClient client, string path)
    {
        this.Logger.LogDebug("Requesting files at this path '{path}'", path);
        try
        {
            // Create cancellation token with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // 5 minute timeout

            var task = Task.Factory.FromAsync<IEnumerable<ISftpFile>>(
                (callback, obj) => client.BeginListDirectory(path, callback, obj),
                client.EndListDirectory,
                null);

            // Wait for task completion or timeout
            await using var registration = cts.Token.Register(() =>
            {
                if (!task.IsCompleted)
                {
                    this.Logger.LogWarning("SFTP list directory operation timed out for path '{path}'", path);
                    try
                    {
                        client.Disconnect(); // Force disconnect on timeout
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Error disconnecting SFTP client after timeout");
                    }
                }
            });

            return await task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"SFTP list directory operation timed out for path '{path}'");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error listing SFTP directory '{path}'", path);
            throw;
        }
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
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task ImportArticleAsync(IIngestActionManager manager, SourceContent content)
    {
        try
        {
            // Fetch content reference.
            var reference = await this.FindContentReferenceAsync(content.Source, content.Uid);
            if (reference == null)
            {
                reference = await AddContentReferenceAsync(manager.Ingest, content);
            }
            else if ((reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow) ||
                    (reference.Status != (int)WorkflowStatus.InProgress && reference.PublishedOn < content.PublishedOn))
            {
                // If another process has it in progress only attempt to do an import if it's
                // more than an hour old. Assumption is that it is stuck.
                reference.PublishedOn = content.PublishedOn;
            }
            else reference = null;

            await this.ContentReceivedAsync(manager, reference, content);
        }
        catch (Exception ex)
        {
            // Reached limit return to ingest manager.
            if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                throw;

            this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}'", manager.Ingest.Name);
            await manager.RecordFailureAsync(ex);
            await manager.SendEmailAsync($"Failed to ingest item for ingest '{manager.Ingest.Name}'", ex);
        }
    }

    /// <summary>
    /// Send AJAX request to api to add content reference.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ContentReferenceModel?> AddContentReferenceAsync(IngestModel ingest, SourceContent content)
    {
        // Add a content reference record.
        var model = new ContentReferenceModel
        {
            Source = content.Source,
            Uid = content.Uid,
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress,
            PublishedOn = content.PublishedOn,
            Metadata = new Dictionary<string, object?> {
                { ContentReferenceMetaDataKeys.IngestSource, ingest.Source!.Code }
            }
        };

        return await this.Api.AddContentReferenceAsync(model);
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
    /// <param name="namespaceManager"></param>
    /// <returns></returns>
    private string GetXmlData(XmlElement story, string key, IngestModel ingest, XmlNamespaceManager? namespaceManager = null)
    {
        var value = ingest.GetConfigurationValue(key);
        if (!String.IsNullOrEmpty(value))
        {
            try
            {
                var node = namespaceManager != null ? story.SelectSingleNode(value, namespaceManager) : story.SelectSingleNode(value);
                return node?.InnerXml ?? "";
            }
            catch (Exception e)
            {
                this.Logger.LogWarning(e, "Error extracting XML value '{key}' for ingest {name}.", key, ingest.Name);
            }
        }

        return "";
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
    /// <param name="endOfData">The regex value that indicates the end of the data being searched for.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetFmsData(string story, string key, IngestModel ingest, string endOfData = "\n\n")
    {
        var find = ingest.GetConfigurationValue(key);

        if (String.IsNullOrEmpty(find))
        {
            throw new InvalidOperationException($"Ingest configuration value '{key}' is not defined for {ingest.Name}.");
        }
        else
        {
            var matches = Regex.Matches(story, $"{find}(.+?){endOfData}", RegexOptions.Singleline);

            // Make sure each paragraph of a story is on a single line and that paragraphs are delimited by "<p>".
            if (matches.Count > 0 && matches[0].Groups.Count > 0)
            {
                var cleaned = matches[0].Groups[1].Value
                    .ReplaceLast("&nbsp;", "")
                    .Replace("\r", "");

                cleaned = StringExtensions.ConvertTextToParagraphs(cleaned, @"\n{2,}\|?");

                // remove any single new-line characters with space as extraneous
                return cleaned.Replace("\n", " ").Trim();
            }
            else
            {
                return "";
            }
        }
    }

    /// <summary>
    /// Format text based on ingest configuration.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private string FormatText(IIngestActionManager manager, string text)
    {
        var value = new StringBuilder(text);
        var removeNewlines = manager.Ingest.GetConfigurationValue<bool>("removeNewlines", false);
        if (removeNewlines) value = value.Replace("\r\n", " ").Replace("\n", " ");
        return value.ToString();
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
    private async Task GetXmlArticlesAsync(string dir, IIngestActionManager manager, Dictionary<string, string> sources)
    {
        var fileList = GetFileList(manager, dir);
        var isBylineTitleCase = manager.Ingest.GetConfigurationValue<bool>("bylineTitleCase", false);
        var namespacesValue = manager.Ingest.GetConfigurationValue<string>("namespaces", "[]");
        var namespaces = JsonSerializer.Deserialize<XmlNamespace[]>(namespacesValue) ?? Array.Empty<XmlNamespace>();

        foreach (var path in fileList)
        {
            try
            {
                // Extract a list of stories from the current document.
                var document = GetValidXmlDocument(path, manager.Ingest);
                if (document != null)
                {
                    var namespaceManager = new XmlNamespaceManager(document.NameTable);
                    namespaces.ForEach(ns => namespaceManager.AddNamespace(ns.Id, ns.Href));

                    var elementList = document.GetElementsByTagName(manager.Ingest.GetConfigurationValue(Fields.Item));

                    // Iterate over the list of stories and add a new item to the articles list for each story.
                    foreach (XmlElement story in elementList)
                    {
                        var paperName = GetXmlData(story, Fields.PaperName, manager.Ingest, namespaceManager);
                        var code = GetItemSourceCode(manager.Ingest, paperName, sources);
                        var mediaTypeId = await GetMediaTypeIdAsync(manager.Ingest, code, sources);
                        var headline = GetXmlData(story, Fields.Headline, manager.Ingest, namespaceManager);
                        var publishedOn = GetPublishedOn(GetXmlData(story, Fields.Date, manager.Ingest, namespaceManager), manager.Ingest, this.Options);
                        var contentHash = Runners.BaseService.GetContentHash(code, headline, publishedOn);
                        var author = FormatText(manager, GetXmlData(story, Fields.Author, manager.Ingest, namespaceManager));
                        var summary = GetXmlData(story, Fields.Summary, manager.Ingest, namespaceManager);
                        var body = GetXmlData(story, Fields.Story, manager.Ingest, namespaceManager);

                        var item = new SourceContent(
                            this.Options.DataLocation,
                            code,
                            ContentType.PrintContent,
                            mediaTypeId,
                            contentHash,
                            FormatText(manager, headline),
                            FormatText(manager, summary),
                            FormatText(manager, body),
                            publishedOn)
                        {
                            Page = GetXmlData(story, Fields.Page, manager.Ingest, namespaceManager),
                            Section = GetXmlData(story, Fields.Section, manager.Ingest, namespaceManager),
                            Language = manager.Ingest.GetConfigurationValue("language"),
                            Authors = GetAuthorList(isBylineTitleCase ? ToTitleCase(author) : author),
                            ExternalUid = GetXmlData(story, Fields.Id, manager.Ingest, namespaceManager)
                        };

                        await ImportArticleAsync(manager, item);
                    }
                }
                // This ingest has just completed running for one content item.
                await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);
            }
            catch (Exception ex)
            {
                // Reached limit return to ingest manager.
                if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                    throw new FormatException($"File contents for ingest '{manager.Ingest.Name}' is invalid.", ex);

                this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}', File: {file}", manager.Ingest.Name, path);
                await manager.RecordFailureAsync(ex);
                await manager.SendEmailAsync($"Failed to ingest item for ingest '{manager.Ingest.Name}', File: {path}", ex);
            }
        }
    }

    private IEnumerable<string> GetFileList(IIngestActionManager manager, string dir)
    {
        var directoryExists = Directory.Exists(dir);
        var result = directoryExists ? Directory.GetFiles(dir) : Array.Empty<string>();
        if (!directoryExists) Logger.LogWarning("Directory '{dir}' does not exist for {manager}.", dir, manager.Ingest.Name);
        return result;
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
    private async Task GetFmsArticlesAsync(string dir, IIngestActionManager manager, Dictionary<string, string> sources)
    {
        var fileList = GetFileList(manager, dir);
        var isBylineTitleCase = manager.Ingest.GetConfigurationValue<bool>("bylineTitleCase", false);

        // Iterate over the files in the list and process the stories they contain.
        foreach (var path in fileList)
        {
            try
            {
                // Extract a list of stories from the current document. Replace the story delimiter with a string that
                // facilitates the extraction of stories using regular expressions in single-line mode.
                var text = ReadFileContents(path, manager.Ingest);
                if (text != null)
                {
                    var entries = text.Split(manager.Ingest.GetConfigurationValue(Fields.Item)).Where(t => !String.IsNullOrWhiteSpace(t)).Select(t => t.Trim());

                    // Iterate over the list of stories and add a new item to the articles list for each story.
                    foreach (var entry in entries)
                    {
                        // Single line mode prevents matching on "\n\n", so replace this with a meaningful field delimiter.
                        var papername = GetFmsData(entry, Fields.PaperName, manager.Ingest);
                        var code = GetItemSourceCode(manager.Ingest, papername, sources);
                        var author = GetFmsData(entry, Fields.Author, manager.Ingest);

                        if (!String.IsNullOrEmpty(code)) // This is a valid newspaper source
                        {
                            var mediaTypeId = await GetMediaTypeIdAsync(manager.Ingest, code, sources);
                            var item = new SourceContent(
                                this.Options.DataLocation,
                                code,
                                ContentType.PrintContent,
                                mediaTypeId,
                                GetFmsData(entry, Fields.Id, manager.Ingest),
                                GetFmsData(entry, Fields.Headline, manager.Ingest),
                                GetFmsData(entry, Fields.Summary, manager.Ingest),
                                GetFmsData(entry, Fields.Story, manager.Ingest, "$"),
                                GetPublishedOn(GetFmsData(entry, Fields.Date, manager.Ingest), manager.Ingest, this.Options))
                            {
                                Page = GetFmsData(entry, Fields.Page, manager.Ingest),
                                Section = GetFmsData(entry, Fields.Section, manager.Ingest),
                                Language = manager.Ingest.GetConfigurationValue("language"),

                                Labels = GetLabelList(entry, manager.Ingest),
                                Authors = GetAuthorList(isBylineTitleCase ? ToTitleCase(author) : author),
                            };

                            await ImportArticleAsync(manager, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Reached limit return to ingest manager.
                if (manager.Ingest.FailedAttempts + 1 >= manager.Ingest.RetryLimit)
                    throw new FormatException($"File contents for ingest '{manager.Ingest.Name}' is invalid.", ex);

                this.Logger.LogError(ex, "Failed to ingest item for ingest '{name}', File: {file}", manager.Ingest.Name, path);
                await manager.RecordFailureAsync(ex);
                await manager.SendEmailAsync($"Failed to ingest item for ingest '{manager.Ingest.Name}', File: {path}", ex);
            }
        }
    }

    /// <summary>
    /// Get the "mediaTypeId" value for the current source. If the ingest is selfPublished (relates to only one publication) the sources
    /// dictionary will be empty and the mediaTypeId for the ingest will be returned. If sources is not empty the source for the "code"
    /// parameter will be retrieved and if the source's override mediaTypeId is not null it will be returned.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="code"></param>
    /// <param name="sources"></param>
    /// <returns></returns>
    private async Task<int> GetMediaTypeIdAsync(IngestModel ingest, string code, Dictionary<string, string> sources)
    {
        if (sources.Count == 0) // Self published
        {
            return ingest.MediaTypeId;
        }
        else
        {
            var source = await this.Api.GetSourceForCodeAsync(code);
            if (source == null)
            {
                return ingest.MediaTypeId;
            }
            else
            {
                return source.MediaTypeId ?? ingest.MediaTypeId;
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
            return ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' Paper name '{paperName}' is missing source code.");
        }
        else
        {
            if (sources.TryGetValue(paperName, out string? code))
            {
                if (String.IsNullOrWhiteSpace(code))
                    return ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' Paper name '{paperName}' is missing source code.");
                return code;
            }
            else
            {
                var source = ingest.GetConfigurationValue("defaultSource");
                this.Logger.LogWarning($"Ingest '{ingest.Name}' Paper name '{paperName}' does not have a mapped source.");
                if (String.IsNullOrWhiteSpace(source))
                    return ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' Paper name '{paperName}' is missing source code.");
                return source;
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
    private Dictionary<string, string> GetSourcesForIngest(IngestModel ingest)
    {
        var sources = new Dictionary<string, string>();
        var sourcesStr = ingest.GetConfigurationValue(Fields.Sources);
        if (!string.IsNullOrEmpty(sourcesStr))
        {
            var sourcesArr = sourcesStr.Split("&");
            foreach (var source in sourcesArr)
            {
                var pair = source.Split('=');
                if (pair.Length == 2 && !sources.ContainsKey(pair[0]))
                {
                    sources.Add(pair[0], pair[1]);
                }
                else
                {
                    this.Logger.LogError("Invalid source value in ingest configuration. Source '{source}' for ingest '{ingestName}'", source, ingest.Name);
                }
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
    ///     "fixBlacksXML" - The BCNG files regularly are corrupt and are missing the bottom of the XML.
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
        if (xmlTxt != null)
        {
            // BCNG files regularly are corrupt and are missing the bottom of the XML.
            var fixBlacks = ingest.GetConfigurationValue<bool>(Fields.FixBlacksXml);
            if (fixBlacks)
                xmlTxt = FixBlacksNewsgroupXml(filePath, xmlTxt);

            // BCNG files have multiple top-level objects which need to be wrapped in a single pair of tags.
            var addParent = ingest.GetConfigurationValue<bool>(Fields.AddParent);
            if (addParent)
                xmlTxt = FixParentTag(xmlTxt);

            // BCNG stories contain invalid XHTML content which must be escaped to parse the document.
            var escapeContent = ingest.GetConfigurationValue<bool>(Fields.EscapeContent);
            if (escapeContent)
                xmlTxt = StoryToCdata(xmlTxt, ingest);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlTxt);
            return xmlDoc;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Fix the Blacks Newsgroup XML issue.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="xml"></param>
    /// <returns></returns>
    private string FixBlacksNewsgroupXml(string filePath, string xml)
    {
        // Files regularly have the 'last' story corrupting in the <story> node.
        // This results in the whole file being invalid.
        // Check for and fix the common failure.
        if (!_storyTag.Match(xml).Success)
        {
            xml = $"{xml}</story><date>{DateTime.Now:MM-dd-yyyy}</date></bcng>";

            var match = _storyBody.Match(xml);
            var find = match.Groups["body"].Value;
            if (match.Success && !String.IsNullOrWhiteSpace(find))
            {
                xml = xml.Replace(find, this.Options.FailedStoryBody);
            }

            File.WriteAllText(filePath, xml);
            this.Logger.LogError("The file '{path}' is missing data", filePath);
        }

        return xml;
    }

    /// <summary>
    /// Reads content files at the specified path and returns it as a string.
    /// Uses the UDE library to detect the file encoding, supporting various encodings (UTF-8, Windows-1252, ISO-8859-1, etc.).
    /// Automatically converts non-UTF-8 encoded file contents to UTF-8.
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="ingest">The ingest model</param>
    /// <returns>The file content as a string, or null if processing fails</returns>
    private string? ReadFileContents(string filePath, IngestModel ingest)
    {
        this.Logger.LogDebug("Reading file '{file}' for ingest '{name}'", filePath, ingest.Name);

        try
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            return GetFileContent(fileBytes, filePath);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error reading file '{file}' for ingest '{name}'", filePath, ingest.Name);
            throw;
        }
    }

    /// <summary>
    /// Get the file content based on the file bytes and file path.
    /// Use the UDE library to detect the file encoding and read the file content based on the detected encoding.
    /// If the encoding cannot be detected, UTF-8 is used as a fallback option (previous behavior).
    /// </summary>
    /// <param name="fileBytes">The byte array of the file</param>
    /// <param name="filePath">The file path</param>
    /// <returns>The file content as a string, or null if processing fails</returns>
    private string? GetFileContent(byte[] fileBytes, string filePath)
    {
        var detector = new CharsetDetector();
        detector.Feed(fileBytes, 0, fileBytes.Length);
        detector.DataEnd();

        if (detector.Charset != null)
        {
            return ReadWithDetectedEncoding(fileBytes, detector.Charset, filePath);
        }
        else
        {
            return ReadWithFallbackEncoding(fileBytes, filePath);
        }
    }

    /// <summary>
    /// Use the detected encoding to read the file content.
    /// If reading fails, use UTF-8 as a fallback option.
    /// </summary>
    /// <param name="fileBytes">The byte array of the file</param>
    /// <param name="charset">The detected character set</param>
    /// <param name="filePath">The file path</param>
    /// <returns>The file content as a string, or null if processing fails</returns>
    private string? ReadWithDetectedEncoding(byte[] fileBytes, string charset, string filePath)
    {
        try
        {
            var detectedEncoding = Encoding.GetEncoding(charset);
            string content = detectedEncoding.GetString(fileBytes);
            this.Logger.LogDebug("Successfully read file '{file}' with detected encoding {encoding}", filePath, charset);

            return CheckContent(content, filePath);
        }
        catch (Exception ex)
        {
            // we don't want this to throw an exception, just log a warning and fallback to UTF-8
            this.Logger.LogWarning("Failed to use detected encoding {encoding}: {error}, falling back to UTF-8", charset, ex.Message);
            return ReadWithFallbackEncoding(fileBytes, filePath);
        }
    }

    /// <summary>
    /// Use UTF-8 encoding to read the file content.
    /// </summary>
    /// <param name="fileBytes">The byte array of the file</param>
    /// <param name="filePath">The file path</param>
    /// <returns>The file content as a string</returns>
    private string? ReadWithFallbackEncoding(byte[] fileBytes, string filePath)
    {
        string content = Encoding.UTF8.GetString(fileBytes);
        this.Logger.LogWarning("Could not detect encoding for '{file}', defaulting to UTF-8", filePath);
        return CheckContent(content, filePath);
    }

    /// <summary>
    /// Check if the file content is empty and log a warning if it is.
    /// </summary>
    /// <param name="content">The file content</param>
    /// <param name="filePath">The file path</param>
    /// <returns>The file content if it is not empty, otherwise null</returns>
    private string? CheckContent(string content, string filePath)
    {
        if (String.IsNullOrWhiteSpace(content))
        {
            this.Logger.LogError("Missing file contents at '{path}'", filePath);
            return null;
        }
        return content;
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
        // Remove "by " prefix (exactly 3 characters, case insensitive) from the beginning of the author string
        var cleanedAuthor = author?.Trim() ?? "";
        if (cleanedAuthor.Length >= 3 &&
            cleanedAuthor.Substring(0, 3).Equals("by ", StringComparison.OrdinalIgnoreCase))
        {
            cleanedAuthor = cleanedAuthor.Substring(3).Trim();
        }

        var authors = new List<Author>
            {
                new Author(cleanedAuthor, "", "")
            };
        return authors;
    }

    /// <summary>
    /// Get the published date for the story in Universal Time.
    /// </summary>
    /// <param name="dateStr"></param>
    /// <param name="ingest"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static DateTime GetPublishedOn(string dateStr, IngestModel ingest, FileMonitorOptions options)
    {
        var dateFmt = ingest.GetConfigurationValue(Fields.DateFmt);
        if (!String.IsNullOrEmpty(dateFmt))
        {
            var dateTime = DateTime.ParseExact(dateStr, dateFmt, CultureInfo.InvariantCulture);
            return LocalizeForIngestTimezone(dateTime, ingest, options);
        }
        else
        {
            return DateTime.Now;
        }
    }

    /// <summary>
    /// Calculate the UTC time at midnight in the ingest's configured timezone. This is required to set
    /// the correct date for publishedOn, with a time component of "00:00:00".
    /// </summary>
    /// <param name="date"></param>
    /// <param name="ingest"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static DateTime LocalizeForIngestTimezone(DateTime date, IngestModel ingest, FileMonitorOptions options)
    {
        var timeZone = ingest.Configuration.GetDictionaryJsonValue<string>("timeZone") ?? options.TimeZone;
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        var offset = tz.GetUtcOffset(date).Hours;

        // Invert the sign of the offset to add ours if West of Greenwich and subtract if East.
        offset = offset <= 0 ? Math.Abs(offset) : 0 - offset;
        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        return date.AddHours(offset);
    }

    /// <summary>
    /// Check is a string is All Caps excluding the spaces.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>bool</returns>
    private static bool IsAllCaps(string text)
    {
        return text.Where(c => !Char.IsWhiteSpace(c)).All(x => char.IsUpper(x));
    }

    /// <summary>
    /// Remove Special chars from a string replacing with a ' '.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string RemoveSpecial(string text)
    {
        var arr = text.Select(ch =>
            ((ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z')
                || (ch >= '0' && ch <= '9')
                ) ? ch : ' ').ToArray();
        return new string(arr);
    }

    /// <summary>
    /// Convert to Title Case if the text is All Caps.
    /// </summary>
    /// <param name="author"></param>
    /// <returns>bool</returns>
    private static string ToTitleCase(string author)
    {
        var stringCheck = RemoveSpecial(author.Replace("By", "BY"));
        TextInfo tInfo = new CultureInfo("en-US", false).TextInfo;
        return IsAllCaps(stringCheck) ? tInfo.ToTitleCase(author.ToLower()) : author;
    }

    /// <summary>
    /// Get the list of labels for this story. Currently only supported for FMS files.
    /// </summary>
    /// <param name="filtered"></param>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static List<TNO.Kafka.Models.LabelModel> GetLabelList(string filtered, IngestModel ingest)
    {
        var tags = GetFmsData(filtered, Fields.Tags, ingest);
        var labelArray = tags.Split(',');
        var labelList = new List<TNO.Kafka.Models.LabelModel>();
        foreach (string labelValue in labelArray)
        {
            labelList.Add(new TNO.Kafka.Models.LabelModel("", labelValue));
        }

        return labelList;
    }
    #endregion
}
