using TNO.Services.Config;

namespace TNO.Services.Image.Config;

/// <summary>
/// ImageOptions class, configuration options for image
/// </summary>
public class ImageOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The path to the local mapped volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - The path to private key files
    /// </summary>
    public string PrivateKeysPath { get; set; } = "";

    #endregion
}
