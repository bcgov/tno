namespace TNO.API.Areas.Services.Models.Notification;

/// <summary>
/// ChesNotificationMessagesModel class, provides a model to pass CHES notification response message Ids.
/// </summary>
public class ChesNotificationMessagesModel
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
    /// get/set - The status of this CHES request.
    /// </summary>
    public Entities.NotificationStatus Status { get; set; }

    /// <summary>
    /// get/set - The CHES message Ids.
    /// </summary>
    public IEnumerable<Guid> MessageIds { get; set; } = [];
}
