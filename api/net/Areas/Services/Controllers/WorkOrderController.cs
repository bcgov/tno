using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.WorkOrder;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// WorkOrderController class, provides WorkOrder endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
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
    private readonly IWorkOrderService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderController(IWorkOrderService service, IKafkaMessenger kafkaMessenger, IOptions<KafkaHubConfig> kafkaHubOptions, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult FindById(long id)
    {
        var result = _service.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new WorkOrderModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update the work order in the data source.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(WorkOrderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> Update(WorkOrderModel workOrder)
    {
        var entity = _service.FindById(workOrder.Id) ?? throw new NoContentException();

        var result = _service.UpdateAndSave(workOrder.CopyTo(entity, _serializerOptions));
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.WorkOrder, new[] { new WorkOrderMessageModel(result, _serializerOptions) })));
        return new JsonResult(new WorkOrderModel(entity, _serializerOptions));
    }
    #endregion
}
