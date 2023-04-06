using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Notification;

/// <summary>
/// UserNotificationModel class, provides a model that represents a subscriber to a notification.
/// </summary>
public class UserNotificationModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the notification.
    /// </summary>
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the notification.
    /// </summary>
    public int NotificationId { get; set; }

    /// <summary>
    /// get/set - When to resend the notification.
    /// This overrides the notification Resend rule.
    /// </summary>
    public ResendOption? Resend { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserNotificationModel.
    /// </summary>
    public UserNotificationModel() { }

    /// <summary>
    /// Creates a new instance of an UserNotificationModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserNotificationModel(Entities.UserNotification entity) : base(entity)
    {
        this.UserId = entity.UserId;
        this.User = entity.User != null ? new UserModel(entity.User) : null;
        this.NotificationId = entity.NotificationId;
        this.Resend = entity.Resend;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserNotification(UserNotificationModel model)
    {
        return new Entities.UserNotification(model.UserId, model.NotificationId)
        {
            Resend = model.Resend,
            Version = model.Version ?? 0
        };
    }
    #endregion
}
