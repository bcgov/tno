namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// UserContentNotificationModel class, provides a model that represents a user content notification.
/// </summary>
public class UserContentNotificationModel
{
    #region Properties

    /// <summary>
    /// get/set - Primary key and Foreign key to user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - User that is subscribed to this content.
    /// </summary>
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Whether the user is subscribed to this notification.
    /// </summary>
    public bool IsSubscribed { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserContentNotificationModel.
    /// </summary>
    public UserContentNotificationModel() { }

    /// <summary>
    /// Creates a new instance of an UserContentNotificationModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserContentNotificationModel(Entities.UserContentNotification entity)
    {
        this.UserId = entity.UserId;
        this.User = entity.User != null ? new UserModel(entity.User) : null;
        this.ContentId = entity.ContentId;
        this.IsSubscribed = entity.IsSubscribed;
    }
    #endregion
}
