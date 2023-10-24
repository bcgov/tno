namespace TNO.Entities;

/// <summary>
/// Provides event schedule type.
/// </summary>
public enum EventScheduleType
{
    /// <summary>
    /// This event creates a report.
    /// </summary>
    Report = 0,
    /// <summary>
    /// This event creates a notification.
    /// </summary>
    Notification = 1,
    /// <summary>
    /// This event cleans content from a folder.
    /// </summary>
    CleanFolder = 2,
}
