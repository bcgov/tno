using TNO.Services.Config;

namespace TNO.Services.Image.Config;

/// <summary>
/// ImageOptions class, configuration options for image
/// </summary>
public class ImageOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The path to store files.
    /// </summary>
    public string OutputPath { get; set; } = "";
    #endregion
}
