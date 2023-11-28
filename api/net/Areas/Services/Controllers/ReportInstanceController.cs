using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ReportInstance;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ReportInstanceController class, provides ReportInstance endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/report/instances")]
[Route("api/[area]/report/instances")]
[Route("v{version:apiVersion}/[area]/report/instances")]
[Route("[area]/report/instances")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportInstanceController : ControllerBase
{
    #region Variables
    private readonly IReportInstanceService _reportInstanceService;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportInstanceService"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportInstanceController(
        IReportInstanceService reportInstanceService,
        IUserService userService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportInstanceService = reportInstanceService;
        _userService = userService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find report instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult FindById(long id)
    {
        var result = _reportInstanceService.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add report instance for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult Add(ReportInstanceModel model)
    {
        var result = _reportInstanceService.AddAndSave((Entities.ReportInstance)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update report instance for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public async Task<IActionResult> UpdateAsync(ReportInstanceModel model)
    {
        var result = _reportInstanceService.UpdateAndSave((Entities.ReportInstance)model) ?? throw new NoContentException();
        var ownerId = result.OwnerId ?? result.Report?.OwnerId;
        if (ownerId.HasValue)
        {
            var user = _userService.FindById(ownerId.Value) ?? throw new NotAuthorizedException();
            await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendUser, user.Username, new KafkaInvocationMessage(MessageTarget.ReportStatus, new[] { new ReportMessageModel(result) })));
        }

        return new JsonResult(new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Make a request to the database for all content assigned to the specified report instance 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/content")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportInstanceContentModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult GetContentForReportInstanceIdAsync(long id)
    {
        var content = _reportInstanceService.GetContentForInstance(id);
        return new JsonResult(content.Select(c => new ReportInstanceContentModel(c)));
    }
    #endregion
}
