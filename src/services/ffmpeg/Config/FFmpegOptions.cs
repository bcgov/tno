
using TNO.Services.Config;

namespace TNO.Services.FFmpeg.Config;

/// <summary>
/// FFmpegOptions class, configuration options for transcription service
/// </summary>
public class FFmpegOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - An array of converter configuration options.
    /// </summary>
    public ConverterOptions[] Converters { get; set; } = Array.Empty<ConverterOptions>();

    /// <summary>
    /// get/set - Number of retry attempts to update content.
    /// </summary>
    public int RetryUpdateLimit { get; set; } = 6;
    #endregion
}
