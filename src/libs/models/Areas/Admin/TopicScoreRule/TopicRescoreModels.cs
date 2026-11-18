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
/// TopicRescoreJobModel class, a bulk rescore (a work order the Event Handler runs) and its progress.
/// </summary>
public class TopicRescoreJobModel
{
    #region Properties
    /// <summary>
    /// get/set - The work order.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - The work order status.
    /// </summary>
    public WorkOrderStatus Status { get; set; }

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
    /// get/set - Content in the range when the rescore started.
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
    /// get/set - Who requested the rescore.
    /// </summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// get/set - When the rescore was requested.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set - When its progress last changed.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicRescoreJobModel.
    /// </summary>
    public TopicRescoreJobModel() { }

    /// <summary>
    /// Creates a new instance of a TopicRescoreJobModel, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="configuration"></param>
    public TopicRescoreJobModel(Entities.WorkOrder workOrder, TNO.API.Areas.Services.Models.TopicScore.TopicRescoreConfigurationModel configuration)
    {
        this.Id = workOrder.Id;
        this.Status = workOrder.Status;
        this.StartOn = configuration.StartOn;
        this.EndOn = configuration.EndOn;
        this.SourceIds = configuration.SourceIds;
        this.Total = configuration.Total;
        this.Processed = configuration.Processed;
        this.Changed = configuration.Changed;
        this.Failed = configuration.Failed;
        this.Error = configuration.Error;
        this.CreatedBy = workOrder.CreatedBy;
        this.CreatedOn = workOrder.CreatedOn;
        this.UpdatedOn = workOrder.UpdatedOn;
    }
    #endregion
}
