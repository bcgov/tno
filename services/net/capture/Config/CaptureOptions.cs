using TNO.Services.Command.Config;

namespace TNO.Services.Capture.Config;

/// <summary>
/// CaptureOptions class, configuration options for capture
/// </summary>
public class CaptureOptions : CommandOptions
{
    #region Properties
    /// <summary>
    /// get/set - The command to execute (i.e. ffmpeg).
    /// </summary>
    public string Command { get; set; } = "ffmpeg";

    /// <summary>
    /// get/set - The path to store files.
    /// </summary>
    public string OutputPath { get; set; } = "";
    #endregion
}
