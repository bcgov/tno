using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.TopicScoreRule;

/// <summary>
/// TopicRescoreRequestModel class, the content a bulk rescore covers.
/// </summary>
public class TopicRescoreRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Content published on or after this date (inclusive).
    /// </summary>
    public DateTime StartOn { get; set; }

    /// <summary>
    /// get/set - Content published before this date (exclusive).
    /// </summary>
    public DateTime EndOn { get; set; }

    /// <summary>
    /// get/set - Limit to these sources; empty includes every source.
    /// </summary>
    public IEnumerable<int> SourceIds { get; set; } = Array.Empty<int>();
    #endregion
}

/// <summary>
/// TopicRescorePreviewModel class, what a bulk rescore would change.
/// </summary>
public class TopicRescorePreviewModel
{
    #region Properties
    /// <summary>
    /// get/set - Content in the range with a calculated score.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// get/set - Content whose calculated score would change. Overridden scores are excluded.
    /// </summary>
    public int Changed { get; set; }
    #endregion
}

/// <summary>
/// TopicRescoreJobModel class, a bulk rescore and its progress.
/// </summary>
public class TopicRescoreJobModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The job status.
    /// </summary>
    public BackgroundJobStatus Status { get; set; }

    /// <summary>
    /// get/set - Content published on or after this date.
    /// </summary>
    public DateTime StartOn { get; set; }

    /// <summary>
    /// get/set - Content published before this date.
    /// </summary>
    public DateTime EndOn { get; set; }

    /// <summary>
    /// get/set - The sources included; empty is every source.
    /// </summary>
    public IEnumerable<int> SourceIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - Content in the range.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// get/set - Content processed so far.
    /// </summary>
    public int Processed { get; set; }

    /// <summary>
    /// get/set - Content whose score changed.
    /// </summary>
    public int Changed { get; set; }

    /// <summary>
    /// get/set - Content that could not be rescored.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// get/set - The last error.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// get/set - Who requested the job.
    /// </summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// get/set - When the job was requested.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set - When the job started.
    /// </summary>
    public DateTime? StartedOn { get; set; }

    /// <summary>
    /// get/set - When the job finished.
    /// </summary>
    public DateTime? CompletedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicRescoreJobModel.
    /// </summary>
    public TopicRescoreJobModel() { }

    /// <summary>
    /// Creates a new instance of a TopicRescoreJobModel, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    public TopicRescoreJobModel(Entities.TopicRescoreJob entity)
    {
        this.Id = entity.Id;
        this.Status = entity.Status;
        this.StartOn = entity.StartOn;
        this.EndOn = entity.EndOn;
        this.SourceIds = entity.SourceIds;
        this.Total = entity.Total;
        this.Processed = entity.Processed;
        this.Changed = entity.Changed;
        this.Failed = entity.Failed;
        this.Error = entity.Error;
        this.CreatedBy = entity.CreatedBy;
        this.CreatedOn = entity.CreatedOn;
        this.StartedOn = entity.StartedOn;
        this.CompletedOn = entity.CompletedOn;
    }
    #endregion
}
