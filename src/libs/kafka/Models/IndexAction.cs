namespace TNO.Kafka.Models;

/// <summary>
/// IndexRequest class, provides a model for requesting the specified content to be indexed into Elasticsearch.
/// </summary>
public enum IndexAction
{
    /// <summary>
    /// Add the content to the unpublished index.
    /// </summary>
    Index = 0,
    /// <summary>
    /// Add the content to the published index.
    /// </summary>
    Publish = 1,
    /// <summary>
    /// Remove the content from the published index.
    /// </summary>
    Unpublish = 2,
    /// <summary>
    /// Delete the content from both indexes.
    /// </summary>
    Delete = 3
}
