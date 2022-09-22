using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Core.Exceptions;
using TNO.Models.Extensions;
using TNO.Services.Clip.Config;
using TNO.Services.Command;

namespace TNO.Services.Clip;

/// <summary>
/// ClipIngestActionManager class, provides a way to manage the clip ingestion process for this data source.
/// </summary>
public class ClipIngestActionManager : CommandIngestActionManager<ClipOptions>
{
    #region Variables
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClipIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ClipIngestActionManager(IngestModel dataSource, IApiService api, IIngestAction<ClipOptions> action, IOptions<ClipOptions> options, ILogger<ClipIngestActionManager> logger)
        : base(dataSource, api, action, options)
    {
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    public override async Task RunAsync()
    {
        var run = await PreRunAsync();
        if (run)
        {
            try
            {
                this.IsRunning = true;

                await PerformActionAsync("start");

                this.RanCounter++;

                await PostRunAsync();
            }
            catch (Exception ex)
            {
                if ((ex is MissingFileException || (ex is AggregateException && ex.InnerException is MissingFileException)) &&
                    !this.Ingest.GetConfigurationValue<bool>("throwOnMissingFile"))
                {
                    _logger.LogWarning(ex, "File missing for clip service");
                }
                else
                {
                    this.IsRunning = false;
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public override bool VerifyIngest()
    {
        return true;
    }


    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date' and time.
    /// Compares the schedule 'StartAt' and 'StopAt' values with the specified 'date' and time.
    /// The clip service can only run at the end of a 'StopAt' because it needs a file.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public override bool VerifyStartAt(DateTime date, ScheduleModel schedule)
    {
        // Must have a start and stop.
        if (schedule.StartAt == null || schedule.StopAt == null) return false;

        // This provides a way to keep checking if earlier schedules have created their clips.
        // This isn't ideal as it will continue to scan the file directly every single run.
        var keepChecking = bool.Parse(this.Ingest.GetConfigurationValue("keepChecking"));

        // Verify the stop at time has passed, but also before the schedule limiter.
        // The schedule limiter ensures we don't keep trying past schedules.
        var time = date.TimeOfDay;
        var limiter = schedule.StopAt?.Add(this.Options.ScheduleLimiter);
        if (schedule.StopAt <= time && (keepChecking || limiter >= time)) return true;

        return false;
    }
    #endregion
}
