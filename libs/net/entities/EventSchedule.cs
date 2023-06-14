using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// EventSchedule class, provides an entity model for an event schedule.
/// An event schedule provides a way to manage when an event occurs and what that event is.
/// </summary>
[Table("event_schedule")]
public class EventSchedule : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - The primary key.
    /// </summary>
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - The name of the schedule.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The schedule description.
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether the schedule is enabled.
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Foreign key to the schedule
    /// </summary>
    [Column("schedule_id")]
    public int ScheduleId { get; set; }

    /// <summary>
    /// get/set - The schedule for this event.
    /// </summary>
    public virtual Schedule? Schedule { get; set; }

    /// <summary>
    /// get/set - The event schedule type.
    /// </summary>
    [Column("event_type")]
    public EventScheduleType EventType { get; set; }

    /// <summary>
    /// get/set - The settings for this event.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The last time this event was run.
    /// </summary>
    [Column("last_ran_on")]
    public DateTime? LastRanOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a EventSchedule object.
    /// </summary>
    protected EventSchedule() { }

    /// <summary>
    /// Creates a new instance of a EventSchedule object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="schedule"></param>
    /// <param name="settings"></param>
    /// <exception cref="ArgumentException"></exception>
    public EventSchedule(string name, EventScheduleType type, Schedule schedule, JsonDocument settings)
        : this(name, type, schedule)
    {
        this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Creates a new instance of a EventSchedule object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="schedule"></param>
    /// <exception cref="ArgumentException"></exception>
    public EventSchedule(string name, EventScheduleType type, Schedule schedule)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
        this.EventType = type;
        this.Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        this.ScheduleId = schedule.Id;
    }

    /// <summary>
    /// Creates a new instance of a EventSchedule object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="scheduleId"></param>
    /// <param name="settings"></param>
    /// <exception cref="ArgumentException"></exception>
    public EventSchedule(string name, EventScheduleType type, int scheduleId, JsonDocument settings)
        : this(name, type, scheduleId)
    {
        this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Creates a new instance of a EventSchedule object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="scheduleId"></param>
    /// <exception cref="ArgumentException"></exception>
    public EventSchedule(string name, EventScheduleType type, int scheduleId)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
        this.EventType = type;
        this.ScheduleId = scheduleId;
    }
    #endregion
}
