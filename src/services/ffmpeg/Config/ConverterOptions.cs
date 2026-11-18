
namespace TNO.Services.FFmpeg.Config;

/// <summary>
/// ConverterOptions class, configuration options for transcription service
/// </summary>
public class ConverterOptions
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the media type.
    /// </summary>
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The file extension to convert from
    /// </summary>
    public string FromFormat { get; set; } = "";

    /// <summary>
    /// get/set - The file extension to convert to.
    /// </summary>
    public string ToFormat { get; set; } = "";

    /// <summary>
    /// get/set - The content type to convert to.
    /// </summary>
    public string? ToContentType { get; set; }
    #endregion
}
