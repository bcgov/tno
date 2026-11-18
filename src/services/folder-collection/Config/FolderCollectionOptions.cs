
using TNO.Services.Config;

namespace TNO.Services.FolderCollection.Config;

/// <summary>
/// FolderCollectionOptions class, configuration options for indexing service
/// </summary>
public class FolderCollectionOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of Kafka topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - Ignore any content that was indexed before this day offset.
    /// </summary>
    public int? IgnoreContentPublishedBeforeOffset { get; set; }
    #endregion

    #region Methods
    /// <summary>
    /// Get the configured topics, or return the default topics.
    /// </summary>
    /// <param name="defaultTopics"></param>
    /// <returns></returns>
    public string[] GetTopics(string[]? defaultTopics = null)
    {
        var topics = this.Topics?.Split(',').Where(v => !String.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray() ?? Array.Empty<string>();
        return topics.Length > 0 ? topics : defaultTopics ?? Array.Empty<string>();
    }
    #endregion
}
