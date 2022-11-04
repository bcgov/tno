using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Kafka.Models;
using TNO.Services.Actions;
using TNO.Services.Image.Config;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using TNO.Core.Exceptions;
using TNO.API.Areas.Kafka.Models;
using System.Text.RegularExpressions;

namespace TNO.Services.Image;

/// <summary>
/// ImageAction class, performs the image ingestion action.
/// Fetch image from ingest location.
/// Send content reference to API.
/// Process image based on configuration.
/// Send message to Kafka.
/// Update content reference status.
/// </summary>
public class ImageAction : IngestAction<ImageOptions>
{
    #region Properties
    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageAction(IApiService api, IOptions<ImageOptions> options, ILogger<ImageAction> logger) : base(api, options)
    {
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
    /// <exception cref="ConfigurationException"></exception>
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);

        // Extract the ingest configuration settings.
        var inputFileName = String.IsNullOrWhiteSpace(manager.Ingest.GetConfigurationValue("fileName")) ? manager.Ingest.Source?.Code : manager.Ingest.GetConfigurationValue("fileName");
        if (String.IsNullOrWhiteSpace(inputFileName)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' is missing a 'fileName'.");

        // TODO: Handle different remote connections.
        // TODO: create new account to access server
        // Extract the source connection configuration settings.
        var remotePath = GetInputPath(manager.Ingest);
        var username = manager.Ingest.SourceConnection?.GetConfigurationValue("username");
        var keyFileName = manager.Ingest.SourceConnection?.GetConfigurationValue("keyFileName");
        var hostname = manager.Ingest.SourceConnection?.GetConfigurationValue("hostname");
        if (String.IsNullOrWhiteSpace(hostname)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'hostname'.");
        if (String.IsNullOrWhiteSpace(username)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'username'.");
        if (String.IsNullOrWhiteSpace(keyFileName)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'keyFileName'.");
        if (String.IsNullOrWhiteSpace(remotePath)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' source connection is missing a 'path'.");

        var sshKeyFile = this.Options.PrivateKeysPath.CombineWith(keyFileName);

        if (File.Exists(sshKeyFile))
        {
            var keyFile = new PrivateKeyFile(sshKeyFile);
            var keyFiles = new[] { keyFile };
            var connectionInfo = new ConnectionInfo(hostname,
                                                username,
                                                new PrivateKeyAuthenticationMethod(username, keyFiles));
            using var client = new SftpClient(connectionInfo);
            client.Connect();
            remotePath = remotePath.Replace("~/", $"{client.WorkingDirectory}/");
            var files = await FetchImagesAsync(client, remotePath);
            files = files.Where(f => f.Name.Contains(inputFileName));
            this.Logger.LogDebug("{count} files were discovered that match the filter '{filter}'.", files.Count(), inputFileName);

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
                    await CopyImageAsync(client, manager.Ingest, remotePath.CombineWith(file.Name));

                    var messageResult = sendMessage ? await SendMessageAsync(manager.Ingest, reference) : null;
                    await UpdateContentReferenceAsync(reference, messageResult);
                }
            }

            client.Disconnect();
        }
        else
        {
            throw new ConfigurationException($"SSH Private key file does not exist: {sshKeyFile}");
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
        return this.Options.VolumePath.CombineWith(ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "", $"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/");
    }

    /// <summary>
    /// Get the path to the source file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetInputPath(IngestModel ingest)
    {
        var currentDate = GetDateTimeForTimeZone(ingest);
        return (ingest.SourceConnection?.GetConfigurationValue("path") ?? "").CombineWith(
                ingest.GetConfigurationValue("path")?.MakeRelativePath() ?? "",
                currentDate.Year.ToString(),
                currentDate.Month.ToString("00"),
                currentDate.Day.ToString("00"));
    }

    /// <summary>
    /// Fetch the image from the remote ingest based on configuration.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static async Task<IEnumerable<SftpFile>> FetchImagesAsync(SftpClient client, string remoteFullName)
    {
        // TODO: use private keys in ./keys folder to connect to remote ingest.
        // TODO: Fetch image from source data location.  Only continue if the image exists.
        // TODO: Eventually handle different ingest locations based on config.
        return await Task.Factory.FromAsync<IEnumerable<SftpFile>>((callback, obj) => client.BeginListDirectory(remoteFullName, callback, obj), client.EndListDirectory, null);
    }

    /// <summary>
    /// Perform image processing based on configuration.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private Task ProcessImageAsync(IngestModel ingest)
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
    private async Task CopyImageAsync(SftpClient client, IngestModel ingest, string pathToFile)
    {
        // Copy image to destination data location.
        // TODO: Eventually handle different destination data locations based on config.
        var outputPath = GetOutputPath(ingest);
        var fileName = Path.GetFileName(pathToFile);
        var outputFile = outputPath.CombineWith(fileName);

        if (!System.IO.File.Exists(outputFile))
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            using var saveFile = File.OpenWrite(outputFile);
            await Task.Factory.FromAsync(client.BeginDownloadFile(pathToFile, saveFile), client.EndDownloadFile);
        }
    }

    /// <summary>
    /// Create a content reference for this clip.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(IngestModel ingest, string filename)
    {
        var publishedOnExpression = ingest.GetConfigurationValue("publishedOnExpression");

        var today = GetDateTimeForTimeZone(ingest);
        DateTime publishedOn;
        if (!String.IsNullOrWhiteSpace(publishedOnExpression))
        {
            try
            {
                var match = Regex.Match(filename, publishedOnExpression, RegexOptions.Singleline);
                var year = String.IsNullOrWhiteSpace(match.Groups["year"].Value) ? today.Year : int.Parse(match.Groups["year"].Value);
                var month = String.IsNullOrWhiteSpace(match.Groups["month"].Value) ? today.Month : int.Parse(match.Groups["month"].Value);
                var day = String.IsNullOrWhiteSpace(match.Groups["day"].Value) ? today.Day : int.Parse(match.Groups["day"].Value);
                var hour = String.IsNullOrWhiteSpace(match.Groups["hour"].Value) ? today.Hour : int.Parse(match.Groups["hour"].Value);
                var minute = String.IsNullOrWhiteSpace(match.Groups["minute"].Value) ? today.Minute : int.Parse(match.Groups["minute"].Value);
                var second = String.IsNullOrWhiteSpace(match.Groups["second"].Value) ? today.Second : int.Parse(match.Groups["second"].Value);
                publishedOn = new DateTime(year, month, day, hour, minute, second, today.Kind);

                // If the published on date is greater than today we will assume it's in the morning.
                if (today < publishedOn) publishedOn = publishedOn.Add(new TimeSpan(0));
            }
            catch (Exception ex)
            {
                // Essentially ignore the error and set the published on date to today.
                this.Logger.LogError(ex, "Regex failed for 'publishedOnExpression': {regex}", publishedOnExpression);
                publishedOn = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
            }
        }
        else
            publishedOn = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
        return new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = $"{filename}",
            PublishedOn = this.ToTimeZone(publishedOn, ingest).ToUniversalTime(),
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
    private async Task<DeliveryResultModel<SourceContent>> SendMessageAsync(IngestModel ingest, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var content = new SourceContent(
            this.Options.DataLocation,
            reference.Source,
            contentType,
            ingest.ProductId,
            reference.Uid,
            $"{ingest.Name} Frontpage",
            "",
            "",
            publishedOn.ToUniversalTime())
        {
            StreamUrl = ingest.GetConfigurationValue("url"),
            FilePath = (ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "")
                .CombineWith($"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/", reference.Uid),
            Language = ingest.GetConfigurationValue("language")
        };
        var result = await this.Api.SendMessageAsync(reference.Topic, content);
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
        var now = GetDateTimeForTimeZone(ingest).TimeOfDay;
        return ingest.IngestSchedules.Where(s =>
            s.Schedule != null &&
            s.Schedule.StopAt != null &&
            s.Schedule.StopAt.Value <= now
        ).Select(s => s.Schedule!);
    }
    #endregion
}
