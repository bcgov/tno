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
    /// get/set  When the notification was sent.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The notification status.
    /// </summary>
    public Entities.NotificationStatus Status { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a notification.
    /// </summary>
    public Dictionary<string, object> Response { get; set; } = new Dictionary<string, object>();
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
    /// <param name="options"></param>
    public NotificationInstanceModel(Entities.NotificationInstance entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.NotificationId = entity.NotificationId;
        this.ContentId = entity.ContentId;
        this.SentOn = entity.SentOn;
        this.Status = entity.Status;
        this.Response = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Response, options) ?? new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a NotificationInstance object.
    /// </summary>
    /// <returns></returns>
    public Entities.NotificationInstance ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.NotificationInstance)this;
        entity.Response = JsonDocument.Parse(JsonSerializer.Serialize(this.Response, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.NotificationInstance(NotificationInstanceModel model)
    {
        var entity = new Entities.NotificationInstance(model.NotificationId, model.ContentId)
        {
            Id = model.Id,
            SentOn = model.SentOn,
            Status = model.Status,
            Response = JsonDocument.Parse(JsonSerializer.Serialize(model.Response)),
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
