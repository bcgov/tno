using TNO.Services.Command.Config;

namespace TNO.Services.Clip.Config;

/// <summary>
/// ClipOptions class, configuration options for clip
/// </summary>
public class ClipOptions : CommandOptions
{
    #region Properties
    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - Limit the time after a schedule 'StopAt' time has passed that will still result in a verified schedule.
    /// We don't want to continuously check if prior schedules have been run successfully, so this limits the window for checking.
    /// </summary>
    public TimeSpan ScheduleLimiter { get; set; } = new TimeSpan(0, 5, 0);
    #endregion
}
