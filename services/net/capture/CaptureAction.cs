using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Models.Kafka;
using TNO.Services.Capture.Config;
using TNO.Services.Command;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureAction class, performs the capture ingestion action.
/// Execute ffmpeg command to capture audio/video.
/// </summary>
public class CaptureAction : CommandAction<CaptureOptions>
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The kafka messenger service.
    /// </summary>
    protected IKafkaMessenger Kafka { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureAction, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CaptureAction(IKafkaMessenger kafka, IApiService api, IOptions<CaptureOptions> options, ILogger<CaptureAction> logger) : base(api, options, logger)
    {
        this.Kafka = kafka;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    ///
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync<T>(IDataSourceIngestManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);

        // Each schedule will have its own process.
        foreach (var schedule in GetSchedules(manager.DataSource))
        {
            var process = await GetProcessAsync(manager, schedule);

            var content = CreateContentReference(manager.DataSource, schedule);
            var reference = await this.Api.FindContentReferenceAsync(content.Source, content.Uid);

            // Override the original action name based on the schedule.
            name = manager.VerifySchedule(schedule) ? "start" : "stop";

            if (name == "start" && !IsRunning(process))
            {
                if (reference == null)
                {
                    await this.Api.AddContentReferenceAsync(content);
                }
                else if (reference.WorkflowStatus != (int)WorkflowStatus.InProgress)
                {
                    // Change back to in progress.
                    reference.WorkflowStatus = (int)WorkflowStatus.InProgress;
                    await this.Api.UpdateContentReferenceAsync(reference);
                }

                // Do not wait for process to exit.
                // This allows multiple schedules and data sources to be running in parallel.
                RunProcess(process);
            }
            else if (name == "stop")
            {
                await StopProcessAsync(process, cancellationToken);
                RemoveProcess(manager, schedule);

                if (reference != null)
                {
                    if (manager.DataSource.ContentTypeId > 0)
                    {
                        var messageResult = await SendMessageAsync(process, manager.DataSource, schedule, reference);
                        reference.Partition = messageResult.Partition;
                        reference.Offset = messageResult.Offset;
                    }

                    // Assuming some success at this point, even though a stop command can be called for different reasons.
                    reference.WorkflowStatus = (int)WorkflowStatus.Received;
                    await this.Api.UpdateContentReferenceAsync(reference);
                }
            }
        }
    }

    /// <summary>
    /// Stop the specified process.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task StopProcessAsync(ICommandProcess process, CancellationToken cancellationToken = default)
    {
        var args = process.Process.StartInfo.Arguments;
        this.Logger.LogInformation("Stopping process for command '{args}'", args);
        if (IsRunning(process) && !process.Process.HasExited)
        {
            var writer = process.Process.StandardInput;
            await writer.WriteLineAsync("q");
            await process.Process.WaitForExitAsync(cancellationToken);
        }
        process.Process.Dispose();
    }

    /// <summary>
    /// Send message to kafka with new source content.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="dataSource"></param>
    /// <param name="schedule"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<Confluent.Kafka.DeliveryResult<string, SourceContent>> SendMessageAsync(ICommandProcess process, DataSourceModel dataSource, ScheduleModel schedule, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var file = (string)process.Data["filename"];
        var mediaType = await IsVideoAsync(file) ? SourceMediaType.Video : SourceMediaType.Audio;
        var content = new SourceContent(mediaType, reference.Source, reference.Uid, $"{schedule.Name} {schedule.StartAt:c}-{schedule.StopAt:c}", "", "", publishedOn.ToUniversalTime())
        {
            StreamUrl = dataSource.Parent?.GetConnectionValue("url") ?? "",
            FilePath = GetFilePath(file),
            Language = dataSource.Parent?.GetConnectionValue("language") ?? ""
        };
        var result = await this.Kafka.SendMessageAsync(reference.Topic, content);
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {reference.Source}:{reference.Uid}");
        return result;
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
    /// Check if the clip file contains a video stream.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private async Task<bool> IsVideoAsync(string file)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.Verb = $"Stream Type";
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -i {file} 2>&1 | grep Video | awk '{{print $0}}' | tr -d ,\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.ErrorDataReceived += OnError;
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return !String.IsNullOrWhiteSpace(output);
    }

    /// <summary>
    /// Create a content reference for this capture.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    private ContentReferenceModel CreateContentReference(DataSourceModel dataSource, ScheduleModel schedule)
    {
        var today = GetLocalDateTime(dataSource, DateTime.UtcNow);
        var publishedOn = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Local) + schedule.StartAt;
        return new ContentReferenceModel()
        {
            Source = dataSource.Code,
            Uid = $"{schedule.Name}:{schedule.Id}-{publishedOn:yyyy-MM-dd-hh-mm-ss}",
            PublishedOn = publishedOn?.ToUniversalTime(),
            Topic = dataSource.Topic,
            WorkflowStatus = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Generate the command arguments for the service action.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected override string GenerateCommandArguments(ICommandProcess process, IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        var input = GetInput(manager.DataSource);
        var format = GetFormat(manager.DataSource);
        var volume = GetVolume(manager.DataSource);
        var otherArgs = GetOtherArgs(manager.DataSource);
        var output = GetOutput(manager.DataSource, schedule);
        process.Data.Add("filename", output);

        return $"{input}{volume}{format}{otherArgs} {output}";
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected string GetOutputPath(DataSourceModel dataSource)
    {
        return Path.Combine(this.Options.OutputPath, $"{dataSource.Code}/{GetLocalDateTime(dataSource, DateTime.Now):yyyy-MM-dd}");
    }

    /// <summary>
    /// Get the URL from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetInput(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("url");
        var options = new UriCreationOptions();
        if (!Uri.TryCreate(value, options, out Uri? uri)) throw new InvalidOperationException("Data source connection 'url' is not a valid format.");
        return $"-i {uri.ToString().Replace(" ", "+")}";
    }

    /// <summary>
    /// Get the file name from the connection settings.
    /// This will generate a unique name for each time it has to start.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string GetOutput(DataSourceModel dataSource, ScheduleModel schedule)
    {
        string filename;
        var path = GetOutputPath(dataSource);
        Directory.CreateDirectory(path);
        var configuredName = dataSource.GetConnectionValue("fileName");

        if (dataSource.ContentTypeId > 0)
        {
            filename = String.IsNullOrWhiteSpace(configuredName) ? $"{schedule.Name}.mp3" : configuredName.Replace("{schedule.Name}", schedule.Name);
        }
        else
        {
            // Streams that do not generate content will prepend the created time.
            // This should be the time for the timezone configured for the schedule.
            var now = GetLocalDateTime(dataSource, DateTime.UtcNow);
            filename = $"{now.Hour:00}-{now.Minute:00}-{now.Second:00}-{(String.IsNullOrWhiteSpace(configuredName) ? $"{schedule.Name}.mp3" : configuredName.Replace("{schedule.Name}", schedule.Name))}";
        }

        var name = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        // If the file already exists, create a new version.
        // This ensures we don't overwrite a prior recording.
        // If multiple services are sharing the same pvc it will result in multiple versions of the same capture.
        var versions = Directory.GetFiles(path, $"{name}*{ext}").Length;
        return Path.Combine(path, $"{name}{(versions == 0 ? "" : $"-{versions}")}{ext}");
    }

    /// <summary>
    /// Get the format from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetFormat(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("format");
        return String.IsNullOrWhiteSpace(value) ? "" : $" -f {value}";
    }

    /// <summary>
    /// Get the volume from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetVolume(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("volume");
        return String.IsNullOrWhiteSpace(value) ? "" : $" -filter:a 'volume={value}'";
    }

    /// <summary>
    /// Get the other arguments from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetOtherArgs(DataSourceModel dataSource)
    {
        var value = dataSource.GetConnectionValue("otherArgs");
        return String.IsNullOrWhiteSpace(value) ? "" : $" {value}";
    }
    #endregion
}
