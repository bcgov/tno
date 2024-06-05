using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Actions;
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CaptureAction(IApiService api, IOptions<CaptureOptions> options, ILogger<CaptureAction> logger) : base(api, options, logger)
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
    public override async Task<ServiceActionResult> PerformActionAsync<T>(IIngestActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{name}'", manager.Ingest.Name);

        // This ingest has just started to run
        await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);

        // Each schedule will have its own process.
        foreach (var schedule in GetSchedules(manager.Ingest))
        {
            var process = await GetProcessAsync(manager, schedule);

            var content = CreateContentReference(manager.Ingest, schedule);

            // Override the original action name based on the schedule.
            name = manager.VerifySchedule(schedule) ? "start" : "stop";

            if (name == "start" && !IsRunning(process))
            {
                var reference = await FindContentReferenceAsync(content.Source, content.Uid);
                if (reference == null)
                {
                    await this.Api.AddContentReferenceAsync(content);
                }
                else if (reference.Status != (int)WorkflowStatus.InProgress)
                {
                    // Change back to in progress.
                    reference = await this.UpdateContentReferenceAsync(reference, WorkflowStatus.InProgress);
                }

                // Do not wait for process to exit.
                // This allows multiple schedules and data sources to be running in parallel.
                RunProcess(process);

                // This ingest has just completed running for one content item.
                await manager.UpdateIngestStateFailedAttemptsAsync(manager.Ingest.FailedAttempts);
            }
            else if (name == "stop")
            {
                await StopProcessAsync(process, cancellationToken);
                RemoveProcess(manager, schedule);
                var reference = await FindContentReferenceAsync(content.Source, content.Uid);
                await this.ContentReceivedAsync(manager, reference, CreateSourceContent(process, manager.Ingest, schedule, reference));
            }
        }

        return ServiceActionResult.Success;
    }

    /// <summary>
    /// FFMPEG sends all logs to the error output.  There isn't a way to tell the difference regrettably.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="manager"></param>
    /// <param name="e"></param>
    protected override void OnErrorReceived(object? sender, IIngestActionManager? manager, DataReceivedEventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(e.Data))
        {
            this.Logger.LogInformation("{data}", e.Data);
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
    /// <param name="ingest"></param>
    /// <param name="schedule"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private SourceContent? CreateSourceContent(ICommandProcess process, IngestModel ingest, ScheduleModel schedule, ContentReferenceModel? reference)
    {
        if (reference == null) return null;

        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var file = (string)process.Data["filename"];
        var path = file.Replace(this.Options.VolumePath, "");
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var content = new SourceContent(
            this.Options.DataLocation,
            reference.Source,
            contentType,
            ingest.MediaTypeId,
            reference.Uid,
            $"{schedule.Name} {schedule.StartAt:c}-{schedule.StopAt:c}",
            "",
            "",
            publishedOn.ToUniversalTime())
        {
            StreamUrl = ingest.GetConfigurationValue("url") ?? "",
            FilePath = path?.MakeRelativePath() ?? "",
            Language = ingest.GetConfigurationValue("language") ?? ""
        };
        return content;
    }

    /// <summary>
    /// Check if the clip file contains a video stream.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private async Task<bool> IsVideoAsync(string file)
    {
        var process = new Process();
        process.StartInfo.Verb = $"Stream Type";
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -i '{file}' 2>&1 | grep Video | awk '{{print $0}}' | tr -d ,\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.EnableRaisingEvents = true;
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return !String.IsNullOrWhiteSpace(output);
    }

    /// <summary>
    /// Create a content reference for this capture.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private ContentReferenceModel CreateContentReference(IngestModel ingest, ScheduleModel schedule)
    {
        var today = GetDateTimeForTimeZone(ingest);
        var publishedOn = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, today.Kind);
        if (schedule.StartAt.HasValue)
            publishedOn = publishedOn.Add(schedule.StartAt.Value);
        return new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = Runners.BaseService.GetContentHash(ingest.Source.Code, $"{schedule.Name}:{schedule.Id}", publishedOn),
            PublishedOn = this.ToTimeZone(publishedOn, ingest).ToUniversalTime(),
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress,
            Metadata = new Dictionary<string, object> {
                { ContentReferenceMetaDataKeys.MetadataKeyIngestSource, ingest.Source!.Code }
            }
        };
    }

    /// <summary>
    /// Extract the configuration value from the ingest for the specified 'key'.
    /// If the 'arg' contains {0} then it will be replaced with the value.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="key">The ingest configuration key.</param>
    /// <param name="arg">The FFmpeg argument.</param>
    /// <param name="defaultValue">The default value if none is provided.</param>
    /// <returns>The 'arg' and value or default value.</returns>
    private static string GetArgumentValue(IngestModel ingest, string key, string arg, string defaultValue = "")
    {
        var value = ingest.GetConfigurationValue<string>(key, defaultValue);
        if (String.IsNullOrWhiteSpace(value)) return defaultValue;
        if (arg == "") return $" {value}";
        if (arg.Contains("{0}")) return $" {String.Format(arg, value)}";
        return $" {arg} {value}";
    }

    /// <summary>
    /// Generate the command arguments for the service action.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected override string GenerateCommandArguments(ICommandProcess process, IIngestActionManager manager, ScheduleModel schedule)
    {
        var logLevel = GetArgumentValue(manager.Ingest, "logLevel", "-loglevel", "warning");
        var input = GetInput(manager.Ingest);
        var format = GetOutputArguments(manager.Ingest);
        var output = GetOutput(manager.Ingest, schedule);
        process.Data.Add("filename", output);

        return $"{logLevel}{input}{format} \"{output}\"";
    }

    /// <summary>
    /// Get the input arguments.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    private static string GetInput(IngestModel ingest)
    {
        var url = ingest.GetConfigurationValue("url");
        if (!String.IsNullOrWhiteSpace(url))
        {
            var options = new UriCreationOptions();
            if (!Uri.TryCreate(url, options, out Uri? uri)) throw new ConfigurationException("Ingest connection 'url' is not a valid format.");
            return $" -i {uri.ToString().Replace(" ", "+")}";
        }

        var input = ingest.GetConfigurationValue("input");
        if (!String.IsNullOrWhiteSpace(input))
        {
            return $" -i {input}";
        }

        var videoThreadQueueSize = GetArgumentValue(ingest, "videoThreadQueueSize", "-thread_queue_size");
        var videoInputFormat = GetArgumentValue(ingest, "videoInputFormat", "-f", "v4l2");
        var videoFramerate = GetArgumentValue(ingest, "videoFramerate", "-r");
        var videoArgs = GetArgumentValue(ingest, "videoArgs", "");
        var video = GetArgumentValue(ingest, "videoInput", "-i");

        var audioThreadQueueSize = GetArgumentValue(ingest, "audioThreadQueueSize", "-thread_queue_size");
        var audioInputFormat = GetArgumentValue(ingest, "audioInputFormat", "-f", "alsa");
        var audioChannels = GetArgumentValue(ingest, "audioChannels", "-ac", "2");
        var audioChannelLayout = GetArgumentValue(ingest, "audioChannelLayout", "-channel_layout");
        var audioArgs = GetArgumentValue(ingest, "audioArgs", "");
        var audio = GetArgumentValue(ingest, "audioInput", "-i");

        if (!String.IsNullOrWhiteSpace(video) && !String.IsNullOrWhiteSpace(audio))
        {
            return $"{videoThreadQueueSize}{videoInputFormat}{videoFramerate}{videoArgs}{video}{audioThreadQueueSize}{audioInputFormat}{audioChannels}{audioChannelLayout}{audioArgs}{audio}";
        }
        else if (!String.IsNullOrWhiteSpace(video))
        {
            return $"{videoThreadQueueSize}{videoInputFormat}{videoFramerate}{videoArgs}{video}";
        }
        else if (!String.IsNullOrWhiteSpace(audio))
        {
            return $"{audioThreadQueueSize}{audioInputFormat}{audioChannels}{audioChannelLayout}{audioArgs}{audio}";
        }

        throw new ConfigurationException("An input must be configured");
    }

    /// <summary>
    /// Extract the configuration values and generate an output formatting string.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    private static string GetOutputArguments(IngestModel ingest)
    {
        var outputThreadQueueSize = GetArgumentValue(ingest, "outputThreadQueueSize", "-thread_queue_size");
        var bufSize = GetArgumentValue(ingest, "bufSize", "-bufsize");
        var bufMinRate = GetArgumentValue(ingest, "minRate", "-minrate");
        var bufMaxRate = GetArgumentValue(ingest, "maxRate", "-maxrate");

        var audioEncoder = GetArgumentValue(ingest, "audioEncoder", "-c:a");
        var audioBufSize = GetArgumentValue(ingest, "audioBufferSize", "-b:a");
        var volume = GetArgumentValue(ingest, "volume", "-filter:a 'volume={0}'");

        var videoEncoder = GetArgumentValue(ingest, "videoEncoder", "-c:v");
        var videoBufSize = GetArgumentValue(ingest, "videoBufferSize", "-b:v");
        var scale = GetArgumentValue(ingest, "scale", "-s");
        var pixelFormat = GetArgumentValue(ingest, "pixelFormat", "-pix_fmts");
        var frameRate = GetArgumentValue(ingest, "frameRate", "-r");
        var keyframe = GetArgumentValue(ingest, "keyframe", "-g");
        var preset = GetArgumentValue(ingest, "preset", "-preset");
        var crf = GetArgumentValue(ingest, "crf", "-crf");

        var format = GetArgumentValue(ingest, "outputFormat", "-f");
        var other = GetArgumentValue(ingest, "otherArgs", "");

        return $"{outputThreadQueueSize}{bufSize}{bufMinRate}{bufMaxRate}{audioEncoder}{audioBufSize}{volume}{videoEncoder}{videoBufSize}{scale}{pixelFormat}{frameRate}{keyframe}{preset}{crf}{format}{other}";
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetOutputPath(IngestModel ingest)
    {
        return this.Options.VolumePath.CombineWith(ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "", $"{ingest.Source?.Code}/{GetDateTimeForTimeZone(ingest):yyyy-MM-dd}");
    }

    /// <summary>
    /// Get the file name from the connection settings.
    /// This will generate a unique name for each time it has to start.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    private string GetOutput(IngestModel ingest, ScheduleModel schedule)
    {
        string filename;
        var path = GetOutputPath(ingest);
        Directory.CreateDirectory(path);
        var configuredName = ingest.GetConfigurationValue("fileName");

        if (ingest.PostToKafka())
        {
            filename = String.IsNullOrWhiteSpace(configuredName) ? $"{schedule.Name}.mp3" : configuredName.Replace("{schedule.Name}", schedule.Name);
        }
        else
        {
            // Streams that do not generate content will prepend the created time.
            // This should be the time for the timezone configured for the schedule.
            var now = GetDateTimeForTimeZone(ingest);
            filename = $"{now.Hour:00}-{now.Minute:00}-{now.Second:00}-{(String.IsNullOrWhiteSpace(configuredName) ? $"{schedule.Name}.mp3" : configuredName.Replace("{schedule.Name}", schedule.Name))}";
        }

        var name = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        // If the file already exists, create a new version.
        // This ensures we don't overwrite a prior recording.
        // If multiple services are sharing the same pvc it will result in multiple versions of the same capture.
        var versions = Directory.GetFiles(path, $"{name}*{ext}").Length;
        return path.CombineWith($"{name}{(versions == 0 ? "" : $"-{versions}")}{ext}");
    }
    #endregion
}
