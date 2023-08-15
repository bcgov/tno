using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ReportInstance;
using TNO.API.Config;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ReportInstanceController class, provides ReportInstance endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
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
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportInstanceService"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportInstanceController(
        IReportInstanceService reportInstanceService,
        IUserService userService,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportInstanceService = reportInstanceService;
        _userService = userService;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult FindById(long id)
    {
        var result = _reportInstanceService.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add content for the specified 'id'.
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
        var result = _reportInstanceService.AddAndSave(model.ToEntity(_serializerOptions));
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult Update(ReportInstanceModel model)
    {
        var result = _reportInstanceService.UpdateAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(new ReportInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult Delete(ReportInstanceModel model)
    {
        _reportInstanceService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }

    /// <summary>
    /// Publish the report instance and send to all subscribers.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/publish")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public async Task<IActionResult> Publish(int id)
    {
        var instance = _reportInstanceService.FindById(id);
        if (instance == null) return new NoContentResult();

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, instance.ReportId, instance.Id, new { })
        {
            RequestorId = user.Id
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{instance.ReportId}", request);
        return new JsonResult(new ReportInstanceModel(instance, _serializerOptions));
    }
    #endregion
}
