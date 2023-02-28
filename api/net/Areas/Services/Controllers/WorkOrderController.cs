using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.WorkOrder;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.API.SignalR;
using TNO.DAL.Services;
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
    private readonly IHubContext<MessageHub> _hub;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="hub"></param>
    /// <param name="serializerOptions"></param>
    public WorkOrderController(IWorkOrderService service, IHubContext<MessageHub> hub, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _hub = hub;
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
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public IActionResult FindById(long id)
    {
        var result = _service.FindById(id);
        if (result == null)
            return NoContent();

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
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "WorkOrder" })]
    public async Task<IActionResult> Update(WorkOrderModel workOrder)
    {
        var entity = _service.FindById(workOrder.Id);
        if (entity == null) throw new InvalidOperationException("Work order does not exist");

        var result = _service.UpdateAndSave(workOrder.CopyTo(entity, _serializerOptions));
        await _hub.Clients.All.SendAsync("WorkOrder", new WorkOrderMessageModel(result, _serializerOptions));
        return new JsonResult(new WorkOrderModel(entity, _serializerOptions));
    }
    #endregion
}
