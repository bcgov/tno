using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// IngestSchedule class, provides an entity model that is a many-to-many between a schedule and an ingest.
/// </summary>
[Table("ingest_schedule")]
public class IngestSchedule : AuditColumns, IEquatable<IngestSchedule>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the ingest.
    /// </summary>
    [Column("ingest_id")]
    public int IngestId { get; set; }

    /// <summary>
    /// get/set - The ingest.
    /// </summary>
    public virtual Ingest? Ingest { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to schedule.
    /// </summary>
    [Column("schedule_id")]
    public int ScheduleId { get; set; }

    /// <summary>
    /// get/set - The schedule.
    /// </summary>
    public virtual Schedule? Schedule { get; set; }
    #endregion

    #region Constructors
    protected IngestSchedule() { }

    public IngestSchedule(Ingest ingest, Schedule schedule)
    {
        this.IngestId = ingest?.Id ?? throw new ArgumentNullException(nameof(ingest));
        this.Ingest = ingest;
        this.ScheduleId = schedule?.Id ?? throw new ArgumentNullException(nameof(schedule));
        this.Schedule = schedule;
    }

    public IngestSchedule(int ingestId, int scheduleId)
    {
        this.IngestId = ingestId;
        this.ScheduleId = scheduleId;
    }
    #endregion

    #region Methods
    public bool Equals(IngestSchedule? other)
    {
        if (other == null) return false;
        return this.IngestId == other.IngestId && this.ScheduleId == other.ScheduleId;
    }

    public override bool Equals(object? obj) => Equals(obj as IngestSchedule);
    public override int GetHashCode() => (this.IngestId, this.ScheduleId).GetHashCode();
    #endregion
}
