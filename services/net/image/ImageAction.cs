using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Models.Extensions;
using TNO.Kafka.Models;
using TNO.Services.Actions;
using TNO.Services.Actions.Managers;
using TNO.Services.Image.Config;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using TNO.Core.Exceptions;

namespace TNO.Services.Image;

/// <summary>
/// ImageAction class, performs the image ingestion action.
/// Fetch image from data source location.
/// Send content reference to API.
/// Process image based on configuration.
/// Send message to Kafka.
/// Update content reference status.
/// </summary>
public class ImageAction : IngestAction<ImageOptions>
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The kafka messenger service.
    /// </summary>
    protected IKafkaMessenger Producer { get; private set; }

    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageAction, initializes with specified parameters.
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageAction(IKafkaMessenger producer, IApiService api, IOptions<ImageOptions> options, ILogger<ImageAction> logger) : base(api, options)
    {
        this.Producer = producer;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// Checks if a content reference has already been created for each image before deciding whether to import it or not.
    /// Sends message to kafka if content has been added or updated.
    /// Informs API of content reference status.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{name}'", manager.Ingest.Name);

        // Extract the ingest configuration settings.
        // TODO: Change to regex to allow for more advanced searches.
        var inputFileCode = String.IsNullOrWhiteSpace(manager.Ingest.GetConfigurationValue("code")) ? manager.Ingest.Source?.Code : manager.Ingest.GetConfigurationValue("code");
        if (String.IsNullOrWhiteSpace(inputFileCode)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' is missing a 'code'.");

        // TODO: Handle different remote connections.
        // TODO: create new account to access server
        // Extract the source connection configuration settings.
        var remotePath = manager.Ingest.SourceConnection?.GetConfigurationValue("path");
        var username = manager.Ingest.SourceConnection?.GetConfigurationValue("username");
        var keyFileName = manager.Ingest.SourceConnection?.GetConfigurationValue("keyFileName");
        var hostname = manager.Ingest.SourceConnection?.GetConfigurationValue("hostname");
        if (String.IsNullOrWhiteSpace(hostname)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'hostname'.");
        if (String.IsNullOrWhiteSpace(username)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'username'.");
        if (String.IsNullOrWhiteSpace(keyFileName)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'keyFileName'.");
        if (String.IsNullOrWhiteSpace(remotePath)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'path'.");

        var sshKeyFile = Path.Combine(this.Options.PrivateKeysPath, keyFileName);

        if (File.Exists(sshKeyFile))
        {
            var keyFile = new PrivateKeyFile(sshKeyFile);

            var keyFiles = new[] { keyFile };
            var connectionInfo = new ConnectionInfo(hostname,
                                                    username,
                                                    new PrivateKeyAuthenticationMethod(username, keyFiles));
            using var client = new SftpClient(connectionInfo);
            client.Connect();

            var files = await FetchImage(client, remotePath);
            files = files.Where(f => f.Name.Contains(inputFileCode));

            foreach (var file in files)
            {
                var content = CreateContentReference(manager.Ingest, file.Name);
                var reference = await this.Api.FindContentReferenceAsync(content.Source, content.Uid);

                var sendMessage = manager.Ingest.PostToKafka();
                if (reference == null)
                {
                    reference = await this.Api.AddContentReferenceAsync(content);
                }
                else if (reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(2) < DateTime.UtcNow)
                {
                    // If another process has it in progress only attempt to do an import if it's
                    // more than an 2 minutes old. Assumption is that it is stuck.
                    reference = await this.Api.UpdateContentReferenceAsync(reference);
                }
                else sendMessage = false;

                if (reference != null)
                {
                    await CopyImage(client, manager.Ingest, Path.Combine(remotePath, file.Name));

                    if (sendMessage)
                    {
                        var messageResult = await SendMessageAsync(manager.Ingest, reference);
                        reference.Partition = messageResult.Partition;
                        reference.Offset = messageResult.Offset;
                    }

                    reference.Status = (int)WorkflowStatus.Received;
                    await this.Api.UpdateContentReferenceAsync(reference);
                }
            }

            client.Disconnect();
        }
        else
        {
            this.Logger.LogError("SSH Private key file does not exist: {file}", sshKeyFile);
        }

    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetOutputPath(IngestModel ingest)
    {
        // TODO: Handle different destination connections.
        return Path.Combine(this.Options.VolumePath, ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "", $"{ingest.Source?.Code}/{GetLocalDateTime(ingest, DateTime.Now):yyyy-MM-dd/}");
    }

    /// <summary>
    /// Get the path to the source file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetInputPath(IngestModel ingest)
    {
        return Path.Combine(ingest.SourceConnection?.GetConfigurationValue("path") ?? "", $"{GetLocalDateTime(ingest, DateTime.Now):yyyy/MM/dd/}");
    }


    /// <summary>
    /// Fetch the image from the remote data source based on configuration.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static async Task<IEnumerable<SftpFile>> FetchImage(SftpClient client, string remoteFullName)
    {
        // TODO: use private keys in ./keys folder to connect to remote data source.
        // TODO: Fetch image from source data location.  Only continue if the image exists.
        // TODO: Eventually handle different data source locations based on config.
        return await Task.Factory.FromAsync<IEnumerable<SftpFile>>((callback, obj) => client.BeginListDirectory(remoteFullName, callback, obj), client.EndListDirectory, null);
    }


    /// <summary>
    /// Perform image processing based on configuration.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private Task ProcessImage(IngestModel ingest)
    {
        // TODO: Process Image based on configuration.
        throw new NotImplementedException();
    }


    /// <summary>
    /// Copy image from image server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="ingest"></param>
    /// <param name="pathToFile"></param>
    /// <returns></returns>
    private async Task CopyImage(SftpClient client, IngestModel ingest, string pathToFile)
    {
        // Copy image to destination data location.
        // TODO: Eventually handle different destination data locations based on config.
        var outputPath = GetOutputPath(ingest);
        var fileName = Path.GetFileName(pathToFile);
        var outputFile = Path.Combine(outputPath, fileName);

        if (!System.IO.File.Exists(outputFile))
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            using var saveFile = File.OpenWrite(outputFile);
            var task = Task.Factory.FromAsync(client.BeginDownloadFile(pathToFile, saveFile), client.EndDownloadFile);
            await task;
        }
    }

    /// <summary>
    /// Create a content reference for this clip.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(IngestModel ingest, string filename)
    {
        var today = GetLocalDateTime(ingest, DateTime.UtcNow);
        var publishedOn = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, DateTimeKind.Local);
        return new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = $"{filename}",
            PublishedOn = publishedOn.ToUniversalTime(),
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Send message to kafka with new source content.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<Confluent.Kafka.DeliveryResult<string, SourceContent>> SendMessageAsync(IngestModel ingest, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var content = new SourceContent(reference.Source, contentType, ingest.ProductId, ingest.DestinationConnectionId, reference.Uid, $"{ingest.Name} Frontpage", "", "", publishedOn.ToUniversalTime())
        {
            StreamUrl = ingest.GetConfigurationValue("url"),
            FilePath = Path.Combine(GetOutputPath(ingest), reference.Uid),
            Language = ingest.GetConfigurationValue("language")
        };
        var result = await this.Producer.SendMessageAsync(reference.Topic, content);
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {reference.Source}:{reference.Uid}");
        return result;
    }

    /// <summary>
    /// Only return schedules that have passed and are within the 'ScheduleLimiter' configuration setting.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected IEnumerable<ScheduleModel> GetSchedules(IngestModel ingest)
    {
        var now = GetLocalDateTime(ingest, DateTime.UtcNow).TimeOfDay;
        return ingest.IngestSchedules.Where(s =>
            s.Schedule != null &&
            s.Schedule.StopAt != null &&
            s.Schedule.StopAt.Value <= now
        ).Select(s => s.Schedule!);
    }

    /// <summary>
    /// Convert to timezone and return as local.
    /// Dates should be stored in the timezone of the data source.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime GetLocalDateTime(IngestModel ingest, DateTime date)
    {
        return date.ToTimeZone(IngestActionManager<ImageOptions>.GetTimeZone(ingest, this.Options.TimeZone));
    }

    #endregion
}
