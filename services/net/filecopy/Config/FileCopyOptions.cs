
using TNO.Services.Config;

namespace TNO.Services.FileCopy.Config;

/// <summary>
/// FileCopyOptions class, configuration options for transcription service
/// </summary>
public class FileCopyOptions : ServiceOptions
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
    /// get/set - The data location this service is being run in.
    /// This provides context information for content that is stored on local volumes.
    /// </summary>
    public string DataLocation { get; set; } = "";
    #endregion
}
