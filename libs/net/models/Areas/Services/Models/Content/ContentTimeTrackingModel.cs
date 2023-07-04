using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentTimeTrackingModel class, provides a model that represents a time tracking activity.
/// </summary>
public class ContentTimeTrackingModel : AuditColumnsModel
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
    /// Creates a new instance of an ContentTimeTrackingModel.
    /// </summary>
    public ContentTimeTrackingModel()
    {
    }

    /// <summary>
    /// Creates a new instance of an ContentTimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="code"></param>
    public ContentTimeTrackingModel(int userId, float effort, string activity)
    {
        this.UserId = userId;
        this.Effort = effort;
        this.Activity = activity;
    }

    /// <summary>
    /// Creates a new instance of an TimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTimeTrackingModel(Entities.TimeTracking entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.Id;
        this.Effort = entity.Effort;
        this.Activity = entity.Activity;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a TimeTracking object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.TimeTracking ToEntity(long contentId)
    {
        var entity = (Entities.TimeTracking)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.TimeTracking(ContentTimeTrackingModel model)
    {
        return new Entities.TimeTracking(model.ContentId, model.UserId, model.Effort, model.Activity)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
