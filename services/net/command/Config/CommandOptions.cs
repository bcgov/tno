using TNO.Services.Config;

namespace TNO.Services.Command.Config;

/// <summary>
/// CommandOptions class, configuration options for command
/// </summary>
public class CommandOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The command to execute (i.e. ffmpeg).
    /// </summary>
    public string Command { get; set; } = "";
    #endregion
}
