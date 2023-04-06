namespace TNO.Kafka.Models;

/// <summary>
/// NotificationDestination enum, provides a way to control where to send a notification.
/// </summary>
[Flags]
public enum NotificationDestination
{
    /// <summary>
    /// No destination specified.
    /// </summary>
    None = 0,
    /// <summary>
    /// Broadcast message with SignalR
    /// </summary>
    SignalR = 1,
    /// <summary>
    /// The Notification Service listens for these.
    /// </summary>
    NotificationService = 2,
}
