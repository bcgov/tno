using System.Text.Json;

namespace TNO.API.Areas.Services.Models.Notification;

/// <summary>
/// SmtpNotificationMessagesModel class, provides a model to pass SMTP notification response message Ids.
/// </summary>
public class SmtpNotificationMessagesModel
{
    /// <summary>
    /// get/set - The primary key to the notification.
    /// </summary>
    public long NotificationId { get; set; }

    /// <summary>
    /// get/set - The notification instance id.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - When the notification was sent.
    /// </summary>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The status of this SMTP request.
    /// </summary>
    public Entities.NotificationStatus Status { get; set; }

    /// <summary>
    /// get/set - The SMTP response messages.
    /// </summary>
    public JsonDocument Responses { get; set; } = JsonDocument.Parse("[]");
}
