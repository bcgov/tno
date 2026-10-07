using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AnalysisBackfill class, provides a DB model for an administrator's analysis of existing content
/// in a date range. The start is inclusive and the end exclusive (UTC).
/// </summary>
[Table("analysis_backfill")]
public class AnalysisBackfill : AuditColumns
{
    #region Properties
    /// <summary>get/set - Primary key.</summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>get/set - The range start, inclusive (UTC).</summary>
    [Column("start_on")]
    public DateTime StartOn { get; set; }

    /// <summary>get/set - The range end, exclusive (UTC).</summary>
    [Column("end_on")]
    public DateTime EndOn { get; set; }

    /// <summary>get/set - The time zone the administrator chose the range in (shown back to them).</summary>
    [Column("time_zone")]
    public string TimeZone { get; set; } = "";

    /// <summary>get/set - Which content date the range applies to.</summary>
    [Column("date_field")]
    public AnalysisBackfillDateField DateField { get; set; }

    /// <summary>get/set - Missing or stale analysis only, or everything.</summary>
    [Column("mode")]
    public AnalysisBackfillMode Mode { get; set; }

    /// <summary>get/set - The backfill status.</summary>
    [Column("status")]
    public BackgroundJobStatus Status { get; set; }

    /// <summary>get/set - Content created after this is left to lifecycle work.</summary>
    [Column("high_water_mark")]
    public DateTime HighWaterMark { get; set; }

    /// <summary>get/set - The last content ID scheduled, so a resume continues after it.</summary>
    [Column("checkpoint_content_id")]
    public long CheckpointContentId { get; set; }

    /// <summary>get/set - Content matching the criteria.</summary>
    [Column("total")]
    public int Total { get; set; }

    /// <summary>get/set - Jobs the backfill queued.</summary>
    [Column("scheduled")]
    public int Scheduled { get; set; }

    /// <summary>get/set - Matching content whose analysis was already current (skipped).</summary>
    [Column("already_current")]
    public int AlreadyCurrent { get; set; }

    /// <summary>get/set - The last error.</summary>
    [Column("error")]
    public string? Error { get; set; }

    /// <summary>get/set - When scheduling finished.</summary>
    [Column("completed_on")]
    public DateTime? CompletedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisBackfill object.
    /// </summary>
    protected AnalysisBackfill() { }

    /// <summary>
    /// Creates a new instance of an AnalysisBackfill object, initializes with specified parameters.
    /// </summary>
    /// <param name="startOn"></param>
    /// <param name="endOn"></param>
    /// <param name="timeZone"></param>
    /// <param name="dateField"></param>
    /// <param name="mode"></param>
    public AnalysisBackfill(DateTime startOn, DateTime endOn, string timeZone, AnalysisBackfillDateField dateField, AnalysisBackfillMode mode)
    {
        this.StartOn = startOn;
        this.EndOn = endOn;
        this.TimeZone = timeZone;
        this.DateField = dateField;
        this.Mode = mode;
        this.Status = BackgroundJobStatus.Pending;
        this.HighWaterMark = DateTime.UtcNow;
    }
    #endregion
}
