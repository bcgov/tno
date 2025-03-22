using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.EventSchedule;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.SignalR;
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
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly IUserService _userService;
    private readonly IReportService _reportService;
    private readonly IEventScheduleService _eventScheduleService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a EventScheduleController object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaConfig"></param>
    /// <param name="userService"></param>
    /// <param name="reportService"></param>
    /// <param name="eventScheduleService"></param>
    /// <param name="serializerOptions"></param>
    public EventScheduleController(IKafkaMessenger kafkaMessenger, IOptions<KafkaHubConfig> kafkaConfig, IUserService userService, IReportService reportService, IEventScheduleService eventScheduleService, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaConfig.Value;
        _userService = userService;
        _reportService = reportService;
        _eventScheduleService = eventScheduleService;
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
        var result = _eventScheduleService.FindAll();
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
        var result = _eventScheduleService.FindById(id);
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
    public async Task<IActionResult> UpdateAsync([FromBody] EventScheduleModel model)
    {
        _eventScheduleService.UpdateAndSave(model.ToEntity(_serializerOptions));

        var result = _eventScheduleService.FindById(model.Id) ?? throw new NoContentException();
        if (result.ReportId.HasValue)
        {
            var report = _reportService.FindById(result.ReportId.Value);
            if (report?.OwnerId.HasValue == true)
            {
                var instance = _reportService.GetCurrentReportInstance(report.Id, report.OwnerId);
                var user = _userService.FindById(report.OwnerId.Value) ?? throw new NotAuthorizedException();
                await _kafkaMessenger.SendMessageAsync(
                    _kafkaHubOptions.HubTopic,
                    new KafkaHubMessage(HubEvent.SendUser, user.Username, new KafkaInvocationMessage(MessageTarget.ReportStatus, new[] { new ReportMessageModel()
                    {
                        Id = instance?.Id ?? 0,
                        ReportId = report.Id,
                        Status = instance?.Status ?? Entities.ReportStatus.Pending,
                        Subject = instance?.Subject ?? report.Name,
                        OwnerId = user.Id,
                        Message = "event",
                    } }))
                );
            }
        }
        return new JsonResult(new EventScheduleModel(result, _serializerOptions));
    }
    #endregion
}
