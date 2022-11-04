using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.WorkOrder;
using TNO.API.Keycloak;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;

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
    private readonly IKeycloakHelper _keycloakHelper;
    private readonly IHubContext<WorkOrderHub> _hub;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderController object, initializes with specified parameters.
    /// </summary>
    /// <param name="workOrderService"></param>
    /// <param name="keycloakHelper"></param>
    /// <param name="hub"></param>
    public WorkOrderController(IWorkOrderService workOrderService, IKeycloakHelper keycloakHelper, IHubContext<WorkOrderHub> hub)
    {
        _workOrderService = workOrderService;
        _keycloakHelper = keycloakHelper;
        _hub = hub;
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
    public async Task<IActionResult> Update(WorkOrderModel model)
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
    #endregion
}
