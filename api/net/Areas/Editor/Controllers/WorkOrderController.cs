using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Content;
using TNO.API.Config;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;
namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// WorkOrderController class, provides WorkOrder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
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
    private readonly IWorkOrderService _workOrderService;
    private readonly IContentService _contentService;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly IHubContext<WorkOrderHub> _hub;

    // The following work order status ensure only a single request can be completed for content.
    private readonly IEnumerable<WorkOrderStatus> _workLimiterStatus = new[] { WorkOrderStatus.Submitted, WorkOrderStatus.InProgress };
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderService"></param>
    /// <param name="contentService"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="hub"></param>
    /// <param name="kafkaOptions"></param>
    public WorkOrderController(
        IWorkOrderService workOrderService,
        IContentService contentService,
        IUserService userService,
        IKafkaMessenger kafkaMessenger,
        IHubContext<WorkOrderHub> hub,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _workOrderService = workOrderService;
        _contentService = contentService;
        _userService = userService;
        _kafkaMessenger = kafkaMessenger;
        _hub = hub;
        _kafkaOptions = kafkaOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of work order for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<WorkOrderModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _workOrderService.Find(new WorkOrderFilter(query));
        var items = result.Items.Select(ds => new WorkOrderModel(ds));
        var page = new Paged<WorkOrderModel>(items, result.Page, result.Quantity, items.Count());
        return new JsonResult(page);
    }

    /// <summary>
    /// Find work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult FindById(int id)
    {
        var result = _workOrderService.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new WorkOrderModel(result));
    }

    /// <summary>
    /// Add work order for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Add(WorkOrderModel model)
    {
        var result = _workOrderService.Add((WorkOrder)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new WorkOrderModel(result));
    }

    /// <summary>
    /// Update work order for the specified 'id'.
    /// Update the work order in Keycloak if the 'Key' is linked.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> UpdateAsync(WorkOrderModel model)
    {
        var entity = _workOrderService.FindById(model.Id);
        if (entity == null) throw new InvalidOperationException("Work order not found");
        var result = _workOrderService.Update(model.CopyTo(entity));
        await _hub.Clients.All.SendAsync("Update", model.ContentId);
        return new JsonResult(new WorkOrderModel(result));
    }

    /// <summary>
    /// Delete work order for the specified 'id'.
    /// Delete the work order from Keycloak if the 'Key' is linked.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Delete(WorkOrderModel model)
    {
        _workOrderService.Delete((WorkOrder)model);
        return new JsonResult(model);
    }
    /// <summary>
    /// Request a transcript for the content for the specified 'contentId'.
    /// Publish message to kafka to request a transcription.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPut("transcribe/{contentId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> RequestTranscriptionAsync(long contentId)
    {
        var content = _contentService.FindById(contentId) ?? throw new InvalidOperationException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.TranscriptionTopic)) throw new ConfigurationException("Kafka transcription topic not configured.");

        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (workOrders.Any(o => o.WorkType == WorkOrderType.Transcription && _workLimiterStatus.Contains(o.Status)))
            return new JsonResult(new WorkOrderModel(workOrders.First(o => o.WorkType == WorkOrderType.Transcription && _workLimiterStatus.Contains(o.Status))))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var workOrder = _workOrderService.Add(new WorkOrder(WorkOrderType.Transcription, content, user, ""));

        var result = await _kafkaMessenger.SendMessageAsync(_kafkaOptions.TranscriptionTopic, new TranscriptRequest(workOrder, username));
        if (result == null)
        {
            workOrder.Status = WorkOrderStatus.Failed;
            workOrder.Note = "Transcript request to Kafka failed";
            _workOrderService.Update(workOrder);
            await _hub.Clients.All.SendAsync("Update", workOrder.ContentId);
            throw new BadRequestException("Transcription request failed");
        }
        return new JsonResult(new WorkOrderModel(workOrder));
    }

    /// <summary>
    /// Request a Natural Language Processing for the content for the specified 'contentId'.
    /// Publish message to kafka to request a NLP.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPut("nlp/{contentId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> RequestNLPAsync(long contentId)
    {
        var content = _contentService.FindById(contentId) ?? throw new InvalidOperationException("Content does not exist");
        if (String.IsNullOrWhiteSpace(_kafkaOptions.NLPTopic)) throw new ConfigurationException("Kafka NLP topic not configured.");

        // Only allow one work order transcript request at a time.
        // TODO: Handle blocked work orders stuck in progress.
        var workOrders = _workOrderService.FindByContentId(contentId);
        if (workOrders.Any(o => o.WorkType == WorkOrderType.NaturalLanguageProcess && _workLimiterStatus.Contains(o.Status)))
            return new JsonResult(new WorkOrderModel(workOrders.First(o => o.WorkType == WorkOrderType.NaturalLanguageProcess && _workLimiterStatus.Contains(o.Status))))
            {
                StatusCode = (int)HttpStatusCode.AlreadyReported
            };

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var workOrder = _workOrderService.Add(new WorkOrder(WorkOrderType.NaturalLanguageProcess, content, user, ""));

        var result = await _kafkaMessenger.SendMessageAsync(_kafkaOptions.NLPTopic, new NLPRequest(workOrder));
        if (result == null)
        {
            workOrder.Status = WorkOrderStatus.Failed;
            workOrder.Note = "NLP request to Kafka failed";
            _workOrderService.Update(workOrder);
            await _hub.Clients.All.SendAsync("Update", workOrder.ContentId);
            throw new BadRequestException("Natural Language Processing request failed");
        }
        return new JsonResult(new WorkOrderModel(workOrder));
    }
    #endregion
}
