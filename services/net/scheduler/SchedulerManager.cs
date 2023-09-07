using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.EventSchedule;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Managers;
using TNO.Services.Scheduler.Config;

namespace TNO.Services.Scheduler;

/// <summary>
/// SchedulerManager class, provides a way to manage the scheduler service.
/// </summary>
public class SchedulerManager : ServiceManager<SchedulerOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SchedulerManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SchedulerManager(
        IApiService api,
        IOptions<SchedulerOptions> options,
        ILogger<SchedulerManager> logger)
        : base(api, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continuous loop fetches event configurations and sends messages to Kafka when scheduled.
    /// TODO: Provide way to horizontally scale service.  Presently only one can run as they would all send the same messages to Kafka otherwise.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    var events = await this.Api.GetEventSchedulesAsync();

                    foreach (var ev in events.Where(e => e.IsEnabled && this.Options.EventTypes.Contains(e.EventType)))
                    {
                        var scheduledEvent = await this.Api.GetEventScheduleAsync(ev.Id) ?? throw new InvalidOperationException($"Event does not exist {ev.Id}:{ev.Name}");
                        if (VerifySchedule(scheduledEvent))
                        {
                            // Generate topic message
                            // Send message to Kafka
                            if (ev.EventType == EventScheduleType.Report)
                                await GenerateReportRequestAsync(scheduledEvent);
                            else if (ev.EventType == EventScheduleType.Notification)
                                await GenerateNotificationRequestAsync(scheduledEvent);

                            // Update even last ran on
                            scheduledEvent.LastRanOn = DateTime.UtcNow;
                            await this.Api.UpdateEventScheduleAsync(scheduledEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                }
            }

            // The delay ensures we don't have a run away thread.
            this.Logger.LogDebug("Service sleeping for {delay} ms", delay);
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Verify if the schedule is expecting the event to fire.
    /// </summary>
    /// <param name="scheduledEvent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private bool VerifySchedule(EventScheduleModel scheduledEvent)
    {
        var schedule = scheduledEvent.Schedule ?? throw new InvalidOperationException($"Schedule is missing from event {scheduledEvent.Id}:{scheduledEvent.Name}");

        var now = DateTime.Now;
        var currentMonth = now.Month switch
        {
            1 => ScheduleMonth.January,
            2 => ScheduleMonth.February,
            3 => ScheduleMonth.March,
            4 => ScheduleMonth.April,
            5 => ScheduleMonth.May,
            6 => ScheduleMonth.June,
            7 => ScheduleMonth.July,
            8 => ScheduleMonth.August,
            9 => ScheduleMonth.September,
            10 => ScheduleMonth.October,
            11 => ScheduleMonth.November,
            12 => ScheduleMonth.December,
            _ => ScheduleMonth.NA,
        };

        var currentWeekDay = now.DayOfWeek switch
        {
            DayOfWeek.Sunday => ScheduleWeekDay.Sunday,
            DayOfWeek.Monday => ScheduleWeekDay.Monday,
            DayOfWeek.Tuesday => ScheduleWeekDay.Tuesday,
            DayOfWeek.Wednesday => ScheduleWeekDay.Wednesday,
            DayOfWeek.Thursday => ScheduleWeekDay.Thursday,
            DayOfWeek.Friday => ScheduleWeekDay.Friday,
            DayOfWeek.Saturday => ScheduleWeekDay.Saturday,
            _ => ScheduleWeekDay.NA,
        };

        if (!schedule.IsEnabled) return false;

        var lastRanOn = scheduledEvent.LastRanOn?.ToTimeZone(this.Options.TimeZone);
        var nextRun = lastRanOn?.AddMilliseconds(schedule.DelayMS);
        // Delay must have expired.
        if (nextRun > now) return false;
        // Only run once per day.
        if (!schedule.Repeat && now.Year == lastRanOn?.Year && now.Month == lastRanOn?.Month && now.Day == lastRanOn?.Day) return false;
        // Only run on the specified months.
        if (schedule.RunOnMonths != null && !schedule.RunOnMonths.Contains((int)ScheduleMonth.NA) && !schedule.RunOnMonths.Contains((int)currentMonth)) return false;
        // Only run on the specified week days.
        if (schedule.RunOnWeekDays != null && !schedule.RunOnWeekDays.Contains((int)ScheduleWeekDay.NA) && !schedule.RunOnWeekDays.Contains((int)currentWeekDay)) return false;
        // Only run on the specified day of the month.
        if (schedule.DayOfMonth != 0 && now.Day != schedule.DayOfMonth) return false;
        // Only run on and after the schedule begin process.
        if (schedule.RunOn != null && schedule.RunOn < now) return false;
        // Only run on and after the start at time.
        if (schedule.StartAt != null && schedule.StartAt > now.TimeOfDay) return false;
        // Only run before the stop time.
        if (schedule.StopAt != null && schedule.StopAt <= now.TimeOfDay) return false;

        return true;
    }

    /// <summary>
    /// Send message to Kafka to request a report.
    /// </summary>
    /// <param name="scheduledEvent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task GenerateReportRequestAsync(EventScheduleModel scheduledEvent)
    {
        if (scheduledEvent.EventType != EventScheduleType.Report) throw new InvalidOperationException("Only report event types allowed");

        var destination = scheduledEvent.Settings.GetDictionaryJsonValue<ReportDestination?>("destination") ?? ReportDestination.ReportingService;
        var reportId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("reportId");
        var reportInstanceId = scheduledEvent.Settings.GetDictionaryJsonValue<long?>("reportInstanceId");
        var requestorId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("requestorId");
        var assignedId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("assignedId");
        var reportType = scheduledEvent.Settings.GetDictionaryJsonValue<ReportType?>("reportType") ?? ReportType.Content;
        var to = scheduledEvent.Settings.GetDictionaryJsonValue<string>("to") ?? "";
        var data = scheduledEvent.Settings.GetDictionaryJsonValue<object?>("data") ?? new { };
        var generateInstance = scheduledEvent.Settings.GetDictionaryJsonValue<bool?>("generateInstance") ?? true;

        if (reportId == null || reportId == 0) throw new InvalidOperationException($"Event schedule configuration must have a valid report {scheduledEvent.Id}:{scheduledEvent.Name}");
        var request = new ReportRequestModel(destination, reportType, reportId.Value, data)
        {
            ReportInstanceId = reportInstanceId,
            RequestorId = requestorId,
            AssignedId = assignedId,
            To = to,
            GenerateInstance = generateInstance
        };
        await this.Api.SendMessageAsync(request);
    }

    /// <summary>
    /// Send message to Kafka to request a notification.
    /// </summary>
    /// <param name="scheduledEvent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task GenerateNotificationRequestAsync(EventScheduleModel scheduledEvent)
    {
        if (scheduledEvent.EventType != EventScheduleType.Notification) throw new InvalidOperationException("Only notification event types allowed");

        var destination = scheduledEvent.Settings.GetDictionaryJsonValue<NotificationDestination?>("destination") ?? NotificationDestination.NotificationService;
        var notificationId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("notificationId");
        var contentId = scheduledEvent.Settings.GetDictionaryJsonValue<long?>("contentId");
        var requestorId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("requestorId");
        var assignedId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("assignedId");
        var to = scheduledEvent.Settings.GetDictionaryJsonValue<string>("to") ?? "";
        var data = scheduledEvent.Settings.GetDictionaryJsonValue<object?>("data") ?? new { };

        var request = new NotificationRequestModel(destination, data)
        {
            NotificationId = notificationId,
            ContentId = contentId,
            RequestorId = requestorId,
            AssignedId = assignedId,
            To = to,
        };
        await this.Api.SendMessageAsync(request);
    }
    #endregion
}
