using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("data_source_schedule")]
public class DataSourceSchedule : AuditColumns, IEquatable<DataSourceSchedule>
{
    #region Properties
    [Column("data_source_id")]
    public int DataSourceId { get; set; }

    public virtual DataSource? DataSource { get; set; }

    [Column("schedule_id")]
    public int ScheduleId { get; set; }

    public virtual Schedule? Schedule { get; set; }
    #endregion

    #region Constructors
    protected DataSourceSchedule() { }

    public DataSourceSchedule(DataSource dataSource, Schedule schedule)
    {
        this.DataSourceId = dataSource?.Id ?? throw new ArgumentNullException(nameof(dataSource));
        this.DataSource = dataSource;
        this.ScheduleId = schedule?.Id ?? throw new ArgumentNullException(nameof(schedule));
        this.Schedule = schedule;
    }

    public DataSourceSchedule(int dataSourceId, int scheduleId)
    {
        this.DataSourceId = dataSourceId;
        this.ScheduleId = scheduleId;
    }
    #endregion

    #region Methods
    public bool Equals(DataSourceSchedule? other)
    {
        if (other == null) return false;
        return this.DataSourceId == other.DataSourceId && this.ScheduleId == other.ScheduleId;
    }

    public override bool Equals(object? obj) => Equals(obj as DataSourceSchedule);
    public override int GetHashCode() => (this.DataSourceId, this.ScheduleId).GetHashCode();
    #endregion
}
