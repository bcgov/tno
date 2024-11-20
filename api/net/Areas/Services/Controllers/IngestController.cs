using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// IngestController class, provides Ingest endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/ingests")]
[Route("api/[area]/ingests")]
[Route("v{version:apiVersion}/[area]/ingests")]
[Route("[area]/ingests")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class IngestController : ControllerBase
{
    #region Variables
    private readonly IIngestStateService _serviceIngestState;
    private readonly IIngestService _serviceIngest;
    private readonly IScheduleService _serviceSchedule;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceIngestState"></param>
    /// <param name="serviceIngest"></param>
    /// <param name="serviceSchedule"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="serializerOptions"></param>
    public IngestController(
      IIngestStateService serviceIngestState,
      IIngestService serviceIngest,
      IScheduleService serviceSchedule,
      IKafkaMessenger kafkaMessenger,
      IOptions<KafkaHubConfig> kafkaHubOptions,
      IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceIngestState = serviceIngestState;
        _serviceIngest = serviceIngest;
        _serviceSchedule = serviceSchedule;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a ingest for the specified 'id'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult FindById(int id)
    {
        var result = _serviceIngest.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find a ingest for the specified 'topic'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("for/topic/{topic}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<IngestModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult FindByCode(string topic)
    {
        var result = _serviceIngest.FindByTopic(topic, true);
        return new JsonResult(result.Select(ds => new IngestModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find an array of ingests for the specified ingest type.
    /// </summary>
    /// <returns></returns>
    [HttpGet("for/ingest/type/{ingestTypeName}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<IngestModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult FindByIngestType(string ingestTypeName)
    {
        var result = _serviceIngest.FindByIngestType(ingestTypeName, true);
        return new JsonResult(result.Select(ds => new IngestModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Get an array of ingests.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<IngestModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    [ETagCacheTableFilter("ingests")]
    public IActionResult GetIngests()
    {
        var result = _serviceIngest.FindAll(true);
        return new JsonResult(result.Select(ds => new IngestModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Update data-service associated with the specified ingest in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/state")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public async Task<IActionResult> UpdateStateAsync([FromBody] IngestModel model)
    {
        _serviceIngestState.AddOrUpdate(model.ToEntity(_serializerOptions).State!);
        var result = _serviceIngest.FindById(model.Id) ?? throw new NoContentException();
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendGroup, "editor", new KafkaInvocationMessage(MessageTarget.IngestUpdated, new[] { new IngestMessageModel(result) })));
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update data-service associated with the specified ingest in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/configuration")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public async Task<IActionResult> UpdateConfigurationAsync([FromBody] IngestModel model)
    {
        var convertedEntity = model.ToEntity(_serializerOptions);
        var target = _serviceIngest.FindById(model.Id) ?? throw new NoContentException("Ingest does not exist");
        target.Configuration = convertedEntity.Configuration;
        _serviceIngest.UpdateAndSave(target);
        var result = _serviceIngest.FindById(model.Id) ?? throw new NoContentException();
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendGroup, "editor", new KafkaInvocationMessage(MessageTarget.IngestUpdated, new[] { new IngestMessageModel(result) })));
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Delete specified schedule from database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("schedules/{id:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult DeleteSchedule([FromBody] ScheduleModel model)
    {
        _serviceSchedule.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
