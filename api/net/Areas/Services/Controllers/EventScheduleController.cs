using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.EventSchedule;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// EventScheduleController class, provides event schedule endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/events/schedules")]
[Route("api/[area]/events/schedules")]
[Route("v{version:apiVersion}/[area]/events/schedules")]
[Route("[area]/events/schedules")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class EventScheduleController : ControllerBase
{
    #region Variables
    private readonly IEventScheduleService _serviceEventSchedule;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a EventScheduleController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceEventSchedule"></param>
    /// <param name="serializerOptions"></param>
    public EventScheduleController(IEventScheduleService serviceEventSchedule, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceEventSchedule = serviceEventSchedule;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Get an array of event schedules.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<EventScheduleModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult GetEventSchedules()
    {
        var result = _serviceEventSchedule.FindAll();
        return new JsonResult(result.Select(ds => new EventScheduleModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find a event schedule for the specified 'id'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EventScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult FindById(int id)
    {
        var result = _serviceEventSchedule.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new EventScheduleModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update data-service associated with the specified event schedule in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EventScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "EventSchedule" })]
    public IActionResult Update([FromBody] EventScheduleModel model)
    {
        _serviceEventSchedule.UpdateAndSave(model.ToEntity(_serializerOptions));

        var result = _serviceEventSchedule.FindById(model.Id) ?? throw new NoContentException();
        return new JsonResult(new EventScheduleModel(result, _serializerOptions));
    }
    #endregion
}
