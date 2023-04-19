using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Content;

/// <summary>
/// TimeTrackingModel class, provides a model that represents an time tracking.
/// </summary>
public class TimeTrackingModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Foreign key to user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The number of minutes.
    /// </summary>
    public float Effort { get; set; }

    /// <summary>
    /// get/set - The activity performed.
    /// </summary>
    public string Activity { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TimeTrackingModel.
    /// </summary>
    public TimeTrackingModel() { }

    /// <summary>
    /// Creates a new instance of an TimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TimeTrackingModel(Entities.TimeTracking entity) : base(entity)
    {
        this.Id = entity.Id;
        this.ContentId = entity.ContentId;
        this.UserId = entity.UserId;
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
    public static explicit operator Entities.TimeTracking(TimeTrackingModel model)
    {
        return new Entities.TimeTracking(model.ContentId, model.UserId, model.Effort, model.Activity)
        {
            Id = model.Id,
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
