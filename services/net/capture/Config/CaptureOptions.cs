using TNO.Services.Command.Config;

namespace TNO.Services.Capture.Config;

/// <summary>
/// CaptureOptions class, configuration options for capture
/// </summary>
public class CaptureOptions : CommandOptions
{
    #region Properties
    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";
    #endregion
}
