using System.Text.Json.Serialization;

namespace TNO.API.Models.Settings;

/// <summary>
/// MediaTypeSettingsModel class, provides a model to control media type settings consistently.
/// </summary>
public class MediaTypeSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Perform FFmpeg actions to content assigned to this media type.
    /// </summary>
    [JsonPropertyName("ffmpeg")]
    public IEnumerable<FFmpegActionSettingsModel> FFmpeg { get; set; } = Array.Empty<FFmpegActionSettingsModel>();
    #endregion
}
