
using TNO.Services.Config;

namespace TNO.Services.Indexing.Config;

/// <summary>
/// IndexingOptions class, configuration options for indexing service
/// </summary>
public class IndexingOptions : ServiceOptions
{
    #region Properties
    /// <summary>
    /// get/set - A comma separated list of Kafka topics to consume.
    /// </summary>
    public string Topics { get; set; } = "";

    /// <summary>
    /// get/set - The topic to publish notifications to.
    /// </summary>
    public string NotificationTopic { get; set; } = "";

    /// <summary>
    /// get/set - The topic to publish hub notifications to.
    /// </summary>
    public string HubTopic { get; set; } = "";

    /// <summary>
    /// get/set - Control whether the service will only perform index operations and nothing else.
    ///           Useful for indexing to separate cluster.
    /// </summary>
    public bool IndexOnly { get; set; }
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
