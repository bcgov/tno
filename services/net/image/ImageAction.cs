using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Models.Extensions;
using TNO.Models.Kafka;
using TNO.Services.Actions;
using TNO.Services.Actions.Managers;
using TNO.Services.Image.Config;
using Renci.SshNet;
using Renci.SshNet.Sftp;

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
    protected IKafkaMessenger Kafka { get; private set; }

    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageAction, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageAction(IKafkaMessenger kafka, IApiService api, IOptions<ImageOptions> options, ILogger<ImageAction> logger) : base(api, options)
    {
        this.Kafka = kafka;
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
    public override async Task PerformActionAsync<T>(IDataSourceIngestManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);
        // TODO: create new account to access server
        var username = String.IsNullOrEmpty(manager.DataSource.GetConnectionValue("username")) ? this.Options.Username : manager.DataSource.GetConnectionValue("username");
        var filename = String.IsNullOrEmpty(manager.DataSource.GetConnectionValue("filename")) ? this.Options.PrivateKeyFileName : manager.DataSource.GetConnectionValue("filename");
        var hostname = String.IsNullOrEmpty(manager.DataSource.GetConnectionValue("hostname")) ? this.Options.HostName : manager.DataSource.GetConnectionValue("hostname");
        var mountPath = String.IsNullOrEmpty(manager.DataSource.GetConnectionValue("inputpath")) ? this.Options.InputPath : GetInputPath(manager.DataSource);
        var inputFileCode = String.IsNullOrEmpty(manager.DataSource.GetConnectionValue("inputfilecode")) ? manager.DataSource.Code: manager.DataSource.GetConnectionValue("inputfilecode");
        var keyFilePath = Path.Combine(this.Options.PrivateKeysPath, filename);
        if (File.Exists(keyFilePath))
        {
            var keyFile = new PrivateKeyFile(keyFilePath);

            var keyFiles = new[] { keyFile };
            var connectionInfo = new ConnectionInfo(hostname,
                                                    username,
                                                    new PrivateKeyAuthenticationMethod(username, keyFiles));
            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    var files = await FetchImage(client, mountPath);
                    files = files.Where(f => ((f.Name.Contains(inputFileCode))));

                    foreach (var file in files)
                    {
                        var content = CreateContentReference(manager.DataSource, file.Name);
                        var reference = await this.Api.FindContentReferenceAsync(content.Source, content.Uid);

                        // Frontpage Content Type ID 4
                        var sendMessage = manager.DataSource.ContentTypeId == 4;

                        if (reference == null)
                        {
                            reference = await this.Api.AddContentReferenceAsync(content);
                        }
                        else if (reference.WorkflowStatus == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(2) < DateTime.UtcNow)
                        {
                            // If another process has it in progress only attempt to do an import if it's
                            // more than an 2 minutes old. Assumption is that it is stuck.
                            reference = await this.Api.UpdateContentReferenceAsync(reference);
                        }
                        else sendMessage = false;

                        if (reference != null)
                        {
                            await CopyImage(client, manager.DataSource, file.Name);

                            if (sendMessage)
                            {
                                var messageResult = await SendMessageAsync(manager.DataSource, reference);
                                reference.Partition = messageResult.Partition;
                                reference.Offset = messageResult.Offset;
                            }

                            reference.WorkflowStatus = (int)WorkflowStatus.Received;
                            await this.Api.UpdateContentReferenceAsync(reference);
                        }
                    }

                    client.Disconnect();

                }
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.Message);
            }
        }
        else
        {
            this.Logger.LogError("SSH Private key file does not exist");
        }

    }

    /// <summary>
    /// Remove the configured mapping path.
    /// Any pods which need access to this file will need to know the original mapping path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string GetFilePath(string path)
    {
        return path.ReplaceFirst($"{this.Options.OutputPath}{Path.DirectorySeparatorChar}", "")!;
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected string GetOutputPath(DataSourceModel dataSource)
    {
        return Path.Combine(this.Options.OutputPath, $"{dataSource.Code}/{GetLocalDateTime(dataSource, DateTime.Now):yyyy-MM-dd/}");
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected string GetInputPath(DataSourceModel dataSource)
    {
        return Path.Combine(dataSource.GetConnectionValue("inputpath"), $"{GetLocalDateTime(dataSource, DateTime.Now):yyyy/MM/dd/}");
    }


    /// <summary>
    /// Fetch the image from the remote data source based on configuration.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task<IEnumerable<SftpFile>> FetchImage(SftpClient client, string remoteFullName)
    {
        // TODO: use private keys in ./keys folder to connect to remote data source.
        // TODO: Fetch image from source data location.  Only continue if the image exists.
        // TODO: Eventually handle different data source locations based on config.
        return await Task.Factory.FromAsync<IEnumerable<SftpFile>>((callback, obj) => client.BeginListDirectory(remoteFullName, callback, obj), client.EndListDirectory, null);
    }


    /// <summary>
    /// Perform image processing based on configuration.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private Task ProcessImage(DataSourceModel dataSource)
    {
        // TODO: Process Image based on configuration.
        throw new NotImplementedException();
    }


    /// <summary>
    /// Copy image from image server
    /// </summary>
    /// <param name="client"></param>
    /// <param name="dataSource"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task CopyImage(SftpClient client, DataSourceModel dataSource, string fileName)
    {
        // Copy image to destination data location.
        // TODO: Eventually handle different destination data locations based on config.
        var inputPath = GetInputPath(dataSource);
        var inputFile = Path.Combine(inputPath, fileName);
        var outputPath = GetOutputPath(dataSource);
        var outputFile = Path.Combine(outputPath, fileName);
        var inputFileCode = String.IsNullOrEmpty(dataSource.GetConnectionValue("inputfilecode")) ? dataSource.Code : dataSource.GetConnectionValue("inputfilecode");
        
        if (!System.IO.File.Exists(outputFile) && fileName.Contains(inputFileCode))
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            using (var saveFile = File.OpenWrite(outputFile))
            {
                var task = Task.Factory.FromAsync(client.BeginDownloadFile(inputFile, saveFile), client.EndDownloadFile);
                await task;
            }
        }
    }

    /// <summary>
    /// Create a content reference for this clip.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(DataSourceModel dataSource, string filename)
    {
        var today = GetLocalDateTime(dataSource, DateTime.UtcNow);
        var publishedOn = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, DateTimeKind.Local);
        return new ContentReferenceModel()
        {
            Source = dataSource.Code,
            Uid = $"{filename}",
            PublishedOn = publishedOn.ToUniversalTime(),
            Topic = dataSource.Topic,
            WorkflowStatus = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Send message to kafka with new source content.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<Confluent.Kafka.DeliveryResult<string, SourceContent>> SendMessageAsync(DataSourceModel dataSource, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var content = new SourceContent(SourceMediaType.Image, reference.Source, reference.Uid, $"{dataSource.Name} Frontpage", "", "", publishedOn.ToUniversalTime())
        {
            StreamUrl = dataSource.Parent?.GetConnectionValue("url") ?? "",
            FilePath = Path.Combine(GetOutputPath(dataSource), reference.Uid),
            Language = dataSource.Parent?.GetConnectionValue("language") ?? ""
        };
        var result = await this.Kafka.SendMessageAsync(reference.Topic, content);
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {reference.Source}:{reference.Uid}");
        return result;
    }

    /// <summary>
    /// Only return schedules that have passed and are within the 'ScheduleLimiter' configuration setting.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected IEnumerable<ScheduleModel> GetSchedules(DataSourceModel dataSource)
    {
        var now = GetLocalDateTime(dataSource, DateTime.UtcNow).TimeOfDay;
        return dataSource.DataSourceSchedules.Where(s =>
            s.Schedule != null &&
            s.Schedule.StopAt != null &&
            s.Schedule.StopAt.Value <= now
        ).Select(s => s.Schedule!);
    }

    /// <summary>
    /// Convert to timezone and return as local.
    /// Dates should be stored in the timezone of the data source.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime GetLocalDateTime(DataSourceModel dataSource, DateTime date)
    {
        return date.ToTimeZone(DataSourceIngestManager<ImageOptions>.GetTimeZone(dataSource, this.Options.TimeZone));
    }

    #endregion
}