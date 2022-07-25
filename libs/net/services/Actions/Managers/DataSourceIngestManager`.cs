using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Services.Config;

namespace TNO.Services.Actions.Managers;

/// <summary>
/// DataSourceIngestManager class, provides a way to manage the ingestion process for this data source.
/// </summary>
public class DataSourceIngestManager<TOptions> : ServiceActionManager<TOptions>, IDataSourceIngestManager
    where TOptions : IngestServiceOptions
{
    #region Variables
    private readonly IApiService _api;
    #endregion

    #region Propeties
    /// <summary>
    /// get - The data source managed by this object.
    /// </summary>
    public DataSourceModel DataSource { get; private set; }

    /// <summary>
    /// get - A dictionary of values that can be stored with this manager.
    /// </summary>
    public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceIngestManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="api"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    public DataSourceIngestManager(DataSourceModel dataSource, IApiService api, IIngestAction<TOptions> action, IOptions<TOptions> options)
        : base(action, options)
    {
        this.DataSource = dataSource;
        _api = api;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Make AJAX request for the configured data source.
    /// Verify that the schedule should run the action.
    /// </summary>
    /// <returns></returns>
    protected override async Task<bool> PreRunAsync()
    {
        // Always fetch the latest version of the data source and update this manager with it.
        var dataSource = await _api.GetDataSourceAsync(this.DataSource.Code);
        if (dataSource != null)
            this.DataSource = dataSource;

        return VerifyDataSource() && this.DataSource.DataSourceSchedules.Where(s => s.Schedule != null).Any(s => VerifySchedule(s.Schedule!));
    }

    /// <summary>
    /// Get the date and time for the source timezone.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public virtual DateTime GetSourceDateTime(DateTime date)
    {
        return date.ToTimeZone(GetTimeZone());
    }

    /// <summary>
    /// Inform data source of successful run.
    /// </summary>
    /// <returns></returns>
    public override async Task RecordSuccessAsync()
    {
        this.DataSource = await UpdateDataSourceAsync();
    }

    /// <summary>
    /// Inform data source of failure.
    /// </summary>
    /// <returns></returns>
    public override async Task RecordFailureAsync()
    {
        this.DataSource = await UpdateDataSourceAsync(this.DataSource.FailedAttempts + 1);
        this.IsRunning = false;
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
    /// <returns></returns>
    public virtual bool VerifyDataSource()
    {
        return true;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// Make certain the date is valid for the source timezone.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifySchedule(ScheduleModel schedule)
    {
        return VerifySchedule(GetSourceDateTime(DateTime.UtcNow), schedule);
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// Make certain the date is valid for the source timezone.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifySchedule(DateTime date, ScheduleModel schedule)
    {
        if (!this.DataSource.IsEnabled || !this.DataSource.DataSourceSchedules.Any(s => s.Schedule?.IsEnabled == true)) return false;
        return VerifyDelay(date, schedule) &&
            VerifyRunOn(date, schedule) &&
            VerifyStartAt(date, schedule) &&
            VerifyDayOfMonth(date, schedule) &&
            VerifyWeekDay(date, schedule) &&
            VerifyMonth(date, schedule);
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// If the schedule type is 'Daily' or 'Advanced' the DelayMS is used to slow down the process rather than to determine if it should run.
    /// * This will always return true for those schedule types.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyDelay(DateTime date, ScheduleModel schedule)
    {
        if (schedule.DelayMS == 0 ||
            this.DataSource.LastRanOn == null ||
            this.DataSource.ScheduleType == DataSourceScheduleType.Daily ||
            this.DataSource.ScheduleType == DataSourceScheduleType.Advanced)
            return true;

        var next = this.DataSource.LastRanOn.Value.AddMilliseconds(schedule.DelayMS);
        return next <= date.ToUniversalTime();
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// This setting can be used to ensure a service action is only performed after this date has been reached and there-after.
    /// After the date has been reached, it will use the time of day as a limiter.
    /// Note that if the 'RunOn' is set at 8:00AM, then it will ignore any earlier 'StartAt' values.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyRunOn(DateTime date, ScheduleModel schedule)
    {
        // Only run the configured number of times.
        if (schedule.Repeat > 0 && this.RanCounter >= schedule.Repeat) return false;

        // No limitation imposed by RunON, so always run.
        if (schedule.RunOn == null) return true;

        // RunOn is in the future.
        if (schedule.RunOn > date) return false;

        // If RunOn is in the past we are only interested in the time.
        return schedule.RunOn.Value.TimeOfDay <= date.TimeOfDay;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date' and time.
    /// Compares the schedule 'StartAt' and 'StopAt' values with the specified 'date' and time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyStartAt(DateTime date, ScheduleModel schedule)
    {
        if (schedule.ScheduleType == (int)ScheduleType.Continuous && schedule.StartAt == null) return true;

        // Current time is between start and stop.
        var time = date.TimeOfDay;
        if (schedule.StartAt <= time && schedule.StopAt > time) return true;

        return false;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyDayOfMonth(DateTime date, ScheduleModel schedule)
    {
        return schedule.DayOfMonth == 0 || date.Day == schedule.DayOfMonth;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyWeekDay(DateTime date, ScheduleModel schedule)
    {
        if (!schedule.RunOnWeekDays.Any() || (schedule.RunOnWeekDays.Length == 1) && schedule.RunOnWeekDays.Contains((int)ScheduleWeekDay.NA)) return true;

        return date.DayOfWeek switch
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
    /// Determine if the schedule allows for the process to run at the specified 'date'.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyMonth(DateTime date, ScheduleModel schedule)
    {
        if (!schedule.RunOnMonths.Any() || (schedule.RunOnMonths.Length == 1) && schedule.RunOnMonths.Contains((int)ScheduleMonth.NA)) return true;

        return date.Month switch
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

    /// <summary>
    /// Get the timezone arguments from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected string GetTimeZone()
    {
        return GetTimeZone(this.DataSource, this.Options.TimeZone);
    }

    /// <summary>
    /// Get the timezone arguments from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetTimeZone(DataSourceModel dataSource, string defaultTimeZone)
    {
        var value = dataSource.GetConnectionValue("timeZone");
        return String.IsNullOrWhiteSpace(value) ? defaultTimeZone : value;
    }
    #endregion
}
