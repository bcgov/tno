using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserContentNotification class, provides a model to link users with content and notifications.
/// </summary>
[Table("user_content_notification")]
public class UserContentNotification : AuditColumns
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
    /// get/set - Primary key and foreign key to the content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - the content linked to the user.
    /// </summary>
    public Content? Content { get; set; }

    /// <summary>
    /// get/set - Whether the user is subscribed to this content and will receive notifications when it is updated.
    /// </summary>
    [Column("is_subscribed")]
    public bool IsSubscribed { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserContentNotification object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="content"></param>
    public UserContentNotification(User user, Content content)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.ContentId = content.Id;
        this.IsSubscribed = true;
    }

    /// <summary>
    /// Creates a new instance of a UserContentNotification object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="contentId"></param>
    public UserContentNotification(
        int userId, long contentId, bool isSubscribed = true)
    {
        this.UserId = userId;
        this.ContentId = contentId;
        this.IsSubscribed = isSubscribed;
    }
    #endregion

    #region Methods
    public bool Equals(UserContentNotification? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.ContentId == other.ContentId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserContentNotification);
    public override int GetHashCode() => (this.UserId, this.ContentId).GetHashCode();
    #endregion
}
