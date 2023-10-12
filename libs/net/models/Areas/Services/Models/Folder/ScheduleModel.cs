using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.Folder;

/// <summary>
/// ScheduleModel class, provides a model that represents a subscriber to a report.
/// </summary>
public class ScheduleModel : AuditColumnsModel
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
    public ScheduleWeekDay RunOnWeekDays { get; set; } = ScheduleWeekDay.NA;

    /// <summary>
    /// get/set - The month to run on.
    /// </summary>
    public ScheduleMonth RunOnMonths { get; set; } = ScheduleMonth.NA;

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
    /// Creates a new instance of an ScheduleModel.
    /// </summary>
    public ScheduleModel() { }

    /// <summary>
    /// Creates a new instance of an ScheduleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ScheduleModel(Entities.Schedule entity) : base(entity)
    {
        if (entity == null) throw new ArgumentException("Argument 'entity' is required");

        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.DelayMS = entity.DelayMS;
        this.RunOn = entity.RunOn;
        this.StartAt = entity.StartAt;
        this.StopAt = entity.StopAt;
        this.RunOnlyOnce = entity.RunOnlyOnce;
        this.Repeat = entity.Repeat;
        this.RunOnWeekDays = entity.RunOnWeekDays;
        this.RunOnMonths = entity.RunOnMonths;
        this.DayOfMonth = entity.DayOfMonth;
        this.RequestedById = entity.RequestedById;
        this.Version = entity.Version;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Schedule(ScheduleModel model)
    {
        var schedule = new Entities.Schedule(model.Name, model.DelayMS)
        {
            Id = model.Id,
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
            Version = model.Version ?? 0
        };

        return schedule;
    }
    #endregion
}
