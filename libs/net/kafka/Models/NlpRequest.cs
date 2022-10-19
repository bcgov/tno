using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.WorkOrder;
using TNO.Entities;

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

    /// <summary>
    /// get/set - The content to be transcribed.
    /// </summary>
    public ContentModel? Content { get; set; }

    /// <summary>
    /// get/set - The primary key to identify the content to index.
    /// </summary>
    public long WorkOrderId { get; set; }

    /// <summary>
    /// get/set - The work order submitted.
    /// </summary>
    public WorkOrderModel? WorkOrder { get; set; }
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
    public NLPRequest(Content content)
    {
        this.ContentId = content.Id;
        this.Content = new ContentModel(content);
    }

    /// <summary>
    /// Creates a new instance of an NLPRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public NLPRequest(ContentModel content)
    {
        this.ContentId = content.Id;
        this.Content = content;
    }

    /// <summary>
    /// Creates a new instance of an NLPRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    public NLPRequest(WorkOrder workOrder)
    {
        if (!workOrder.ContentId.HasValue) throw new ArgumentException("Work order must be for content", nameof(workOrder));

        this.WorkOrderId = workOrder.Id;
        this.WorkOrder = new WorkOrderModel(workOrder);
        this.ContentId = workOrder.ContentId.Value;
    }

    /// <summary>
    /// Creates a new instance of an NLPRequest object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="contentId"></param>
    public NLPRequest(long workOrderId, long contentId)
    {
        this.WorkOrderId = workOrderId;
        this.ContentId = contentId;
    }
    #endregion
}
