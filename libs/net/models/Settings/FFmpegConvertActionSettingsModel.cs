
namespace TNO.API.Models.Settings;

/// <summary>
/// FFmpegConvertActionSettingsModel class, provides a model to control media type convert settings consistently.
/// </summary>
public class FFmpegConvertActionSettingsModel : FFmpegActionSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Convert the following file type (i.e. .m4a).
    /// </summary>
    public string From { get; set; } = "";

    /// <summary>
    /// get/set - Convert to the following file type (i.e. .mp3).
    /// </summary>
    public string To { get; set; } = "";
    #endregion

    #region Constructors
    public FFmpegConvertActionSettingsModel()
    {
    }

    public FFmpegConvertActionSettingsModel(string fromExt, string toExt, int sortOrder = 0)
        : base(FFmpegAction.Convert, new Dictionary<string, string>() { { "from", fromExt }, { "to", toExt } }, sortOrder)
    {
        this.From = fromExt;
        this.To = toExt;
    }

    public FFmpegConvertActionSettingsModel(Dictionary<string, string> args, int sortOrder = 0)
        : base(FFmpegAction.Convert, args, sortOrder)
    {
        this.From = args.GetValueOrDefault("from", "");
        this.To = args.GetValueOrDefault("to", "");
    }
    #endregion
}
