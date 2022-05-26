using TNO.Services.Command.Config;

namespace TNO.Services.Clip.Config;

/// <summary>
/// ClipOptions class, configuration options for clip
/// </summary>
public class ClipOptions : CommandOptions
{
    #region Properties
    /// <summary>
    /// get/set - The command to execute (i.e. ffmpeg).
    /// </summary>
    public string Command { get; set; } = "ffmpeg";

    /// <summary>
    /// get/set - The path to captured files.
    /// </summary>
    public string CapturePath { get; set; } = "";

    /// <summary>
    /// get/set - The path to store files.
    /// </summary>
    public string OutputPath { get; set; } = "";

    /// <summary>
    /// get/set - Limit the time after a schedule 'StopAt' time has passed that will still result in a verified schedule.
    /// We don't want to continuously check if prior schedules have been run successfully, so this limits the window for checking.
    /// </summary>
    public TimeSpan ScheduleLimiter { get; set; } = new TimeSpan(0, 5, 0);
    #endregion
}
