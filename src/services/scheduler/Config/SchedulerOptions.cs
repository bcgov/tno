using TNO.Entities;
using TNO.Services.Config;

namespace TNO.Services.Scheduler.Config;

/// <summary>
/// SchedulerOptions class, configuration options for syndication
/// </summary>
public class SchedulerOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - An array of supported event types.
    /// </summary>
    public IEnumerable<EventScheduleType> EventTypes { get; set; } = Array.Empty<EventScheduleType>();
    #endregion
}
