using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Services.Config;

namespace TNO.Services.Actions.Managers;

/// <summary>
/// IngestActionManager class, provides a way to manage the ingestion process for this ingest.
/// </summary>
public class IngestActionManager<TOptions> : ServiceActionManager<TOptions>, IIngestActionManager
    where TOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get - The ingest managed by this object.
    /// </summary>
    public IngestModel Ingest { get; set; }

    /// <summary>
    /// get - A dictionary of values that can be stored with this manager.
    /// </summary>
    public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

    /// <summary>
    /// get - The API service.
    /// </summary>
    protected IApiService Api { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="api"></param>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public IngestActionManager(
        IngestModel ingest,
        IApiService api,
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        IIngestAction<TOptions> action,
        IOptions<TOptions> options,
        ILogger<IServiceActionManager> logger)
        : base(ches, chesOptions, action, options, logger)
    {
        this.Ingest = ingest;
        this.Api = api;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Make AJAX request for the configured ingest.
    /// Verify that the schedule should run the action.
    /// </summary>
    /// <returns></returns>
    protected override Task<bool> PreRunAsync()
    {
        if (this.Ingest.IsEnabled && (!this.Ingest.IngestSchedules.Any() || this.Ingest.IngestSchedules.All(s => s.Schedule == null)))
        {
            this.Logger.LogWarning($"Ingest [{this.Ingest.Name}] is enabled but has NO schedules.  Ingest will be SKIPPED.");
            return Task.FromResult(false);
        }

        if (this.Ingest.IsEnabled && !this.Ingest.IngestSchedules.Any(s => s.Schedule?.IsEnabled == true))
        {
            this.Logger.LogWarning($"Ingest [{this.Ingest.Name}] is enabled but has NO enabled schedules.  Ingest will be SKIPPED.");
            return Task.FromResult(false);
        }

        var result = VerifyIngest() && this.Ingest.IngestSchedules.Where(s => s.Schedule != null).Any(s => VerifySchedule(s.Schedule!));

        return Task.FromResult(result);
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
    /// Inform ingest of successful run.
    /// </summary>
    /// <returns></returns>
    public override async Task RecordSuccessAsync()
    {
        this.Ingest = await UpdateIngestStateFailedAttemptsAsync();
    }

    /// <summary>
    /// Inform ingest of failure.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public override async Task RecordFailureAsync(Exception? error = null)
    {
        this.Ingest = await UpdateIngestStateFailedAttemptsAsync(this.Ingest.FailedAttempts + 1);
        this.IsRunning = false;
    }

    /// <summary>
    /// Make AJAX request and update the ingest state - Failed Attempts.
    /// </summary>
    /// <param name="failedAttempts"></param>
    /// <param name="lastItemUpdatedDate"></param>
    /// <returns></returns>
    public async Task<IngestModel> UpdateIngestStateFailedAttemptsAsync(int failedAttempts = 0, DateTime? lastItemUpdatedDate = null)
    {
        this.Ingest.LastRanOn = DateTime.UtcNow;
        this.Ingest.FailedAttempts = failedAttempts;
        if (lastItemUpdatedDate != null)
            this.Ingest.CreationDateOfLastItem = lastItemUpdatedDate.Value.ToUniversalTime();
        this.Ingest = await this.Api.UpdateIngestStateAsync(Ingest) ?? this.Ingest;
        return this.Ingest;
    }

    /// <summary>
    /// Verify that the specified ingest ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public virtual bool VerifyIngest()
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
        if (!schedule.IsEnabled) return false;

        return VerifySchedule(GetSourceDateTime(DateTime.Now), schedule);
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
        if (!this.Ingest.IsEnabled || !this.Ingest.IngestSchedules.Any(s => s.Schedule?.IsEnabled == true)) return false;
        return VerifyDelay(date, schedule) &&
            VerifyRepeat(date, schedule) &&
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
            this.Ingest.LastRanOn == null ||
            this.Ingest.ScheduleType == ScheduleType.Daily ||
            this.Ingest.ScheduleType == ScheduleType.Advanced)
            return true;

        var next = this.Ingest.LastRanOn.Value.ToTimeZone(GetTimeZone()).AddMilliseconds(schedule.DelayMS);
        return next <= date;
    }

    /// <summary>
    /// Determine if the schedule allows for the process to run repeatedly within a single day period.
    /// If Repeat = false then it can only run once per day, unless the ingest schedule is continuous.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public virtual bool VerifyRepeat(DateTime date, ScheduleModel schedule)
    {
        var lastRanOn = this.Ingest.LastRanOn;
        if (lastRanOn == null) return true;
        if (schedule.Repeat) return true;
        if (this.Ingest.ScheduleType == ScheduleType.Continuous) return true;
        if (date.Year != lastRanOn.Value.Year || date.Month != lastRanOn.Value.Month || date.Day != lastRanOn.Value.Day) return true;

        return false;
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
        // No limitation imposed by RunON, so always run.
        if (schedule.RunOn == null) return true;

        var scheduled = GetSourceDateTime(schedule.RunOn.Value);

        // RunOn is in the future.
        if (scheduled > date) return false;

        // If RunOn is in the past we are only interested in the time.
        return scheduled.TimeOfDay <= date.TimeOfDay;
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
        if (schedule.StartAt == null) return true;

        // Current time is between start and stop.
        var time = date.TimeOfDay;
        if (schedule.StopAt != null && schedule.StartAt <= time && schedule.StopAt > time) return true;
        else if (schedule.StartAt <= time) return true;

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
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected string GetTimeZone()
    {
        return GetTimeZone(this.Ingest, this.Options.TimeZone);
    }

    /// <summary>
    /// Get the TimeZoneInfo for the current ingest configuration settings.
    /// </summary>
    /// <returns></returns>
    public TimeZoneInfo GetTimeZoneInfo()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(GetTimeZone());
    }

    /// <summary>
    /// Get the timezone arguments from the connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetTimeZone(IngestModel ingest, string defaultTimeZone)
    {
        var value = ingest.GetConfigurationValue("timeZone");
        return String.IsNullOrWhiteSpace(value) ? defaultTimeZone : value;
    }
    #endregion
}
