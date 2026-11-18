using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.User;

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
    /// get/set - Whether this notification is public to all users.
    /// </summary>
    public bool IsPublic { get; set; }
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
    public NotificationModel(Entities.Notification entity) : base(entity)
    {
        this.NotificationType = entity.NotificationType;
        this.IsPublic = entity.IsPublic;
    }
    #endregion
}
