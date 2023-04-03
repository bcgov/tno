using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// NotificationInstance class, provides a DB model to capture an instance of a notification.
/// </summary>
[Table("notification_instance")]
public class NotificationInstance : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the notification definition.
    /// </summary>
    [Column("notification_id")]
    public int NotificationId { get; set; }

    /// <summary>
    /// get/set - The notification definition.
    /// </summary>
    public Notification? Notification { get; set; }

    /// <summary>
    /// get - Foreign key to the content related to the notification.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get - The content this notification is related to.
    /// </summary>
    public virtual Content? Content { get; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a notification.
    /// </summary>
    [Column("response")]
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationInstance object.
    /// </summary>
    protected NotificationInstance() : base() { }

    /// <summary>
    /// Creates a new instance of a NotificationInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    public NotificationInstance(Notification notification, Content content)
    {
        this.Notification = notification ?? throw new ArgumentNullException(nameof(notification));
        this.NotificationId = notification.Id;
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.ContentId = content.Id;
    }

    /// <summary>
    /// Creates a new instance of a NotificationInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    public NotificationInstance(int notificationId, long contentId)
    {
        this.NotificationId = notificationId;
        this.ContentId = contentId;
    }
    #endregion
}
