namespace TNO.Entities;

/// <summary>
/// Provides schedule type.
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// Continually runs.
    /// </summary>
    Continuous = 1,

    /// <summary>
    /// Daily runs a single start and stop event.
    /// </summary>
    Daily = 2,

    /// <summary>
    /// Runs based on a programmed schedule of multiple start ands top events.
    /// </summary>
    Advanced = 3,
}
