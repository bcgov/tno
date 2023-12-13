using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;
using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models;

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
    /// get/set - The foreign key to the template.
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// get/set - When to resend the notification.
    /// </summary>
    public ResendOption Resend { get; set; } = ResendOption.Never;

    /// <summary>
    /// get/set - Foreign key to user who owns this notification.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Whether this notification is public to all users.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// get/set - Whether this notification is automatically run when the content is indexed.
    /// </summary>
    public bool AlertOnIndex { get; set; }

    /// <summary>
    /// get/set - The default filter for this notification.
    /// </summary>
    public FilterSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The settings for this notification.
    /// </summary>
    public JsonDocument Query { get; set; } = JsonDocument.Parse("{}");

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
        this.TemplateId = entity.TemplateId;
        this.Resend = entity.Resend;
        this.OwnerId = entity.OwnerId;
        this.IsPublic = entity.IsPublic;
        this.AlertOnIndex = entity.AlertOnIndex;
        this.Settings = JsonSerializer.Deserialize<FilterSettingsModel>(entity.Settings, options) ?? new();
        this.Query = entity.Query;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Notification(NotificationModel model)
    {
        var entity = new Entities.Notification(model.Id, model.Name, model.NotificationType, model.TemplateId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            Resend = model.Resend,
            IsPublic = model.IsPublic,
            AlertOnIndex = model.AlertOnIndex,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Query = JsonDocument.Parse(JsonSerializer.Serialize(model.Query)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
