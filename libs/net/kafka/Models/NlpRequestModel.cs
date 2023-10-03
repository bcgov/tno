using System.Text.Json;
using TNO.API.Areas.Services.Models.Content;
using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// NlpRequestModel class, provides a model for requesting natural language processing.
/// </summary>
public class NlpRequestModel : WorkOrderModel
{
    #region Properties
    /// <summary>
    /// get/set - The content Id to process.
    /// </summary>
    public long ContentId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NlpRequestModel object.
    /// </summary>
    public NlpRequestModel() : base(WorkOrderType.NaturalLanguageProcess) { }

    /// <summary>
    /// Creates a new instance of an NlpRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    public NlpRequestModel(long contentId) : this()
    {
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of an NlpRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public NlpRequestModel(Content content, int? requestorId, string requestor) : base(0, WorkOrderType.NaturalLanguageProcess, requestorId, requestor, DateTime.UtcNow)
    {
        this.ContentId = content.Id;
    }

    /// <summary>
    /// Creates a new instance of an NlpRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public NlpRequestModel(ContentModel content, int? requestorId, string requestor) : base(0, WorkOrderType.NaturalLanguageProcess, requestorId, requestor, DateTime.UtcNow)
    {
        this.ContentId = content.Id;
    }

    /// <summary>
    /// Creates a new instance of an NlpRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    public NlpRequestModel(WorkOrder workOrder) : base(workOrder.Id, workOrder.WorkType, workOrder.RequestorId, workOrder.Requestor?.DisplayName ?? "", workOrder.CreatedOn)
    {
        if (workOrder.ContentId.HasValue)
            this.ContentId = workOrder.ContentId.Value;
        else if (workOrder.Configuration.RootElement.TryGetProperty("contentId", out JsonElement element) && element.TryGetInt64(out long contentId))
            this.ContentId = contentId;
        else throw new ArgumentException("Work order must be for a transcription and contain 'contentId' property.");
    }
    #endregion
}
