namespace TNO.Entities;

/// <summary>
/// Provides notification status.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification is pending to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Notification is accepted by CHES.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Notification is sent.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Notification was cancelled.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Notification failed to send.
    /// </summary>
    Failed = 4
}
