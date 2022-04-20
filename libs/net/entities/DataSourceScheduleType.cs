namespace TNO.Entities;

/// <summary>
/// Provides data source schedule type.
/// </summary>
public enum DataSourceScheduleType
{
    /// <summary>
    /// This data source has no schedule.
    /// </summary>
    None = 0,

    /// <summary>
    /// Repeating will run a continuously repeating schedule.
    /// </summary>
    Continuous = 1,

    /// <summary>
    /// Has a start and stop time each day.
    /// </summary>
    Daily = 2,

    /// <summary>
    /// Advanced schedule has many start and stop times.
    /// </summary>
    Advanced = 3,
}
