using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.WorkOrder;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// WorkOrderController class, provides WorkOrder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/work/orders")]
[Route("api/[area]/work/orders")]
[Route("v{version:apiVersion}/[area]/work/orders")]
[Route("[area]/work/orders")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class WorkOrderController : ControllerBase
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly IWorkOrderHelper _workOrderHelper;
    private readonly IWorkOrderService _workOrderService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="workOrderService"></param>
    /// <param name="workOrderHelper"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderController(
        IContentService contentService,
        IWorkOrderService workOrderService,
        IWorkOrderHelper workOrderHelper,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _contentService = contentService;
        _workOrderService = workOrderService;
        _workOrderHelper = workOrderHelper;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of work orders for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<WorkOrderModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var filter = new WorkOrderFilter(query);
        var result = _workOrderService.Find(filter);
        var content = filter.ContentId.HasValue ? _contentService.FindById(filter.ContentId.Value, true) : null;
        var page = new Paged<WorkOrderModel>(result.Items.Select(i => new WorkOrderModel(i, content, _serializerOptions)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Request a transcript for the content for the specified 'contentId'.
    /// Publish message to kafka to request a transcription.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("transcribe/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> RequestTranscriptionAsync(long contentId)
    {
        var content = _contentService.FindById(contentId, true) ?? throw new NoContentException();
        if (content.Source?.DisableTranscribe == true) return BadRequest("Cannot request transcription");
        if (content.IsApproved)
        {
            // The transcript has already been approved, do not allow new requests.
            var workOrder = new Entities.WorkOrder(WorkOrderType.Transcription, "", content.Id, content.Headline);
            return new JsonResult(new WorkOrderModel(workOrder, content, _serializerOptions))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };
        }
        else
        {
            // If there is already a request it will return the existing one, or it will create a new request.
            var workOrder = await _workOrderHelper.RequestTranscriptionAsync(contentId);

            // TODO: Send email to requestor to confirm we have receive their request for a transcript.

            if (WorkOrderHelper.WorkLimiterStatus.Contains(workOrder.Status))
                return new JsonResult(new WorkOrderModel(workOrder, content, _serializerOptions))
                {
                    StatusCode = (int)HttpStatusCode.AlreadyReported
                };
            else
                return new JsonResult(new WorkOrderModel(workOrder, content, _serializerOptions));
        }
    }
    #endregion
}
