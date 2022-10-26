using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Managers;
using TNO.Services.Content.Config;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.API.Areas.Services.Models.DataLocation;
using Renci.SshNet;

namespace TNO.Services.Content;

/// <summary>
/// ContentManager class, provides a Kafka Listener service which imports content from all active topics.
/// </summary>
public class ContentManager : ServiceManager<ContentOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka admin client.
    /// </summary>
    protected IKafkaAdmin KafkaAdmin { get; private set; }

    /// <summary>
    /// get - Kafka message consumer.
    /// </summary>
    protected IKafkaListener<string, SourceContent> Listener { get; private set; }

    /// <summary>
    /// get - Kafka message producer.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaAdmin"></param>
    /// <param name="kafkaListener"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentManager(
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, SourceContent> kafkaListener,
        IKafkaMessenger kafkaMessenger,
        IApiService api,
        IOptions<ContentOptions> options,
        ILogger<ContentManager> logger)
        : base(api, options, logger)
    {
        this.KafkaAdmin = kafkaAdmin;
        this.Producer = kafkaMessenger;
        this.Listener = kafkaListener;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continually poll for updated data source configuration.
    /// When there are topics to listen too it will initialize a Kafka consumer.
    /// When configuration updates result in changes to topics, it will update which topics it is subscribing too.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Listener.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    // TODO: Handle e-tag.
                    var ingest = (await this.Api.GetIngestsAsync()).ToArray();

                    // Listen to every enabled data source with a topic that is configured to produce content.
                    var topics = this.Options.GetContentTopics(ingest
                        .Where(i => i.IsEnabled &&
                            !String.IsNullOrWhiteSpace(i.Topic) &&
                            i.ImportContent())
                        .Select(i => i.Topic).ToArray());

                    // Only include topics that exist.
                    var kafkaTopics = this.KafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics)).ToArray();

                    if (topics.Length > 0)
                    {
                        this.Listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        this.Listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                }
            }

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            // await Thread.Sleep(new TimeSpan(0, 0, 0, delay));
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Creates a new cancellation token.
    /// Create a new thread if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            // Make sure the prior task is cancelled before creating a new one.
            if (_cancelToken?.IsCancellationRequested == false)
                _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();
            _consumer = Task.Run(ListenerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ListenerHandlerAsync()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await this.Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Listener.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>True if the consumer should retry the message.</returns>
    private void ListenerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
                this.Listener.Stop();
        }
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerStopHandler(object sender, EventArgs e)
    {
        if (_consumer != null &&
            !_notRunning.Contains(_consumer.Status) &&
            _cancelToken?.IsCancellationRequested == false)
        {
            this.Logger.LogDebug("Listener thread is being cancelled");
            _cancelToken.Cancel();
        }
    }

    /// <summary>
    /// Import the content.
    /// Copy any file associated with source content.
    /// </summary>
    /// <param name="result"></param>
    /// <returns>Whether to continue with the next message.</returns>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleMessageAsync(ConsumeResult<string, SourceContent> result)
    {
        try
        {
            this.Logger.LogInformation("Importing Content from Topic: {topic}, Uid: {key}", result.Topic, result.Message.Key);
            var model = result.Message.Value;

            // Only add if doesn't already exist.
            var exists = await this.Api.FindContentByUidAsync(model.Uid, model.Source);
            if (exists == null)
            {
                // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
                // TODO: Handle e-tag.
                var source = await this.Api.GetSourceForCodeAsync(model.Source);
                if (model.ProductId == 0)
                {
                    // Messages in Kafka are missing information, replace with best guess.
                    var ingests = await this.Api.GetIngestsForTopicAsync(result.Topic);
                    model.ProductId = ingests.FirstOrDefault()?.ProductId ?? throw new InvalidOperationException($"Unable to find an ingest for the topic '{result.Topic}'");
                }

                var content = new ContentModel()
                {
                    Status = Entities.ContentStatus.Draft, // TODO: Automatically publish based on Data Source config settings.
                    SourceId = source?.Id,
                    OtherSource = model.Source,
                    ContentType = model.ContentType,
                    ProductId = model.ProductId,
                    LicenseId = source?.LicenseId ?? 1,  // TODO: Default license by configuration.
                    SeriesId = null, // TODO: Provide default series from Data Source config settings.
                    OtherSeries = null, // TODO: Provide default series from Data Source config settings.
                    OwnerId = source?.OwnerId,
                    Headline = String.IsNullOrWhiteSpace(model.Title) ? "[TBD]" : model.Title,
                    Uid = model.Uid,
                    Page = "", // TODO: Provide default page from Data Source config settings.
                    Summary = String.IsNullOrWhiteSpace(model.Summary) ? "[TBD]" : StringExtensions.SanitizeContent(model.Summary, "<p(?:\\s[^>]*)?>|</p>", Environment.NewLine),
                    Body = !String.IsNullOrWhiteSpace(model.Body) ? StringExtensions.SanitizeContent(model.Body,  "<p(?:\\s[^>]*)?>|</p>", Environment.NewLine) : model.ContentType == ContentType.Snippet ? "" : StringExtensions.SanitizeContent(model.Summary, "<p(?:\\s[^>]*)?>|</p>", Environment.NewLine),
                    SourceUrl = model.Link,
                    PublishedOn = model.PublishedOn,
                };
                content = await this.Api.AddContentAsync(content);
                this.Logger.LogInformation("Content Imported.  Content ID: {id}, Pub: {published}", content?.Id, content?.PublishedOn);

                // Upload the file to the API.
                if (content != null && !String.IsNullOrWhiteSpace(model.FilePath))
                {
                    // A service needs to know it's context so that it can import files.
                    // Default to the service data location if the source model does not specify.
                    var dataLocation = await this.Api.GetDataLocationAsync(!String.IsNullOrWhiteSpace(model.DataLocation) ? model.DataLocation : this.Options.DataLocation)
                        ?? throw new ConfigurationException("Service data location is not configured correctly");

                    // TODO: It isn't ideal to copy files via the API as large files will block this service.  Need to figure out a more performant process at some point.
                    content = (dataLocation.Connection?.ConnectionType) switch
                    {
                        ConnectionType.NAS => throw new NotImplementedException(),
                        ConnectionType.HTTP => throw new NotImplementedException(),
                        ConnectionType.FTP => throw new NotImplementedException(),
                        ConnectionType.SFTP => throw new NotImplementedException(),
                        ConnectionType.Azure => throw new NotImplementedException(),
                        ConnectionType.AWS => throw new NotImplementedException(),
                        ConnectionType.SSH => await CopyFileWithSSHAsync(dataLocation, model, content),
                        ConnectionType.LocalVolume => await CopyFileFromLocalVolumeAsync(model, content),
                        _ => throw new NotImplementedException("The data location is missing connection information")
                    };

                    // Send a Kafka message to the transcription topic
                    // TODO: Automate transcripts when configured by rules (ingest, source, type).
                    // if (!String.IsNullOrWhiteSpace(this.Options.TranscriptionTopic))
                    // {
                    //     await SendMessageAsync(content!);
                    // }
                }

                if (content != null)
                {
                    // Update the status of the content reference.
                    var reference = await this.Api.FindContentReferenceAsync(content.OtherSource, content.Uid);
                    if (reference != null)
                    {
                        reference.Status = (int)WorkflowStatus.Imported;
                        await this.Api.UpdateContentReferenceAsync(reference);
                    }
                }
            }
            else
            {
                // TODO: Not sure if this should be considered a failure or not.
                // Content shouldn't exist already, it indicates unexpected scenario.
                this.Logger.LogWarning("Content already exists. Content Source: {Source}, UID: {Uid}", model.Source, model.Uid);
            }

            // Successful run clears any errors.
            this.State.ResetFailures();
            _retries = 0;
        }
        catch (HttpClientRequestException ex)
        {
            this.Logger.LogError(ex, "HTTP exception while consuming. {response}", ex.Data["body"] ?? "");
        }
    }

    /// <summary>
    /// Copy the files from the local volume storage to the API.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<ContentModel> CopyFileFromLocalVolumeAsync(SourceContent model, ContentModel content)
    {
        var fullPath = Path.Combine(this.Options.VolumePath, model.FilePath.MakeRelativePath());
        if (File.Exists(fullPath))
        {
            var file = File.OpenRead(fullPath);
            var fileName = Path.GetFileName(fullPath);
            return await this.Api.UploadFileAsync(content.Id, content.Version ?? 0, file, fileName) ?? content;
        }
        else
        {
            // TODO: Not sure if this should be considered a failure or not.
            this.Logger.LogWarning("File not found.  Content ID: {id}, File: {path}", content.Id, fullPath);
        }
        return content;
    }

    /// <summary>
    /// Connect to remote location via SSH and copy files to the API.
    /// </summary>
    /// <param name="dataLocation"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    private async Task<ContentModel> CopyFileWithSSHAsync(DataLocationModel dataLocation, SourceContent model, ContentModel content)
    {
        var connection = dataLocation.Connection ?? throw new ConfigurationException("The data location is missing connection information");
        var hostname = connection.GetConfigurationValue("hostname");
        var username = connection.GetConfigurationValue("username");
        var password = connection.GetConfigurationValue("password");
        var keyFileName = connection.GetConfigurationValue("keyFileName");
        var path = connection.GetConfigurationValue("path");

        AuthenticationMethod authMethod;
        if (!String.IsNullOrWhiteSpace(password))
        {
            authMethod = new PasswordAuthenticationMethod(username, password);
        }
        else if (!String.IsNullOrWhiteSpace(keyFileName))
        {
            var sshKeyFile = Path.Combine(this.Options.PrivateKeysPath, keyFileName);
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

        using var client = new SftpClient(new ConnectionInfo(hostname, username, authMethod));
        client.Connect();
        Stream? file = null;
        try
        {
            var fullPath = Path.Combine(path, model.FilePath.MakeRelativePath()).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (client.Exists(fullPath))
            {
                var fileName = Path.GetFileName(fullPath);
                file = client.OpenRead(fullPath);
                content = await this.Api.UploadFileAsync(content.Id, content.Version ?? 0, file, fileName) ?? content;
            }
            else
            {
                this.Logger.LogWarning("File does not exist in data location '{location}', '{path}'", dataLocation.Name, fullPath);
            }
            return content;
        }
        catch
        {
            this.Logger.LogError("Failed to copy file from remote data location");
            throw;
        }
        finally
        {
            client.Disconnect();
            if (file != null)
                file.Close();
        }
    }

    /// <summary>
    /// Download the file from the remote 'sourcePath' and copy it to the local 'destinationPath'.
    /// Ensures the 'destinationPath' exists.  If it doesn't it will create the folders.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    private static async Task<FileStream> DownloadFileAsync(SftpClient client, string sourcePath, string destinationPath)
    {
        var path = Path.GetDirectoryName(destinationPath);
        if (!String.IsNullOrWhiteSpace(path))
            Directory.CreateDirectory(path);
        using var saveFile = File.OpenWrite(destinationPath);
        await Task.Factory.FromAsync(client.BeginDownloadFile(sourcePath, saveFile), client.EndDownloadFile);
        return saveFile;
    }

    /// <summary>
    /// Send message to kafka with updated transcription.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<DeliveryResult<string, TranscriptRequest>> SendMessageAsync(ContentModel content)
    {
        var result = await this.Producer.SendMessageAsync(this.Options.TranscriptionTopic, new TranscriptRequest(content, "ContentService"));
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {content.OtherSource}:{content.Uid}");
        return result;
    }
    #endregion
}
