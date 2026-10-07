using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AnalysisJob class, provides a DB model for the queued analysis of one content item. There is one
/// job per content item: new work for the same content updates it. This table, not Kafka, is the
/// source of truth for analysis work; Kafka messages only wake workers.
/// </summary>
[Table("analysis_job")]
public class AnalysisJob : AuditColumns
{
    #region Properties
    /// <summary>get/set - Primary key.</summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>get/set - Foreign key to the content.</summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>get/set - The content.</summary>
    public virtual Content? Content { get; set; }

    /// <summary>get/set - Fingerprint of the analysis inputs the job was scheduled for.</summary>
    [Column("input_hash")]
    public string InputHash { get; set; } = "";

    /// <summary>get/set - Why the content is queued.</summary>
    [Column("reason")]
    public AnalysisJobReason Reason { get; set; }

    /// <summary>get/set - Higher is claimed first; lifecycle work outranks backfill.</summary>
    [Column("priority")]
    public int Priority { get; set; }

    /// <summary>get/set - Foreign key to the backfill that queued the job.</summary>
    [Column("backfill_id")]
    public long? BackfillId { get; set; }

    /// <summary>get/set - The backfill that queued the job.</summary>
    public virtual AnalysisBackfill? Backfill { get; set; }

    /// <summary>get/set - The job status.</summary>
    [Column("status")]
    public AnalysisJobStatus Status { get; set; }

    /// <summary>get/set - Attempts made for the current input.</summary>
    [Column("attempts")]
    public int Attempts { get; set; }

    /// <summary>get/set - When the job becomes due (after the quiet period).</summary>
    [Column("due_on")]
    public DateTime DueOn { get; set; }

    /// <summary>get/set - When a failed attempt may be retried.</summary>
    [Column("next_attempt_on")]
    public DateTime? NextAttemptOn { get; set; }

    /// <summary>get/set - When the current claim lapses and the job returns to the queue.</summary>
    [Column("lease_expires_on")]
    public DateTime? LeaseExpiresOn { get; set; }

    /// <summary>get/set - Incremented by every claim; a submission must carry the current value.</summary>
    [Column("fencing_token")]
    public long FencingToken { get; set; }

    /// <summary>get/set - The worker holding the claim.</summary>
    [Column("claimed_by")]
    public string? ClaimedBy { get; set; }

    /// <summary>get/set - The last failure.</summary>
    [Column("last_error")]
    public string? LastError { get; set; }

    /// <summary>get/set - When the job completed.</summary>
    [Column("completed_on")]
    public DateTime? CompletedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisJob object.
    /// </summary>
    protected AnalysisJob() { }

    /// <summary>
    /// Creates a new instance of an AnalysisJob object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="inputHash"></param>
    /// <param name="reason"></param>
    /// <param name="priority"></param>
    /// <param name="dueOn"></param>
    public AnalysisJob(long contentId, string inputHash, AnalysisJobReason reason, int priority, DateTime dueOn)
    {
        this.ContentId = contentId;
        this.InputHash = inputHash;
        this.Reason = reason;
        this.Priority = priority;
        this.DueOn = dueOn;
        this.Status = AnalysisJobStatus.Pending;
    }
    #endregion
}
