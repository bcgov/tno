using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Entities;
using TNO.Models.Extensions;
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
    ///
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync(IDataSourceIngestManager manager, string? name = null, CancellationToken cancellationToken = default)
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);

        // Each schedule will have its own process.
        foreach (var schedule in GetSchedules(manager.DataSource))
        {
            var process = await GetProcessAsync(manager, schedule);
            var isRunning = IsRunning(process);

            var content = CreateContentReference(manager.DataSource, schedule);
            var reference = await this.Api.FindContentReferenceAsync(content.Source, content.Uid);

            if (name == "start" && !isRunning)
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
                    // Assuming some success at this point, even though a stop command can be called for different reasons.
                    reference.WorkflowStatus = (int)WorkflowStatus.Received;
                    await this.Api.UpdateContentReferenceAsync(reference);
                }
            }
        }
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
            Uid = $"{schedule.Name}-{publishedOn:yyyy-MM-dd-hh-mm-ss}",
            PublishedOn = publishedOn?.ToUniversalTime(),
            Topic = dataSource.Topic,
            WorkflowStatus = (int)WorkflowStatus.InProgress
        };
    }

    /// <summary>
    /// Generate the command for the service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected override string GenerateCommand(IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        var input = GetInput(manager.DataSource);
        var format = GetFormat(manager.DataSource);
        var volume = GetVolume(manager.DataSource);
        var otherArgs = GetOtherArgs(manager.DataSource);
        var output = GetOutput(manager.DataSource, schedule);

        return $"{this.Options.Command} {input}{volume}{format}{otherArgs} {output}";
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
        var path = GetOutputPath(dataSource);
        Directory.CreateDirectory(path);

        var value = dataSource.GetConnectionValue("fileName");
        var filename = String.IsNullOrWhiteSpace(value) ? $"{schedule.Name}.mp3" : $"{schedule.Name}-{value}";
        var name = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        var fullname = $"{schedule.StartAt?.Hours:00}-{schedule.StartAt?.Minutes:00}-{schedule.StartAt?.Seconds:00}-{name}";

        // If the file already exists, create a new version.
        var versions = Directory.GetFiles(path, $"{fullname}*{ext}").Length;
        return Path.Combine(path, $"{fullname}{(versions == 0 ? "" : $"-{versions}")}{ext}");
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
