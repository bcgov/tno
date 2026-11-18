using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// NotificationTemplate class, provides a DB model to manage different types of notification templates.
/// </summary>
[Cache("notification_template")]
[Table("notification_template")]
public class NotificationTemplate : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor subject template to generate the notification.
    /// </summary>
    [Column("subject")]
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The Razor body template to generate the notification.
    /// </summary>
    [Column("body")]
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - Whether this notification template is public to all users.
    /// </summary>
    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The notification template settings.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - Collection of notifications that use this template.
    /// </summary>
    public virtual List<Notification> Notifications { get; } = new List<Notification>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationTemplate object.
    /// </summary>
    protected NotificationTemplate() : base() { }

    /// <summary>
    /// Creates a new instance of a NotificationTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="subjectTemplate"></param>
    /// <param name="bodyTemplate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public NotificationTemplate(string name, string subjectTemplate, string bodyTemplate) : base(name)
    {
        this.Subject = subjectTemplate ?? throw new ArgumentNullException(nameof(subjectTemplate));
        this.Body = bodyTemplate ?? throw new ArgumentNullException(nameof(bodyTemplate));
    }

    /// <summary>
    /// Creates a new instance of a NotificationTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="subjectTemplate"></param>
    /// <param name="bodyTemplate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public NotificationTemplate(int id, string name, string subjectTemplate, string bodyTemplate) : base(id, name)
    {
        this.Subject = subjectTemplate ?? throw new ArgumentNullException(nameof(subjectTemplate));
        this.Body = bodyTemplate ?? throw new ArgumentNullException(nameof(bodyTemplate));
    }
    #endregion
}
