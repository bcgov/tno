using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// Schedule class, provides an entity model for a schedule configuration in the database.
/// This provides a way to control when and how often an action (i.e. ingest) will run.
/// </summary>
[Table("schedule")]
public class Schedule : AuditColumns
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
    /// get/set - the type of schedule.
    /// </summary>
    [Column("schedule_type")]
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Continuous;

    /// <summary>
    /// get/set - Number of milliseconds to wait between each execution of the action.
    /// </summary>
    [Column("delay_ms")]
    public int DelayMS { get; set; }

    /// <summary>
    /// get/set - The date and time to start executing the action (based on StartAt).
    /// </summary>
    [Column("run_on")]
    public DateTime? RunOn { get; set; }

    /// <summary>
    /// get/set - The time to start the action.
    /// </summary>
    [Column("start_at")]
    public TimeSpan? StartAt { get; set; }

    /// <summary>
    /// get/set - The time to stop the action.
    /// </summary>
    [Column("stop_at")]
    public TimeSpan? StopAt { get; set; }

    /// <summary>
    /// get/set - Whether this schedule will be deleted after a successful execution.
    /// </summary>
    [Column("run_only_once")]
    public bool RunOnlyOnce { get; set; }

    /// <summary>
    /// get/set - The number of times to execute the action.  This is only relevant for continuous schedules.
    /// </summary>
    [Column("repeat")]
    public int Repeat { get; set; }

    /// <summary>
    /// get/set - The day of the week to run on.
    /// </summary>
    [Column("run_on_week_days")]
    public ScheduleWeekDay RunOnWeekDays { get; set; } = ScheduleWeekDay.NA;

    /// <summary>
    /// get/set - The month to run on.
    /// </summary>
    [Column("run_on_months")]
    public ScheduleMonth RunOnMonths { get; set; } = ScheduleMonth.NA;

    /// <summary>
    /// get/set - The day of the month to run on.
    /// </summary>
    [Column("day_of_month")]
    public int DayOfMonth { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who requested this content to be generated.
    /// </summary>
    [Column("requested_by_id")]
    public int? RequestedById { get; set; }

    /// <summary>
    /// get/set - The user who requested this content to be generated.
    /// </summary>
    public User? RequestedBy { get; set; }

    /// <summary>
    /// get - Collection of ingests this schedule belongs to.
    /// </summary>
    public virtual List<Ingest> Ingests { get; set; } = new List<Ingest>();

    /// <summary>
    /// get/set - Collection of ingests this schedule belongs to (many-to-many).
    /// </summary>
    public virtual List<IngestSchedule> IngestsManyToMany { get; set; } = new List<IngestSchedule>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Schedule object.
    /// </summary>
    protected Schedule() { }

    /// <summary>
    /// Creates a new instance of a Schedule object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="scheduleType"></param>
    /// <param name="delayMS"></param>
    /// <exception cref="ArgumentException"></exception>
    public Schedule(string name, ScheduleType scheduleType, int delayMS)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
        this.ScheduleType = scheduleType;
        this.DelayMS = delayMS;
    }
    #endregion
}
