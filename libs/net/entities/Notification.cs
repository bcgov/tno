using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Notification class, provides a DB model to manage different types of notifications.
/// </summary>
[Cache("notification")]
[Table("notification")]
public class Notification : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of notification.
    /// </summary>
    [Column("notification_type")]
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// get/set - Whether content must be alerted to be included in this notification (default: true).
    /// </summary>
    [Column("require_alert")]
    public bool RequireAlert { get; set; } = true;

    /// <summary>
    /// get/set - The default filter for this notification.
    /// </summary>
    [Column("filter")]
    public JsonDocument Filter { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - When to resend the notification.
    /// </summary>
    [Column("resend")]
    public ResendOption Resend { get; set; } = ResendOption.Never;

    /// <summary>
    /// get/set - Foreign key to user who owns this notification.
    /// </summary>
    [Column("owner_id")]
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this notification.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - Whether this notification is public to all users.
    /// </summary>
    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The notification settings to control the output.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The Razor template to generate the notification.
    /// </summary>
    [Column("template")]
    public string Template { get; set; } = "";

    /// <summary>
    /// get - List of users who are subscribed to this notification (many-to-many).
    /// </summary>
    public virtual List<UserNotification> SubscribersManyToMany { get; } = new List<UserNotification>();

    /// <summary>
    /// get - List of users who are subscribed to this notification.
    /// </summary>
    public virtual List<User> Subscribers { get; } = new List<User>();

    /// <summary>
    /// get - Collection of notification instances.
    /// </summary>
    public virtual List<NotificationInstance> Instances { get; } = new List<NotificationInstance>();

    /// <summary>
    /// get - Collection of event schedules for this notification.
    /// </summary>
    public virtual List<EventSchedule> Schedules { get; } = new List<EventSchedule>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Notification object.
    /// </summary>
    protected Notification() : base() { }

    /// <summary>
    /// Creates a new instance of a Notification object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="owner"></param>
    public Notification(string name, NotificationType type, User owner)
        : this(0, name, type, owner?.Id ?? throw new ArgumentNullException(nameof(owner)))
    {
        this.Owner = owner;
    }

    /// <summary>
    /// Creates a new instance of a Notification object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="ownerId"></param>
    public Notification(int id, string name, NotificationType type, int ownerId) : base(id, name)
    {
        this.NotificationType = type;
        this.OwnerId = ownerId;
    }
    #endregion
}
