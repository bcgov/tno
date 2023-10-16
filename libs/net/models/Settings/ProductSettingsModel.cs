using System.Text.Json.Serialization;

namespace TNO.API.Models.Settings;

/// <summary>
/// ProductSettingsModel class, provides a model to control product settings consistently.
/// </summary>
public class ProductSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Perform FFmpeg actions to content assigned to this product.
    /// </summary>
    [JsonPropertyName("ffmpeg")]
    public IEnumerable<FFmpegActionSettingsModel> FFmpeg { get; set; } = Array.Empty<FFmpegActionSettingsModel>();
    #endregion
}
