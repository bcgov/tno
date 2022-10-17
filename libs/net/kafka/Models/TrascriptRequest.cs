using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.WorkOrder;
using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// TranscriptRequest class, provides a model for requesting a transcript.
/// TODO: Change name to TranscriptRequestModel for consistent naming convention
/// </summary>
public class TranscriptRequest
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the content to be transcribed.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content to be transcribed.
    /// </summary>
    public ContentModel? Content { get; set; }

    /// <summary>
    /// get/set - Foreign key to the work order submitted.
    /// </summary>
    public long WorkOrderId { get; set; }

    /// <summary>
    /// get/set - The work order submitted.
    /// </summary>
    public WorkOrderModel? WorkOrder { get; set; }

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
    /// <param name="workOrderId"></param>
    /// <param name="contentId"></param>
    /// <param name="requestor"></param>
    public TranscriptRequest(long workOrderId, long contentId, string requestor)
    {
        this.WorkOrderId = workOrderId;
        this.ContentId = contentId;
        this.Requestor = requestor;
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestor"></param>
    public TranscriptRequest(Content content, string requestor)
    {
        this.ContentId = content.Id;
        this.Content = new ContentModel(content);
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
        this.Content = content;
        this.Requestor = requestor;
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="requestor"></param>
    public TranscriptRequest(WorkOrder workOrder, string requestor)
    {
        if (!workOrder.ContentId.HasValue) throw new ArgumentException("Work order must be for content", nameof(workOrder));

        this.WorkOrderId = workOrder.Id;
        this.WorkOrder = new WorkOrderModel(workOrder);
        this.ContentId = workOrder.ContentId.Value;
        this.Requestor = requestor;
    }

    /// <summary>
    /// Creates a new instance of an TranscriptRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="requestor"></param>
    public TranscriptRequest(WorkOrderModel workOrder, string requestor)
    {
        if (!workOrder.ContentId.HasValue) throw new ArgumentException("Work order must be for content", nameof(workOrder));

        this.WorkOrderId = workOrder.Id;
        this.WorkOrder = workOrder;
        this.ContentId = workOrder.ContentId.Value;
        this.Requestor = requestor;
    }
    #endregion
}
