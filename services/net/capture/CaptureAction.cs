using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Models.Extensions;
using TNO.Kafka.Models;
using TNO.Services.Capture.Config;
using TNO.Services.Command;
using System.Diagnostics;
using TNO.Core.Exceptions;
using System.Net;

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
    protected IKafkaMessenger Producer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureAction, initializes with specified parameters.
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CaptureAction(IKafkaMessenger producer, IApiService api, IOptions<CaptureOptions> options, ILogger<CaptureAction> logger) : base(api, options, logger)
    {
        this.Producer = producer;
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
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{name}'", manager.Ingest.Name);

        // Each schedule will have its own process.
        foreach (var schedule in GetSchedules(manager.Ingest))
        {
            var process = await GetProcessAsync(manager, schedule);

            var content = CreateContentReference(manager.Ingest, schedule);
            var reference = await this.Api.FindContentReferenceAsync(content.Source, content.Uid);

            // Override the original action name based on the schedule.
            name = manager.VerifySchedule(schedule) ? "start" : "stop";

            if (name == "start" && !IsRunning(process))
            {
                if (reference == null)
                {
                    await this.Api.AddContentReferenceAsync(content);
                }
                else if (reference.Status != (int)WorkflowStatus.InProgress)
                {
                    // Change back to in progress.
                    reference.Status = (int)WorkflowStatus.InProgress;
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
                    var messageResult = manager.Ingest.PostToKafka() ? await SendMessageAsync(process, manager.Ingest, schedule, reference) : null;
                    await UpdateContentReferenceAsync(reference, messageResult);
                }
            }
        }
    }

    /// <summary>
    /// FFMPEG sends all logs to the error output.  There isn't a way to tell the difference regrettably.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="manager"></param>
    /// <param name="e"></param>
    protected override void OnErrorReceived(object? sender, IIngestServiceActionManager? manager, DataReceivedEventArgs e)
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
    private async Task<Confluent.Kafka.DeliveryResult<string, SourceContent>> SendMessageAsync(ICommandProcess process, IngestModel ingest, ScheduleModel schedule, ContentReferenceModel reference)
    {
        var publishedOn = reference.PublishedOn ?? DateTime.UtcNow;
        var file = (string)process.Data["filename"];
        var path = file.Replace(this.Options.VolumePath, "");
        var contentType = ingest.IngestType?.ContentType ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing ingest content type.");
        var content = new SourceContent(reference.Source, contentType, ingest.ProductId, reference.Uid, $"{schedule.Name} {schedule.StartAt:c}-{schedule.StopAt:c}", "", "", publishedOn.ToUniversalTime())
        {
            StreamUrl = ingest.GetConfigurationValue("url") ?? "",
            FilePath = path?.MakeRelativePath() ?? "",
            Language = ingest.GetConfigurationValue("language") ?? ""
        };
        var result = await this.Producer.SendMessageAsync(reference.Topic, content);
        if (result == null) throw new InvalidOperationException($"Failed to receive result from Kafka for {reference.Source}:{reference.Uid}");
        return result;
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
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += (sender, e) => OnErrorReceived(sender, null, e);
        process.OutputDataReceived += (sender, e) => OnOutputReceived(sender, null, e);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

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
    private ContentReferenceModel CreateContentReference(IngestModel ingest, ScheduleModel schedule)
    {
        var today = GetLocalDateTime(ingest, DateTime.UtcNow);
        var publishedOn = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, today.Kind) + schedule.StartAt;
        return new ContentReferenceModel()
        {
            Source = ingest.Source?.Code ?? throw new InvalidOperationException($"Ingest '{ingest.Name}' is missing source code."),
            Uid = $"{schedule.Name}:{schedule.Id}-{publishedOn:yyyy-MM-dd-hh-mm-ss}",
            PublishedOn = publishedOn?.ToUniversalTime(),
            Topic = ingest.Topic,
            Status = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Generate the command arguments for the service action.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected override string GenerateCommandArguments(ICommandProcess process, IIngestServiceActionManager manager, ScheduleModel schedule)
    {
        var input = GetInput(manager.Ingest);
        var format = GetFormat(manager.Ingest);
        var volume = GetVolume(manager.Ingest);
        var otherArgs = GetOtherArgs(manager.Ingest);
        var output = GetOutput(manager.Ingest, schedule);
        process.Data.Add("filename", output);

        return $"{input}{volume}{format}{otherArgs} \"{output}\"";
    }

    /// <summary>
    /// Get the output path to store the file.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected string GetOutputPath(IngestModel ingest)
    {
        return Path.Combine(this.Options.VolumePath, ingest.DestinationConnection?.GetConfigurationValue("path")?.MakeRelativePath() ?? "", $"{ingest.Source?.Code}/{GetLocalDateTime(ingest, DateTime.Now):yyyy-MM-dd}");
    }

    /// <summary>
    /// Get the URL from the connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetInput(IngestModel ingest)
    {
        var value = ingest.GetConfigurationValue("url");
        var options = new UriCreationOptions();
        if (!Uri.TryCreate(value, options, out Uri? uri)) throw new InvalidOperationException("Ingest connection 'url' is not a valid format.");
        return $"-i {uri.ToString().Replace(" ", "+")}";
    }

    /// <summary>
    /// Get the file name from the connection settings.
    /// This will generate a unique name for each time it has to start.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
            var now = GetLocalDateTime(ingest, DateTime.UtcNow);
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
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetFormat(IngestModel ingest)
    {
        var value = ingest.GetConfigurationValue("format");
        return String.IsNullOrWhiteSpace(value) ? "" : $" -f {value}";
    }

    /// <summary>
    /// Get the volume from the connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetVolume(IngestModel ingest)
    {
        var value = ingest.GetConfigurationValue("volume");
        return String.IsNullOrWhiteSpace(value) ? "" : $" -filter:a 'volume={value}'";
    }

    /// <summary>
    /// Get the other arguments from the connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetOtherArgs(IngestModel ingest)
    {
        var value = ingest.GetConfigurationValue("otherArgs");
        return String.IsNullOrWhiteSpace(value) ? "" : $" {value}";
    }
    #endregion
}
