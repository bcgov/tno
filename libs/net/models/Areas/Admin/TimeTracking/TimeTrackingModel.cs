using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.TimeTracking;

/// <summary>
/// TimeTrackingModel class, provides a model that represents an media type.
/// </summary>
public class TimeTrackingModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
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
    public TimeTrackingModel() {}

    /// <summary>
    /// Creates a new instance of an TimeTrackingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TimeTrackingModel(Entities.TimeTracking entity) : base(entity)
    {
        this.Id = entity.Id;
        this.UserId = entity.UserId;
        this.Effort = entity.Effort;
        this.Activity = entity.Activity;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.TimeTracking(TimeTrackingModel model)
    {
        var entity = new Entities.TimeTracking(model.ContentId, model.UserId, model.Effort, model.Activity)
        {
            Id = model.Id,
            UserId = model.UserId,
            Effort = model.Effort,
            Activity = model.Activity,
            Version = model.Version ?? 0,
        };
        return entity;
    }
    #endregion
}
