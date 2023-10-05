using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Notification;

/// <summary>
/// NotificationModel class, provides a model that represents an notification.
/// </summary>
public class NotificationModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of notification.
    /// </summary>
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// get/set - Whether content must be alerted to be included in this notification (default: true).
    /// </summary>
    public bool RequireAlert { get; set; } = true;

    /// <summary>
    /// get/set - The default filter for this notification.
    /// </summary>
    public Dictionary<string, object> Filter { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set - The settings for this notification.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set - The Razor template to generate the notification.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - When to resend the notification.
    /// </summary>
    public ResendOption Resend { get; set; } = ResendOption.Never;

    /// <summary>
    /// get/set - Foreign key to user who owns this notification.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - Whether this notification is public to all users.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// get - List of users who are subscribed to this notification (many-to-many).
    /// </summary>
    public virtual IEnumerable<UserModel> Subscribers { get; set; } = Array.Empty<UserModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NotificationModel.
    /// </summary>
    public NotificationModel() { }

    /// <summary>
    /// Creates a new instance of an NotificationModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public NotificationModel(Entities.Notification entity, JsonSerializerOptions options) : base(entity)
    {
        this.NotificationType = entity.NotificationType;
        this.RequireAlert = entity.RequireAlert;
        this.Template = entity.Template;
        this.Resend = entity.Resend;
        this.OwnerId = entity.OwnerId;
        this.IsPublic = entity.IsPublic;
        this.Filter = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Filter, options) ?? new Dictionary<string, object>();
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();
        this.Subscribers = entity.SubscribersManyToMany.Where(s => s.User != null).Select(s => new UserModel(s.User!, s.IsSubscribed));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Notification object.
    /// </summary>
    /// <returns></returns>
    public Entities.Notification ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Notification)this;
        entity.Filter = JsonDocument.Parse(JsonSerializer.Serialize(this.Filter, options));
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Notification(NotificationModel model)
    {
        var entity = new Entities.Notification(model.Id, model.Name, model.NotificationType, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            RequireAlert = model.RequireAlert,
            Template = model.Template,
            Resend = model.Resend,
            IsPublic = model.IsPublic,
            Filter = JsonDocument.Parse(JsonSerializer.Serialize(model.Filter)),
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new UserNotification(us.Id, entity.Id)
        {
            IsSubscribed = us.IsSubscribed
        }));

        return entity;
    }
    #endregion
}
