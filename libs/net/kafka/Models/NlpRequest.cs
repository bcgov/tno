using TNO.API.Areas.Services.Models.Content;

namespace TNO.Kafka.Models;

/// <summary>
/// NLPRequest class, provides a model for requesting natural language processing.
/// TODO: Change name to NLPRequestModel for consistent naming convention
/// </summary>
public class NLPRequest
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to identify the content to index.
    /// </summary>
    public long ContentId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NLPRequest object.
    /// </summary>
    public NLPRequest() { }

    /// <summary>
    /// Creates a new instance of an NLPRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    public NLPRequest(long contentId)
    {
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of an NLPRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public NLPRequest(ContentModel content)
    {
        this.ContentId = content.Id;
    }
    #endregion
}
