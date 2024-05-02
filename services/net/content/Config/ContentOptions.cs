
using TNO.Services.Config;

namespace TNO.Services.Content.Config;

/// <summary>
/// ContentOptions class, configuration options for content service
/// </summary>
public class ContentOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of topics to consume.
    /// </summary>
    public string ContentTopics { get; set; } = "";

    /// <summary>
    /// get/set - A comma separated list of topics to NOT consume.
    /// </summary>
    public string ContentTopicsToExclude { get; set; } = "";

    /// <summary>
    /// get/set - The path to files stored on the local volume.
    /// </summary>
    public string VolumePath { get; set; } = "";

    /// <summary>
    /// get/set - The data location this service is being run in.
    /// This provides context information for content that is stored on local volumes.
    /// </summary>
    public string DataLocation { get; set; } = "";

    /// <summary>
    /// get/set - The path to private key files
    /// </summary>
    public string PrivateKeysPath { get; set; } = "";

    /// <summary>
    /// get/set - Settings around content coming from the Content Migration service
    /// </summary>
    public ContentMigrationOptions MigrationOptions { get; set; } = new ContentMigrationOptions();
    #endregion

    #region Methods
    /// <summary>
    /// Get the configured topics, or return the default topics.
    /// </summary>
    /// <param name="defaultTopics"></param>
    /// <returns></returns>
    public string[] GetContentTopics(string[]? defaultTopics = null)
    {
        var topics = this.ContentTopics?.Split(',').Where(v => !String.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
        return topics.Length > 0 ? topics : defaultTopics ?? Array.Empty<string>();
    }

    /// <summary>
    /// Get a list of topics to exclude.
    /// </summary>
    /// <param name="defaultTopics"></param>
    /// <returns></returns>
    public string[] GetContentTopicsToExclude()
    {
        var topics = this.ContentTopicsToExclude?.Split(',').Where(v => !String.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
        return topics.Length > 0 ? topics : Array.Empty<string>();
    }
    #endregion
}
