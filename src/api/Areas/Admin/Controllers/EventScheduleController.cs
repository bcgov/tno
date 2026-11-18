using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.EventSchedule;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// EventScheduleController class, provides event schedule endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/event/schedules")]
[Route("api/[area]/event/schedules")]
[Route("v{version:apiVersion}/[area]/event/schedules")]
[Route("[area]/event/schedules")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class EventScheduleController : ControllerBase
{
    #region Variables
    private readonly IEventScheduleService _eventScheduleService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a EventScheduleController object, initializes with specified parameters.
    /// </summary>
    /// <param name="eventScheduleService"></param>
    public EventScheduleController(
        IEventScheduleService eventScheduleService)
    {
        _eventScheduleService = eventScheduleService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all event schedule.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<EventScheduleModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_eventScheduleService.FindAll().Select(ds => new EventScheduleModel(ds)));
    }

    /// <summary>
    /// Find event schedule for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EventScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult FindById(int id)
    {
        var result = _eventScheduleService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new EventScheduleModel(result));
    }

    /// <summary>
    /// Add event schedule for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EventScheduleModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult Add([FromBody] EventScheduleModel model)
    {
        var result = _eventScheduleService.AddAndSave((Entities.EventSchedule)model);
        var eventSchedule = _eventScheduleService.FindById(result.Id) ?? throw new NoContentException("EventSchedule does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new EventScheduleModel(eventSchedule));
    }

    /// <summary>
    /// Update event schedule for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EventScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult Update([FromBody] EventScheduleModel model)
    {
        var result = _eventScheduleService.UpdateAndSave((Entities.EventSchedule)model);
        var eventSchedule = _eventScheduleService.FindById(result.Id) ?? throw new NoContentException("EventSchedule does not exist");
        return new JsonResult(new EventScheduleModel(eventSchedule));
    }

    /// <summary>
    /// Delete event schedule for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EventScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult Delete([FromBody] EventScheduleModel model)
    {
        _eventScheduleService.DeleteAndSave((Entities.EventSchedule)model);
        return new JsonResult(model);
    }
    #endregion
}
