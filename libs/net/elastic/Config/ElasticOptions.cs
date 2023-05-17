
namespace TNO.Elastic;

/// <summary>
/// ElasticOptions class, configuration options for elasticsearch.
/// </summary>
public class ElasticOptions
{
    #region Properties
    /// <summary>
    /// get/set - The Url to the Elasticsearch API.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// get/set - The name of the Elasticsearch index for unpublished content.
    /// </summary>
    public string UnpublishedIndex { get; set; } = "unpublished_content";

    /// <summary>
    /// get/set - The name of the Elasticsearch index for published content.
    /// </summary>
    public string PublishedIndex { get; set; } = "content";
    #endregion
}
