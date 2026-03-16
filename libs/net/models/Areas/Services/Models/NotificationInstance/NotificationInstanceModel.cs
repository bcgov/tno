using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.NotificationInstance;

/// <summary>
/// NotificationInstanceModel class, provides a model that represents an notification instance.
/// </summary>
public class NotificationInstanceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the notification definition.
    /// </summary>
    public int NotificationId { get; set; }

    /// <summary>
    /// get - Foreign key to the content related to the notification.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who owns this notification.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set  When the notification was sent.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The notification status.
    /// </summary>
    public Entities.NotificationStatus Status { get; set; }

    /// <summary>
    /// get/set - SMTP response for each email sent for this notification instance.
    /// </summary>
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - The compiled subject of the notification.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get - The compiled body of the notification.
    /// </summary>
    public string Body { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NotificationInstanceModel.
    /// </summary>
    public NotificationInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an NotificationInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public NotificationInstanceModel(Entities.NotificationInstance entity) : base(entity)
    {
        this.Id = entity.Id;
        this.NotificationId = entity.NotificationId;
        this.ContentId = entity.ContentId;
        this.OwnerId = entity.OwnerId;
        this.SentOn = entity.SentOn;
        this.Status = entity.Status;
        this.Response = entity.Response;
        this.Subject = entity.Subject;
        this.Body = entity.Body;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.NotificationInstance(NotificationInstanceModel model)
    {
        var entity = new Entities.NotificationInstance(model.NotificationId, model.ContentId, model.OwnerId)
        {
            Id = model.Id,
            SentOn = model.SentOn,
            Status = model.Status,
            Response = model.Response,
            Subject = model.Subject,
            Body = model.Body,
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
