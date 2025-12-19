using System.Text.Json;
using TNO.API.Areas.Services.Models.Content;
using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// ClipRequestModel class, provides a model for requesting automatic clip generation via Azure Video Analyzer.
/// </summary>
public class ClipRequestModel : WorkOrderModel
{
    #region Properties
    /// <summary>
    /// get/set - The content Id to process.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Preferred language for the transcript generation.
    /// </summary>
    public string Language { get; set; } = "en-US";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClipRequestModel object.
    /// </summary>
    public ClipRequestModel() : base(WorkOrderType.AutoClipper) { }

    /// <summary>
    /// Creates a new instance of a ClipRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    /// <param name="language"></param>
    public ClipRequestModel(long workOrderId, long contentId, int? requestorId, string requestor, string language = "en-US")
        : base(workOrderId, WorkOrderType.AutoClipper, requestorId, requestor, DateTime.UtcNow)
    {
        this.ContentId = contentId;
        if (!string.IsNullOrWhiteSpace(language)) this.Language = language;
    }

    /// <summary>
    /// Creates a new instance of a ClipRequestModel object for the specified content model.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    /// <param name="language"></param>
    public ClipRequestModel(ContentModel content, int? requestorId, string requestor, string language = "en-US")
        : this(0, content.Id, requestorId, requestor, language)
    {
    }

    /// <summary>
    /// Creates a new instance of a ClipRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    public ClipRequestModel(WorkOrder workOrder) : base(workOrder)
    {
        if (workOrder.ContentId.HasValue)
            this.ContentId = workOrder.ContentId.Value;
        else if (workOrder.Configuration.RootElement.TryGetProperty("contentId", out JsonElement element) && element.TryGetInt64(out long contentId))
            this.ContentId = contentId;
        else throw new ArgumentException("Work order must be for an auto clipper request and contain 'contentId' property.");

        if (workOrder.Configuration.RootElement.TryGetProperty("language", out JsonElement languageElement) && languageElement.ValueKind == JsonValueKind.String)
        {
            var language = languageElement.GetString();
            if (!string.IsNullOrWhiteSpace(language)) this.Language = language!;
        }
    }
    #endregion
}

