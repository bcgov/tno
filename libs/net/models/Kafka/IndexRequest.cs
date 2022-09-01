using TNO.API.Areas.Services.Models.Content;

namespace TNO.Models.Kafka;

/// <summary>
/// IndexRequest class, provides a model for requesting the specified content to be indexed into Elasticsearch.
/// TODO: Change name to IndexRequestModel for consistent naming convention
/// </summary>
public class IndexRequest
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to identify the content to index.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Whether the content should be published.
    /// </summary>
    public IndexAction Action { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IndexRequest object.
    /// </summary>
    public IndexRequest() { }

    /// <summary>
    /// Creates a new instance of an IndexRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="action"></param>
    public IndexRequest(long contentId, IndexAction action = IndexAction.Index)
    {
        this.ContentId = contentId;
        this.Action = action;
    }

    /// <summary>
    /// Creates a new instance of an IndexRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="action"></param>
    public IndexRequest(ContentModel content, IndexAction action = IndexAction.Index)
    {
        this.ContentId = content.Id;
        this.Action = action;
    }
    #endregion
}
