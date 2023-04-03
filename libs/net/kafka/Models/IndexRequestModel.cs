using TNO.API.Areas.Services.Models.Content;

namespace TNO.Kafka.Models;

/// <summary>
/// IndexRequestModel class, provides a model for requesting the specified content to be indexed into Elasticsearch.
/// </summary>
public class IndexRequestModel
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

    /// <summary>
    /// get/set - Foreign key to user who requested the index.
    /// </summary>
    public int? RequestorId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IndexRequestModel object.
    /// </summary>
    public IndexRequestModel() { }

    /// <summary>
    /// Creates a new instance of an IndexRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="action"></param>
    public IndexRequestModel(long contentId, IndexAction action = IndexAction.Index)
    {
        this.ContentId = contentId;
        this.Action = action;
    }

    /// <summary>
    /// Creates a new instance of an IndexRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="action"></param>
    public IndexRequestModel(long contentId, int? requestorId, IndexAction action = IndexAction.Index)
    {
        this.ContentId = contentId;
        this.RequestorId = requestorId;
        this.Action = action;
    }

    /// <summary>
    /// Creates a new instance of an IndexRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="action"></param>
    public IndexRequestModel(ContentModel content, IndexAction action = IndexAction.Index)
    {
        this.ContentId = content.Id;
        this.Action = action;
    }

    /// <summary>
    /// Creates a new instance of an IndexRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId"></param>
    /// <param name="action"></param>
    public IndexRequestModel(ContentModel content, int? requestorId, IndexAction action = IndexAction.Index)
    {
        this.ContentId = content.Id;
        this.RequestorId = requestorId;
        this.Action = action;
    }
    #endregion
}
