using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("schedule")]
public class Schedule : AuditColumns
{
    #region Properties
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("schedule_type")]
    public ScheduleType ScheduleType { get; set; } = ScheduleType.Continuous;

    [Column("delay_ms")]
    public int DelayMS { get; set; }

    [Column("run_on")]
    public DateTime? RunOn { get; set; }

    [Column("start_at")]
    public TimeSpan? StartAt { get; set; }

    [Column("stop_at")]
    public TimeSpan? StopAt { get; set; }

    [Column("repeat")]
    public int Repeat { get; set; }

    [Column("run_on_week_days")]
    public ScheduleWeekDay RunOnWeekDays { get; set; } = ScheduleWeekDay.NA;

    [Column("run_on_months")]
    public ScheduleMonth RunOnMonths { get; set; } = ScheduleMonth.NA;

    [Column("day_of_month")]
    public int DayOfMonth { get; set; }

    public virtual List<DataSource> DataSources { get; set; } = new List<DataSource>();

    public virtual List<DataSourceSchedule> DataSourcesManyToMany { get; set; } = new List<DataSourceSchedule>();
    #endregion

    #region Constructors
    protected Schedule() { }

    public Schedule(string name, ScheduleType scheduleType, int delayMS)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
        this.ScheduleType = scheduleType;
        this.DelayMS = delayMS;
    }
    #endregion
}
