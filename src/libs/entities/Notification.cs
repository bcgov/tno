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
    /// get/set - When to resend the notification.
    /// </summary>
    [Column("resend")]
    public ResendOption Resend { get; set; } = ResendOption.Never;

    /// <summary>
    /// get/set - Foreign key to user who owns this notification.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

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
    /// get/set - Whether this notification is automatically run when the content is indexed.
    /// </summary>
    [Column("alert_on_index")]
    public bool AlertOnIndex { get; set; } = false;

    /// <summary>
    /// get/set - Foreign key to notification template.
    /// </summary>
    [Column("notification_template_id")]
    public int TemplateId { get; set; }

    /// <summary>
    /// get/set - The notification template containing the razor template.
    /// </summary>
    public NotificationTemplate? Template { get; set; }

    /// <summary>
    /// get/set - The notification settings to control the output and filter.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The elasticsearch query that is used when sending out multiple notifications.
    /// </summary>
    [Column("query")]
    public JsonDocument Query { get; set; } = JsonDocument.Parse("{}");

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
    /// <param name="templateId"></param>
    public Notification(string name, NotificationType type, User owner, NotificationTemplate template)
        : this(0, name, type, owner?.Id ?? throw new ArgumentNullException(nameof(owner)), template?.Id ?? throw new ArgumentNullException(nameof(template)))
    {
        this.Owner = owner;
        this.Template = template;
        this.TemplateId = template.Id;
    }

    /// <summary>
    /// Creates a new instance of a Notification object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="ownerId"></param>
    /// <param name="templateId"></param>
    public Notification(int id, string name, NotificationType type, int ownerId, int templateId) : base(id, name)
    {
        this.NotificationType = type;
        this.OwnerId = ownerId;
        this.TemplateId = templateId;
    }

    /// <summary>
    /// Creates a new instance of a Notification object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="templateId"></param>
    public Notification(string name, NotificationType type, NotificationTemplate template)
        : this(0, name, type, template?.Id ?? throw new ArgumentNullException(nameof(template)))
    {
        this.Template = template;
        this.TemplateId = template.Id;
    }

    /// <summary>
    /// Creates a new instance of a Notification object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="ownerId"></param>
    /// <param name="templateId"></param>
    public Notification(int id, string name, NotificationType type, int templateId) : base(id, name)
    {
        this.NotificationType = type;
        this.TemplateId = templateId;
    }

    /// <summary>
    /// Creates a new instance of a Notification object, initializes with specified parameters.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="ownerId"></param>
    public Notification(Notification notification, int ownerId)
    {
        this.Id = 0;
        this.Name = notification.Name;
        this.NotificationType = notification.NotificationType;
        this.OwnerId = ownerId;
        this.Description = notification.Description;
        this.AlertOnIndex = notification.AlertOnIndex;
        this.IsEnabled = notification.IsEnabled;
        this.IsPublic = notification.IsPublic;
        this.Query = notification.Query;
        this.Resend = notification.Resend;
        this.Settings = notification.Settings;
        this.SortOrder = notification.SortOrder;
        this.TemplateId = notification.TemplateId;
        this.SubscribersManyToMany.AddRange(notification.SubscribersManyToMany.Select(s => new UserNotification(s.UserId, 0, true)));
        this.Schedules.AddRange(notification.Schedules.Select(s =>
            new EventSchedule(s.Name, s.EventType,
                new Schedule(s.Schedule!.Name, s.Schedule.DelayMS)
                {
                    Description = s.Schedule!.Description,
                    DayOfMonth = s.Schedule!.DayOfMonth,
                    DelayMS = s.Schedule!.DelayMS,
                    IsEnabled = s.Schedule!.IsEnabled,
                    Repeat = s.Schedule!.Repeat,
                    RequestedBy = s.Schedule!.RequestedBy,
                    RequestedById = s.Schedule!.RequestedById,
                    RunOn = s.Schedule!.RunOn,
                    RunOnlyOnce = s.Schedule!.RunOnlyOnce,
                    RunOnMonths = s.Schedule!.RunOnMonths,
                    RunOnWeekDays = s.Schedule!.RunOnWeekDays,
                    StartAt = s.Schedule!.StartAt,
                    StopAt = s.Schedule!.StopAt,
                }, s.Settings)
            {
                Description = s.Description,
                IsEnabled = s.IsEnabled,
                LastRanOn = s.LastRanOn,
                RequestSentOn = s.RequestSentOn,
                FolderId = s.FolderId,
                ReportId = s.ReportId,
                NotificationId = s.NotificationId,
            })
            );
    }
    #endregion
}
