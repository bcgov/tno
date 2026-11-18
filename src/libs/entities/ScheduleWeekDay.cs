namespace TNO.Entities;

/// <summary>
/// Provides schedule week day control options.
/// </summary>
[Flags]
public enum ScheduleWeekDay
{
    /// <summary>
    /// Not Applicable
    /// </summary>
    NA = 0,
    /// <summary>
    /// Sunday
    /// </summary>
    Sunday = 1,
    /// <summary>
    /// Monday
    /// </summary>
    Monday = 2,
    /// <summary>
    /// Tuesday
    /// </summary>
    Tuesday = 4,
    /// <summary>
    /// Wednesday
    /// </summary>
    Wednesday = 8,
    /// <summary>
    /// Thursday
    /// </summary>
    Thursday = 16,
    /// <summary>
    /// Friday
    /// </summary>
    Friday = 32,
    /// <summary>
    /// Saturday
    /// </summary>
    Saturday = 64
}
