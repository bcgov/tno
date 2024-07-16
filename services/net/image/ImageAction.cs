using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
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
using TNO.Services.Image.Config;

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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageAction(IApiService api, IOptions<ImageOptions> options, ILogger<ImageAction> logger) : base(api, options, logger)
    {
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
    public override async Task<ServiceActionResult> PerformActionAsync<T>(IIngestActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for ingest '{name}'", manager.Ingest.Name);

        // This ingest has just begun running.
        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);

        // Extract the ingest configuration settings.
        var fileNameExp = String.IsNullOrWhiteSpace(manager.Ingest.GetConfigurationValue("fileName")) ? manager.Ingest.Source?.Code : manager.Ingest.GetConfigurationValue("fileName");
        if (String.IsNullOrWhiteSpace(fileNameExp)) throw new ConfigurationException($"Ingest '{manager.Ingest.Name}' is missing a 'fileName'.");

        // TODO: Handle different remote connections.
        // TODO: create new account to access server
        // var remotePath = GetInputPath(manager.Ingest);
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
        using var client = new SftpClient(connectionInfo);
        try
        {
            client.Connect();

            var offset = manager.Ingest.GetConfigurationValue<int>("dateOffset", 0);
            var offsetDate = DateTime.Now.AddDays(offset);
            var date = GetDateTimeForTimeZone(manager.Ingest, offsetDate);
            var pathDateFormat = manager.Ingest.GetConfigurationValue("pathDateFormat");
            var dateFormat = manager.Ingest.GetConfigurationValue("dateFormat", "ddMMyyyy");
            var uppercaseDate = manager.Ingest.GetConfigurationValue<bool>("uppercaseDate", false);
            var pathDateValue = date.ToString(pathDateFormat, CultureInfo.InvariantCulture).Replace(".-", "-"); // Remove the odd period after ddd format.
            var fileDateValue = date.ToString(dateFormat, CultureInfo.InvariantCulture).Replace(".-", "-"); // Remove the odd period after ddd format.
            var deleteOriginal = manager.Ingest.GetConfigurationValue<bool>("deleteOriginal", false);

            remotePath = remotePath.Replace("<date>", pathDateValue).Replace("~/", $"{client.WorkingDirectory}/");

            var datePattern = uppercaseDate ? fileDateValue.ToUpperInvariant() : fileDateValue.ToUpperInvariant();
            var filePattern = fileNameExp.Replace("<date>", datePattern);
            var match = new Regex(filePattern);

            var files = await FetchImagesAsync(client, remotePath);
            files = files.Where(f => match.Match(f.Name).Success);
            this.Logger.LogDebug("{count} files were discovered that match the filter '{filter}'.", files.Count(), filePattern);

            var outputPath = GetOutputPath(manager.Ingest);
            foreach (var file in files)
            {
                // Downloading the file can result in multiple Image Services performing the same task.
                // However, we need to unzip the images before creating a content reference.
                var filePath = await DownloadFileAsync(client, manager.Ingest, remotePath.CombineWith(file.Name), deleteOriginal);

                // If a zip file it needs to be unzipped first.
                if (Path.GetExtension(file.Name).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    UnzipFile(filePath, outputPath, true);
                }
            }

            // Directory gets created as part of unzip action, if it doesnt exist, then no files will be there...
            if (Directory.Exists(outputPath))
            {
                bool isNewSourceContent = false;
                bool isUpdatedSourceContent = false;
                // We're only interested in the files that were copied.
                var localFiles = Directory.GetFiles(outputPath);
                localFiles = localFiles.Where(path => match.Match(path).Success).ToArray();
                foreach (var path in localFiles)
                {
                    var fileName = Path.GetFileName(path);
                    var newReference = CreateContentReference(manager.Ingest, fileName);
                    var reference = await this.FindContentReferenceAsync(manager.Ingest.Source?.Code, newReference.Uid);
                    if (reference == null)
                    {
                        isNewSourceContent = true;
                        reference = await this.Api.AddContentReferenceAsync(newReference);
                        Logger.LogDebug($"Image Ingest : ADD NEW : [{manager.Ingest.IngestType?.Name ?? "Image"} - {manager.Ingest.Name}] : {path}");
                    }
                    else
                    {
                        // if we already have a contentReference, check if this is actually an update to the image
                        if (newReference.PublishedOn > reference.PublishedOn)
                        {
                            isUpdatedSourceContent = true;
                            // update the existing reference published on date with the new date
                            reference.PublishedOn = newReference.PublishedOn;

                            Logger.LogDebug($"Image Ingest : UPDATE : [{manager.Ingest.IngestType?.Name ?? "Image"} - {manager.Ingest.Name}] : {path}");
                        }
                        else
                        {
                            Logger.LogDebug($"Image Ingest : SKIP EXISTING : [{manager.Ingest.IngestType?.Name ?? "Image"} - {manager.Ingest.Name}] : {path}");
                        }
                    }

                    if (isUpdatedSourceContent || (reference != null && reference.Status == (int)WorkflowStatus.InProgress && reference.UpdatedOn?.AddMinutes(5) < DateTime.UtcNow))
                    {
                        // If another process has it in progress only attempt to do an import if it's
                        // more than an 5 minutes old. Assumption is that it is stuck.
                        reference = await UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                    }

                    if (isNewSourceContent || isUpdatedSourceContent)
                    {
                        reference = await FindContentReferenceAsync(reference?.Source, reference?.Uid);
                        if (reference != null)
                            await ContentReceivedAsync(manager, reference, CreateSourceContent(manager.Ingest, reference, fileName));
                    }

                    // This ingest has just imported a story.
                    await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failure to import image");
            // Any failure needs to disconnect.
            if (client.IsConnected)
                client.Disconnect();
            throw;
        }

        return ServiceActionResult.Success;
    }

    /// <summary>
    /// Unzip the file to the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="outputPath"></param>
    /// <param name="deleteZip"></param>
    /// <returns></returns>
    private void UnzipFile(string path, string outputPath, bool deleteZip = false)
    {
        using var archive = ZipFile.OpenRead(path);
        foreach (var entry in archive.Entries)
        {
            var localPath = outputPath.CombineWith(entry.FullName);
            if (!File.Exists(localPath))
                entry.ExtractToFile(localPath);
        }
        archive.Dispose();

        if (deleteZip)
        {
            File.Delete(path);
        };
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetOutputPath(IngestModel ingest)
    {
        // TODO: Handle different destination connections.
        return this.Options.VolumePath.CombineWith(
            ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "",
            $"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/");
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
    private static async Task<IEnumerable<ISftpFile>> FetchImagesAsync(SftpClient client, string remoteFullName)
    {
        // TODO: use private keys in ./keys folder to connect to remote ingest.
        // TODO: Fetch image from source data location.  Only continue if the image exists.
        // TODO: Eventually handle different ingest locations based on config.
        return await Task.Factory.FromAsync<IEnumerable<ISftpFile>>((callback, obj) => client.BeginListDirectory(remoteFullName, callback, obj), client.EndListDirectory, null);
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
    /// <param name="moveFile">Whether to move the file instead of copy.</param>
    /// <returns></returns>
    private async Task<string> DownloadFileAsync(SftpClient client, IngestModel ingest, string pathToFile, bool moveFile = false)
    {
        // Copy image to destination data location.
        // TODO: Eventually handle different destination data locations based on config.
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

            // If move then delete original.
            if (moveFile)
            {
                client.DeleteFile(pathToFile);
            }
        }

        return outputFile;
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
                var hour = String.IsNullOrWhiteSpace(match.Groups["hour"].Value) ? 0 : int.Parse(match.Groups["hour"].Value);
                var minute = String.IsNullOrWhiteSpace(match.Groups["minute"].Value) ? 0 : int.Parse(match.Groups["minute"].Value);
                var second = String.IsNullOrWhiteSpace(match.Groups["second"].Value) ? 0 : int.Parse(match.Groups["second"].Value);
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

        publishedOn = this.ToTimeZone(publishedOn, ingest).ToUniversalTime();

        return new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = Runners.BaseService.GetContentHash(ingest.Source.Code, $"{ingest.IngestType?.Name ?? "Image"} : {ingest.Name}", publishedOn),
            PublishedOn = publishedOn,
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress,
            Metadata = new Dictionary<string, object?> {
                { ContentReferenceMetaDataKeys.IngestSource, ingest.Source!.Code }
            }
        };
    }

    /// <summary>
    /// Send message to kafka with new source content.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="reference"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private SourceContent? CreateSourceContent(IngestModel ingest, ContentReferenceModel reference, string fileName)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var content = new SourceContent(
            this.Options.DataLocation,
            reference.Source,
            contentType,
            ingest.MediaTypeId,
            reference.Uid,
            $"{ingest.Name} Frontpage",
            "",
            "",
            publishedOn.ToUniversalTime(),
            ingest.GetConfigurationValue<bool>("publish"))
        {
            StreamUrl = ingest.GetConfigurationValue("url"),
            FilePath = (ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "")
                .CombineWith($"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}/", fileName),
            Language = ingest.GetConfigurationValue("language")
        };
        return content;
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
