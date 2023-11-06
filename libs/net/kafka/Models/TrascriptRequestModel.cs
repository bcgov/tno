using System.Text.Json;
using TNO.API.Areas.Services.Models.Content;
using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// TranscriptRequest class, provides a model for requesting a transcript.
/// </summary>
public class TranscriptRequestModel : WorkOrderModel
{
    #region Properties
    /// <summary>
    /// get/set - The content Id to process.
    /// </summary>
    public long ContentId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TranscriptRequestModel object.
    /// </summary>
    public TranscriptRequestModel() : base(WorkOrderType.Transcription) { }

    /// <summary>
    /// Creates a new instance of an TranscriptRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    public TranscriptRequestModel(long workOrderId, long contentId, int? requestorId, string requestor) : base(workOrderId, WorkOrderType.Transcription, requestorId, requestor, DateTime.UtcNow)
    {
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    public TranscriptRequestModel(Content content, int? requestorId, string requestor) : this(0, content.Id, requestorId, requestor)
    {
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    public TranscriptRequestModel(ContentModel content, int? requestorId, string requestor) : this(0, content.Id, requestorId, requestor)
    {
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    public TranscriptRequestModel(WorkOrder workOrder) : base(workOrder)
    {
        if (workOrder.ContentId.HasValue)
            this.ContentId = workOrder.ContentId.Value;
        else if (workOrder.Configuration.RootElement.TryGetProperty("contentId", out JsonElement element) && element.TryGetInt64(out long contentId))
            this.ContentId = contentId;
        else throw new ArgumentException("Work order must be for a transcription and contain 'contentId' property.");
    }
    #endregion
}
