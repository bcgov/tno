
namespace TNO.API.Models.Settings;

/// <summary>
/// FFmpegActionSettingsModel class, provides a model to control product convert settings consistently.
/// </summary>
public class FFmpegActionSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - The action to perform
    /// </summary>
    public FFmpegAction Action { get; set; }

    /// <summary>
    /// get/set - The order to perform the action.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - Dictionary of arguments to pass to FFmpeg.
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();
    #endregion

    #region Constructors
    public FFmpegActionSettingsModel()
    {
        this.Action = FFmpegAction.Convert;
    }

    public FFmpegActionSettingsModel(FFmpegAction action, Dictionary<string, string> args, int sortOrder = 0)
    {
        this.Action = action;
        this.Arguments = args;
        this.SortOrder = sortOrder;
    }
    #endregion
}
