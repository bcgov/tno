using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportScheduleModel class, provides a model that represents a subscriber to a report.
/// </summary>
public class ReportScheduleModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The name of the event schedule.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The event schedule description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether the event schedule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - The settings for this event schedule.
    /// </summary>
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - When a request was sent to Kafka.
    /// </summary>
    public DateTime? RequestSentOn { get; set; }

    /// <summary>
    /// get/set - The last time this event schedule was run.
    /// </summary>
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set - Foreign key to the schedule
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// get/set - Row version for the schedule.
    /// </summary>
    public long? ScheduleVersion { get; set; }

    /// <summary>
    /// get/set - Number of milliseconds to wait between each execution of the action.
    /// </summary>
    public int DelayMS { get; set; }

    /// <summary>
    /// get/set - The date and time to start executing the action (based on StartAt).
    /// </summary>
    public DateTime? RunOn { get; set; }

    /// <summary>
    /// get/set - The time to start the action.  Use this for a process that should keep running after it stops.
    /// </summary>
    public TimeSpan? StartAt { get; set; }

    /// <summary>
    /// get/set - The time to stop the action.  Use this for a process that should stop running after a period of time.
    /// </summary>
    public TimeSpan? StopAt { get; set; }

    /// <summary>
    /// get/set - Whether this schedule will be deleted after a successful execution.
    /// </summary>
    public bool RunOnlyOnce { get; set; }

    /// <summary>
    /// get/set - Whether the schedule should repeat after each delay, or whether it should not repeat until the next StartAt.
    /// </summary>
    public bool Repeat { get; set; }

    /// <summary>
    /// get/set - The day of the week to run on.
    /// </summary>
    public Entities.ScheduleWeekDay RunOnWeekDays { get; set; } = Entities.ScheduleWeekDay.NA;

    /// <summary>
    /// get/set - The month to run on.
    /// </summary>
    public Entities.ScheduleMonth RunOnMonths { get; set; } = Entities.ScheduleMonth.NA;

    /// <summary>
    /// get/set - The day of the month to run on.
    /// </summary>
    public int DayOfMonth { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who requested this content to be generated.
    /// </summary>
    public int? RequestedById { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportScheduleModel.
    /// </summary>
    public ReportScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an ReportScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ReportScheduleModel(Entities.EventSchedule entity) : base(entity)
    {
        if (entity.Schedule == null) throw new ArgumentException("Argument 'Schedule' is required");

        // Event properties
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.Settings = entity.Settings;
        this.RequestSentOn = entity.RequestSentOn;
        this.LastRanOn = entity.LastRanOn;

        // Schedule properties
        this.ScheduleId = entity.ScheduleId;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.DelayMS = entity.Schedule.DelayMS;
        this.RunOn = entity.Schedule.RunOn;
        this.StartAt = entity.Schedule.StartAt;
        this.StopAt = entity.Schedule.StopAt;
        this.RunOnlyOnce = entity.Schedule.RunOnlyOnce;
        this.Repeat = entity.Schedule.Repeat;
        this.RunOnWeekDays = entity.Schedule.RunOnWeekDays;
        this.RunOnMonths = entity.Schedule.RunOnMonths;
        this.DayOfMonth = entity.Schedule.DayOfMonth;
        this.RequestedById = entity.Schedule.RequestedById;
        this.ScheduleVersion = entity.Schedule.Version;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.EventSchedule(ReportScheduleModel model)
    {
        var schedule = new Entities.Schedule(model.Name, model.DelayMS)
        {
            Id = model.ScheduleId,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            RunOn = model.RunOn,
            StartAt = model.StartAt,
            StopAt = model.StopAt,
            RunOnlyOnce = model.RunOnlyOnce,
            Repeat = model.Repeat,
            RunOnWeekDays = model.RunOnWeekDays,
            RunOnMonths = model.RunOnMonths,
            DayOfMonth = model.DayOfMonth,
            RequestedById = model.RequestedById,
            Version = model.ScheduleVersion ?? 0
        };

        return new Entities.EventSchedule(model.Name, Entities.EventScheduleType.Report, schedule, model.Settings)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            RequestSentOn = model.RequestSentOn,
            LastRanOn = model.LastRanOn,
            Version = model.Version ?? 0
        };
    }
    #endregion
}
