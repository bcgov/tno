using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Entities;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// DataSourceManager class, provides a way to manage the ingestion process for this data source.
/// </summary>
public class DataSourceManager<TOptions> : IDataSourceManager
    where TOptions : IngestServiceOptions
{
    #region Variables
    private readonly IApiService _api;
    private readonly IIngestAction<TOptions> _action;
    private readonly ILogger _logger;
    #endregion

    #region Propeties
    /// <summary>
    /// get - Whether the current manager is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// get - The number of times this data source process has been run.
    /// </summary>
    public int RanCounter { get; private set; }

    /// <summary>
    /// get - The data source managed by this object.
    /// </summary>
    public DataSourceModel DataSource { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="logger"></param>
    public DataSourceManager(DataSourceModel dataSource, IIngestAction<TOptions> action, IApiService api, ILogger<IDataSourceManager> logger)
    {
        this.DataSource = dataSource;
        _api = api;
        _action = action;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    public async Task RunAsync()
    {
        // Always fetch the latest version of the data source and update this manager with it.
        var dataSource = await _api.GetDataSourceAsync(this.DataSource.Code);
        if (dataSource != null)
            this.DataSource = dataSource;

        var now = DateTime.UtcNow;
        var run = VerifyDataSource(this.DataSource) && this.DataSource.DataSourceSchedules.Where(s => s.Schedule != null).Any(s => VerifySchedule(now, s.Schedule!));
        if (run && !this.IsRunning)
        {
            try
            {

                _logger.LogDebug("Data source '{Code}' ingestion process has started", this.DataSource.Code);

                this.IsRunning = true;

                // Perform configured action.
                await _action.PerformActionAsync(this);

                // Inform data source of run.
                this.DataSource = await RecordSuccessfulRunAsync();

                this.RanCounter++;
            }
            catch
            {
                throw;
            }
            finally
            {
                this.IsRunning = false;
            }
        }
    }

    /// <summary>
    /// Inform data source of successful run.
    /// </summary>
    /// <returns></returns>
    public async Task<DataSourceModel> RecordSuccessfulRunAsync()
    {
        return await UpdateDataSourceAsync();
    }

    /// <summary>
    /// Inform data source of failure.
    /// </summary>
    /// <returns></returns>
    public async Task<DataSourceModel> RecordFailureAsync()
    {
        return await UpdateDataSourceAsync(this.DataSource.FailedAttempts + 1);
    }

    /// <summary>
    /// Make AJAX request and update the data source.
    /// </summary>
    /// <returns></returns>
    private async Task<DataSourceModel> UpdateDataSourceAsync(int failedAttempts = 0)
    {
        this.DataSource.LastRanOn = DateTime.UtcNow;
        this.DataSource.FailedAttempts = failedAttempts;
        return await _api.UpdateDataSourceAsync(this.DataSource) ?? this.DataSource;
    }

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public virtual bool VerifyDataSource(DataSourceModel dataSource)
    {
        return true;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'time'.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifySchedule(DateTime time, ScheduleModel schedule)
    {
        if (!this.DataSource.IsEnabled || !this.DataSource.DataSourceSchedules.Any(s => s.Schedule?.IsEnabled == true)) return false;
        return VerifyDelay(time, schedule) &&
            VerifyRunOn(time, schedule) &&
            VerifyDayOfMonth(time, schedule) &&
            VerifyWeekDay(time, schedule) &&
            VerifyMonth(time, schedule);
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'time'.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyDelay(DateTime time, ScheduleModel schedule)
    {
        if (schedule.DelayMS == 0 || this.DataSource.LastRanOn == null)
            return true;

        var next = this.DataSource.LastRanOn.Value.AddMilliseconds(schedule.DelayMS);
        return next <= time;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'time'.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public bool VerifyRunOn(DateTime time, ScheduleModel schedule)
    {
        // Only run the configured number of times.
        if (schedule.Repeat > 0 && this.RanCounter >= schedule.Repeat) return false;

        // No limitation imposed by RunON, so always run.
        if (schedule.RunOn == null) return true;

        // RunOn is in the future.
        if (schedule.RunOn > time) return false;

        // If RunOn is in the past we are only interested in the time.
        return schedule.RunOn.Value.TimeOfDay <= time.TimeOfDay;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'time'.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public static bool VerifyDayOfMonth(DateTime time, ScheduleModel schedule)
    {
        return schedule.DayOfMonth == 0 || time.Day == schedule.DayOfMonth;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'time'.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public static bool VerifyWeekDay(DateTime time, ScheduleModel schedule)
    {
        if (!schedule.RunOnWeekDays.Any() || (schedule.RunOnWeekDays.Length == 1) && schedule.RunOnWeekDays.Contains((int)ScheduleWeekDay.NA)) return true;

        return time.DayOfWeek switch
        {
            DayOfWeek.Sunday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Sunday),
            DayOfWeek.Monday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Monday),
            DayOfWeek.Tuesday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Tuesday),
            DayOfWeek.Wednesday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Wednesday),
            DayOfWeek.Thursday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Thursday),
            DayOfWeek.Friday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Friday),
            DayOfWeek.Saturday => schedule.RunOnWeekDays.Any(d => d == (int)ScheduleWeekDay.Saturday),
            _ => false,
        };
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'time'.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public static bool VerifyMonth(DateTime time, ScheduleModel schedule)
    {
        if (!schedule.RunOnMonths.Any() || (schedule.RunOnMonths.Length == 1) && schedule.RunOnMonths.Contains((int)ScheduleMonth.NA)) return true;

        return time.Month switch
        {
            1 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.January),
            2 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.February),
            3 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.March),
            4 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.April),
            5 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.May),
            6 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.June),
            7 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.July),
            8 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.August),
            9 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.September),
            10 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.October),
            11 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.November),
            12 => schedule.RunOnMonths.Any(d => d == (int)ScheduleMonth.December),
            _ => false,
        };
    }
    #endregion
}
