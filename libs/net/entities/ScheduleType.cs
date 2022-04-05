namespace TNO.Entities;

/// <summary>
/// Provides schedule type.
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// Continually runs.
    /// </summary>
    Continuous = 0,

    /// <summary>
    /// Daily runs a single start and stop event.
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Runs based on a programmed schedule of multiple start ands top events.
    /// </summary>
    Advanced = 2,
}
