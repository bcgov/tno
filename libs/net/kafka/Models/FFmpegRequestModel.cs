using System.Text.Json;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Models.Settings;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.Kafka.Models;

/// <summary>
/// FFmpegRequestModel class, provides a model for requesting FFmpeg to process a A/V file.
/// </summary>
public class FFmpegRequestModel : WorkOrderModel
{
    #region Properties
    /// <summary>
    /// get/set - The content Id to process.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The action to perform to the content.
    /// </summary>
    public IEnumerable<FFmpegActionSettingsModel> Actions { get; set; } = Array.Empty<FFmpegActionSettingsModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an FFmpegRequestModel object.
    /// </summary>
    public FFmpegRequestModel() : base(WorkOrderType.FFmpeg) { }

    /// <summary>
    /// Creates a new instance of an FFmpegRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="contentId"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    /// <param name="actions"></param>
    public FFmpegRequestModel(long workOrderId, long contentId, int? requestorId, string requestor, IEnumerable<FFmpegActionSettingsModel> actions)
        : base(workOrderId, WorkOrderType.FFmpeg, requestorId, requestor, DateTime.UtcNow)
    {
        this.ContentId = contentId;
        this.Actions = actions;
    }

    /// <summary>
    /// Creates a new instance of an FFmpegRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    /// <param name="actions"></param>
    public FFmpegRequestModel(Content content, int? requestorId, string requestor, IEnumerable<FFmpegActionSettingsModel> actions) : this(0, content.Id, requestorId, requestor, actions)
    {
    }

    /// <summary>
    /// Creates a new instance of an FFmpegRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId"></param>
    /// <param name="requestor"></param>
    /// <param name="actions"></param>
    public FFmpegRequestModel(ContentModel content, int? requestorId, string requestor, IEnumerable<FFmpegActionSettingsModel> actions) : this(0, content.Id, requestorId, requestor, actions)
    {
    }

    /// <summary>
    /// Creates a new instance of an FFmpegRequestModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="options"></param>
    public FFmpegRequestModel(WorkOrder workOrder, JsonSerializerOptions options) : base(workOrder.Id, workOrder.WorkType, workOrder.RequestorId, workOrder.Requestor?.DisplayName ?? "", workOrder.CreatedOn)
    {
        if (workOrder.ContentId.HasValue)
            this.ContentId = workOrder.ContentId.Value;
        else throw new ArgumentException("Work order must be for an FFmpeg and contain 'contentId' property.");

        var settings = workOrder.Configuration.RootElement.GetElementValue<MediaTypeSettingsModel>("ffmpeg", null, options);
        this.Actions = settings?.FFmpeg ?? Array.Empty<FFmpegActionSettingsModel>();
    }
    #endregion
}
