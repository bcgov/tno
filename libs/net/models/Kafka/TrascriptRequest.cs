using TNO.API.Areas.Services.Models.Content;

namespace TNO.Models.Kafka;

/// <summary>
/// TranscriptRequest class, provides a model for requesting a transcript.
/// TODO: Change name to TranscriptRequestModel for consistent naming convention
/// </summary>
public class TranscriptRequest
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to identify the content to index.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Who requested the transcript
    /// </summary>
    public string Requestor { get; set; } = "";

    /// <summary>
    /// get/set - When the request was submitted.
    /// </summary>
    public DateTime RequestedOn { get; set; } = DateTime.UtcNow;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TranscriptRequest object.
    /// </summary>
    public TranscriptRequest() { }

    /// <summary>
    /// Creates a new instance of an TranscriptRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="requestor"></param>
    public TranscriptRequest(long contentId, string requestor)
    {
        this.ContentId = contentId;
        this.Requestor = requestor;
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestor"></param>
    public TranscriptRequest(ContentModel content, string requestor)
    {
        this.ContentId = content.Id;
        this.Requestor = requestor;
    }
    #endregion
}
