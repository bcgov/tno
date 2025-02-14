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
    public IActionResult Add([FromBody] ReportInstanceModel model)
    {
        var result = _reportInstanceService.AddAndSave((Entities.ReportInstance)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update report instance for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateContent"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public async Task<IActionResult> UpdateAsync([FromBody] ReportInstanceModel model, bool updateContent = true)
    {
        var result = _reportInstanceService.UpdateAndSave((Entities.ReportInstance)model, updateContent) ?? throw new NoContentException();
        var ownerId = result.OwnerId ?? result.Report?.OwnerId;
        if (ownerId.HasValue)
        {
            var user = _userService.FindById(ownerId.Value) ?? throw new NotAuthorizedException();
            await _kafkaMessenger.SendMessageAsync(
                _kafkaHubOptions.HubTopic,
                new KafkaHubMessage(HubEvent.SendUser, user.Username, new KafkaInvocationMessage(MessageTarget.ReportStatus, new[] { new ReportMessageModel(result) }))
            );
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


    /// <summary>
    /// Get all user report instances for the specified instance 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/responses")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<UserReportInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult GetUserReportInstancesAsync(long id)
    {
        var content = _reportInstanceService.GetUserReportInstances(id);
        return new JsonResult(content.Select(c => new UserReportInstanceModel(c)));
    }

    /// <summary>
    /// Add or update an array of user report instances.
    /// These keep track of who a report was sent to and the status.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("response")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult AddOrUpdate([FromBody] UserReportInstanceModel model)
    {
        var result = _reportInstanceService.UpdateAndSave((Entities.UserReportInstance)model);
        return new JsonResult(new UserReportInstanceModel(result));
    }

    /// <summary>
    /// Add or update an array of user report instances.
    /// These keep track of who a report was sent to and the status.
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    [HttpPost("responses")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<UserReportInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult AddOrUpdate([FromBody] IEnumerable<UserReportInstanceModel> models)
    {
        var entities = models.Select(m => (Entities.UserReportInstance)m);
        var result = _reportInstanceService.UpdateAndSave(entities);
        return new JsonResult(result.Select(r => new UserReportInstanceModel(r)));
    }

    /// <summary>
    /// Update the user report instance status.
    /// </summary>
    /// <param name="id">The report instance id.</param>
    /// <param name="format">The report email type [link, text].</param>
    /// <param name="userId">The user id.</param>
    /// <param name="status">The status to update to.</param>
    /// <returns></returns>
    [HttpPut("{id}/{format}/{userId}/status/{status}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult UpdateUserReportInstanceStatus(long id, int userId, Entities.ReportDistributionFormat format, Entities.ReportStatus status)
    {
        var userInstance = _reportInstanceService.GetUserReportInstance(id, userId) ?? throw new NoContentException();
        if (format == Entities.ReportDistributionFormat.LinkOnly)
        {
            userInstance.LinkStatus = status;
        }
        else if (format == Entities.ReportDistributionFormat.FullText)
        {
            userInstance.TextStatus = status;
        }
        else throw new BadRequestException("Parameter 'type' must be 'link' or 'text'.");
        _reportInstanceService.UpdateAndSave(userInstance);
        return new JsonResult(new UserReportInstanceModel(userInstance));
    }

    /// <summary>
    /// Update the report instance status.
    /// </summary>
    /// <param name="id">The report instance id.</param>
    /// <param name="status">The status to update to.</param>
    /// <returns></returns>
    [HttpPut("{id}/status/{status}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult UpdateReportInstanceStatus(long id, Entities.ReportStatus status)
    {
        var instance = _reportInstanceService.FindById(id) ?? throw new NoContentException();
        instance.Status = status;
        _reportInstanceService.UpdateAndSave(instance);
        return new JsonResult(new ReportInstanceModel(instance, _serializerOptions));
    }
    #endregion
}
