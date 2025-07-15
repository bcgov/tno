
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
    /// get/set - Elasticsearch username.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// get/set - Elasticsearch password.
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// get/set - Elasticsearch API key.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// get/set - The name of the Elasticsearch index for all content.
    /// </summary>
    public string ContentIndex { get; set; } = "unpublished_content";

    /// <summary>
    /// get/set - The name of the Elasticsearch index for published content.
    /// </summary>
    public string PublishedIndex { get; set; } = "content";
    #endregion
}
