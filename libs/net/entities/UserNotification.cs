using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserNotification class, provides a model to link users with their notifications.
/// </summary>
[Table("user_notification")]
public class UserNotification : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the notification.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the notification.
    /// </summary>
    [Column("notification_id")]
    public int NotificationId { get; set; }

    /// <summary>
    /// get/set - the notification linked to the user.
    /// </summary>
    public Notification? Notification { get; set; }

    /// <summary>
    /// get/set - When to resend the notification.
    /// This overrides the notification Resend rule.
    /// </summary>
    [Column("resend")]
    public ResendOption? Resend { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserNotification object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="notification"></param>
    /// <param name="resend"></param>
    public UserNotification(User user, Notification notification, ResendOption? resend = null)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Notification = notification ?? throw new ArgumentNullException(nameof(notification));
        this.NotificationId = notification.Id;
        this.Resend = resend;
    }

    /// <summary>
    /// Creates a new instance of a UserNotification object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="notificationId"></param>
    /// <param name="resend"></param>
    public UserNotification(int userId, int notificationId, ResendOption? resend = null)
    {
        this.UserId = userId;
        this.NotificationId = notificationId;
        this.Resend = resend;
    }
    #endregion

    #region Methods
    public bool Equals(UserNotification? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.NotificationId == other.NotificationId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserNotification);
    public override int GetHashCode() => (this.UserId, this.NotificationId).GetHashCode();
    #endregion
}
