using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.EventSchedule;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
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
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SchedulerManager(
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<SchedulerOptions> options,
        ILogger<SchedulerManager> logger)
        : base(api, chesService, chesOptions, options, logger)
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
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
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
                        var scheduledEvent = await this.Api.GetEventScheduleAsync(ev.Id) ?? throw new NoContentException($"Event does not exist {ev.Id}:{ev.Name}");
                        var eventTypeId = scheduledEvent.EventType switch
                        {
                            EventScheduleType.Report => scheduledEvent.ReportId.ToString(),
                            EventScheduleType.Notification => scheduledEvent.ReportId.ToString(),
                            _ => "",
                        };
                        if (VerifySchedule(scheduledEvent))
                        {
                            this.Logger.LogInformation("Scheduled event '{id}:{name}' is running", eventTypeId, scheduledEvent.Name);

                            if (ev.EventType == EventScheduleType.Report)
                                await GenerateReportRequestAsync(scheduledEvent);
                            else if (ev.EventType == EventScheduleType.Notification)
                                await GenerateNotificationRequestAsync(scheduledEvent);
                            else
                                await GenerateEventScheduleRequestAsync(scheduledEvent);

                            await this.Api.HandleConcurrencyAsync<EventScheduleModel?>(async () =>
                            {
                                scheduledEvent = await this.Api.GetEventScheduleAsync(scheduledEvent.Id) ?? throw new NoContentException($"Event schedule {scheduledEvent.Id}:{scheduledEvent.Name} does not exist.");
                                scheduledEvent.RequestSentOn = DateTime.UtcNow;
                                return await this.Api.UpdateEventScheduleAsync(scheduledEvent, false);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                    await this.SendErrorEmailAsync("Service had an Unexpected Failure", ex);
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

        var now = DateTime.Now.ToTimeZone(this.Options.TimeZone);
        var currentMonth = GetCurrentMonth(now);
        var currentWeekDay = GetDayOfWeek(now);
        var runOn = schedule.RunOn?.ToTimeZone(this.Options.TimeZone);

        if (!schedule.IsEnabled) return false;

        var requestSentOn = scheduledEvent.RequestSentOn?.ToTimeZone(this.Options.TimeZone);
        var nextRun = requestSentOn?.AddMilliseconds(schedule.DelayMS);

        this.Logger.LogDebug("Scheduled event '{name}' is being verified for '{now}'", scheduledEvent.Name, now);

        // Delay must have expired.
        if (nextRun > now) return false;
        // Only run once per day.
        if (!schedule.Repeat && now.Year == requestSentOn?.Year && now.Month == requestSentOn?.Month && now.Day == requestSentOn?.Day) return false;
        // Only run on the specified months.
        if (schedule.RunOnMonths != null && !schedule.RunOnMonths.Contains((int)ScheduleMonth.NA) && !schedule.RunOnMonths.Contains((int)currentMonth)) return false;
        // Only run on the specified week days.
        if (schedule.RunOnWeekDays != null && !schedule.RunOnWeekDays.Contains((int)ScheduleWeekDay.NA) && !schedule.RunOnWeekDays.Contains((int)currentWeekDay)) return false;
        // Only run on the specified day of the month.
        if (schedule.DayOfMonth != 0 && now.Day != schedule.DayOfMonth) return false;
        // Only run on and after the schedule begin process.
        if (runOn != null && runOn > now) return false;
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

        var reportId = scheduledEvent.ReportId;
        var requestorId = scheduledEvent.Schedule?.RequestedById;
        var destination = scheduledEvent.Settings.GetDictionaryJsonValue<ReportDestination?>("destination") ?? ReportDestination.ReportingService;
        var reportInstanceId = scheduledEvent.Settings.GetDictionaryJsonValue<long?>("reportInstanceId");
        var assignedId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("assignedId");
        var reportType = scheduledEvent.Settings.GetDictionaryJsonValue<ReportType?>("reportType") ?? ReportType.Content;
        var to = scheduledEvent.Settings.GetDictionaryJsonValue<string>("to") ?? "";
        var data = scheduledEvent.Settings.GetDictionaryJsonValue<object?>("data") ?? new { };
        var autoSend = scheduledEvent.Settings.GetDictionaryJsonValue<bool?>("autoSend") ?? false;

        if (reportId == null || reportId == 0) throw new InvalidOperationException($"Event schedule configuration must have a valid report {scheduledEvent.Id}:{scheduledEvent.Name}");
        var request = new ReportRequestModel(destination, reportType, reportId.Value, JsonDocument.Parse(JsonSerializer.Serialize(data)))
        {
            EventScheduleId = scheduledEvent.Id,
            ReportInstanceId = reportInstanceId,
            RequestorId = requestorId,
            AssignedId = assignedId,
            To = to,
            GenerateInstance = true,
            SendToSubscribers = autoSend,
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

        var notificationId = scheduledEvent.NotificationId;
        var requestorId = scheduledEvent.Schedule?.RequestedById;
        var destination = scheduledEvent.Settings.GetDictionaryJsonValue<NotificationDestination?>("destination") ?? NotificationDestination.NotificationService;
        var contentId = scheduledEvent.Settings.GetDictionaryJsonValue<long?>("contentId");
        var assignedId = scheduledEvent.Settings.GetDictionaryJsonValue<int?>("assignedId");
        var to = scheduledEvent.Settings.GetDictionaryJsonValue<string>("to") ?? "";
        var data = scheduledEvent.Settings.GetDictionaryJsonValue<object?>("data") ?? new { };

        var request = new NotificationRequestModel(destination, data)
        {
            EventScheduleId = scheduledEvent.Id,
            NotificationId = notificationId,
            ContentId = contentId,
            RequestorId = requestorId,
            AssignedId = assignedId,
            To = to,
        };
        await this.Api.SendMessageAsync(request);
    }

    /// <summary>
    /// Send message to Kafka to request a generic event to be executed..
    /// </summary>
    /// <param name="scheduledEvent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task GenerateEventScheduleRequestAsync(EventScheduleModel scheduledEvent)
    {
        var data = scheduledEvent.Settings.GetDictionaryJsonValue<object?>("data") ?? new { };
        var request = new EventScheduleRequestModel(scheduledEvent, data) { };
        await this.Api.SendMessageAsync(request);
    }

    private static ScheduleMonth GetCurrentMonth(DateTime date)
    {
        return date.Month switch
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
    }

    private static ScheduleWeekDay GetDayOfWeek(DateTime date)
    {
        return date.DayOfWeek switch
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
    }
    #endregion
}
