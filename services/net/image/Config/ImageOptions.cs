using TNO.Services.Config;

namespace TNO.Services.Image.Config;

/// <summary>
/// ImageOptions class, configuration options for image
/// </summary>
public class ImageOptions : IngestServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - The path to private key files
    /// </summary>
    public string PrivateKeysPath { get; set; } = "";

    /// <summary>
    /// get/set - The path to store files.
    /// </summary>
    public string OutputPath { get; set; } = "";

    /// <summary>
    /// get/set - The path to fetch files.
    /// </summary>
    /// <value></value>
    public string InputPath { get; set; } = "";

    /// <summary>
    /// Server name to fetch images
    /// </summary>
    /// <value></value>
    public string HostName { get; set; } = "";

    /// <summary>
    /// PrivateKeyFileName if not set
    /// </summary>
    /// <value></value>
    public string PrivateKeyFileName { get; set; } = "";

    /// <summary>
    /// Username
    /// </summary>
    /// <value></value>
    public string Username { get; set; } = "";
    
    #endregion
}