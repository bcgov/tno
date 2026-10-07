using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// TopicRescoreJob class, provides a DB model for a bulk recalculation of calculated topic scores
/// over a date range, with its progress.
/// </summary>
[Table("topic_rescore_job")]
public class TopicRescoreJob : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - The job status.
    /// </summary>
    [Column("status")]
    public BackgroundJobStatus Status { get; set; }

    /// <summary>
    /// get/set - Content published on or after this date (inclusive, UTC).
    /// </summary>
    [Column("start_on")]
    public DateTime StartOn { get; set; }

    /// <summary>
    /// get/set - Content published before this date (exclusive, UTC).
    /// </summary>
    [Column("end_on")]
    public DateTime EndOn { get; set; }

    /// <summary>
    /// get/set - Limit the job to these sources. Empty includes every source.
    /// </summary>
    [Column("source_ids")]
    public int[] SourceIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - Content items in the range.
    /// </summary>
    [Column("total")]
    public int Total { get; set; }

    /// <summary>
    /// get/set - Content items processed so far.
    /// </summary>
    [Column("processed")]
    public int Processed { get; set; }

    /// <summary>
    /// get/set - Content items whose calculated score changed.
    /// </summary>
    [Column("changed")]
    public int Changed { get; set; }

    /// <summary>
    /// get/set - Content items that could not be rescored.
    /// </summary>
    [Column("failed")]
    public int Failed { get; set; }

    /// <summary>
    /// get/set - The last error.
    /// </summary>
    [Column("error")]
    public string? Error { get; set; }

    /// <summary>
    /// get/set - When the job started.
    /// </summary>
    [Column("started_on")]
    public DateTime? StartedOn { get; set; }

    /// <summary>
    /// get/set - When the job finished.
    /// </summary>
    [Column("completed_on")]
    public DateTime? CompletedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicRescoreJob object.
    /// </summary>
    protected TopicRescoreJob() { }

    /// <summary>
    /// Creates a new instance of a TopicRescoreJob object, initializes with specified parameters.
    /// </summary>
    /// <param name="startOn"></param>
    /// <param name="endOn"></param>
    /// <param name="sourceIds"></param>
    public TopicRescoreJob(DateTime startOn, DateTime endOn, IEnumerable<int>? sourceIds = null)
    {
        this.StartOn = startOn;
        this.EndOn = endOn;
        this.SourceIds = sourceIds?.Distinct().ToArray() ?? Array.Empty<int>();
        this.Status = BackgroundJobStatus.Pending;
    }
    #endregion
}
