namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// TimeTrackingModel class, provides a model that represents a time tracking activity.
/// </summary>
public class TimeTrackingModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - Effort in minutes for time tracking .
    /// </summary>
    public float Effort { get; set; }

    /// <summary>
    /// get/set - The activity descriptor for the record Updated/Created.
    /// </summary>
    public string Activity { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TimeTrackingModel.
    /// </summary>
    public TimeTrackingModel()
    {
    }

    /// <summary>
    /// Creates a new instance of an TimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="code"></param>
    public TimeTrackingModel(int userId, float effort, string activity)
    {
        this.UserId = userId;
        this.Effort = effort;
        this.Activity = activity;
    }

    /// <summary>
    /// Creates a new instance of an TimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TimeTrackingModel(Entities.TimeTracking entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.Id;
        this.Effort = entity.Effort;
        this.Activity = entity.Activity;
    }

    /// <summary>
    /// Creates a new instance of an TimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public TimeTrackingModel(TNO.API.Areas.Services.Models.Content.TimeTrackingModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Effort = model.Effort;
        this.Activity = model.Activity;
    }
    #endregion
}
