using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.WorkOrder;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.API.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// WorkOrderController class, provides WorkOrder endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
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
    private readonly IHubContext<MessageHub> _hub;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="hub"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderController(IWorkOrderService workOrderService, IKafkaMessenger kafkaMessenger, IOptions<KafkaHubConfig> kafkaHubOptions, IHubContext<MessageHub> hub, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _workOrderService = workOrderService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _hub = hub;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of work order for the specified query filter.
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
        var result = _workOrderService.Find(new WorkOrderFilter(query));
        var items = result.Items.Select(ds => new WorkOrderModel(ds, _serializerOptions));
        var page = new Paged<WorkOrderModel>(items, result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult FindById(int id)
    {
        var result = _workOrderService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new WorkOrderModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add work order for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Add([FromBody] WorkOrderModel model)
    {
        var result = _workOrderService.AddAndSave(model.ToEntity(_serializerOptions));
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new WorkOrderModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update work order for the specified 'id'.
    /// Update the work order in Keycloak if the 'Key' is linked.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> UpdateAsync([FromBody] WorkOrderModel model)
    {
        var entity = _workOrderService.FindById(model.Id) ?? throw new NoContentException();
        var result = _workOrderService.UpdateAndSave(model.CopyTo(entity, _serializerOptions));
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.WorkOrder, new[] { new WorkOrderMessageModel(result, _serializerOptions) })));
        return new JsonResult(new WorkOrderModel(result, _serializerOptions));
    }

    /// <summary>
    /// Delete work order for the specified 'id'.
    /// Delete the work order from Keycloak if the 'Key' is linked.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult Delete([FromBody] WorkOrderModel model)
    {
        _workOrderService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
