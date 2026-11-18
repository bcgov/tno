namespace TNO.Entities;

/// <summary>
/// NotificationType enum, provides a way to identify different types of notifications.
/// </summary>
[Flags]
public enum NotificationType
{
    /// <summary>
    /// Send an email to the user.
    /// </summary>
    Email = 0,
    /// <summary>
    /// Send a signal to the user.
    /// </summary>
    Signal = 1,
    /// <summary>
    /// Send a SMS to the user.
    /// </summary>
    SMS = 2,
    /// <summary>
    /// Send a mobile notification to the user.
    /// </summary>
    Mobile = 4,
}
