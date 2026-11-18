using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Ingest;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// IngestController class, provides Ingest endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
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
    private readonly IIngestService _service;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="serializerOptions"></param>
    public IngestController(
        IIngestService service,
      IKafkaMessenger kafkaMessenger,
      IOptions<KafkaHubConfig> kafkaHubOptions,
      IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// retrieve all ingests
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<IngestModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new IngestModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find ingests based on the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("find")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<IngestModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new IngestFilter(query));
        var page = new Paged<IngestModel>(result.Items.Select(ds => new IngestModel(ds, _serializerOptions)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Retrieve ingest with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add a new ingest
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult Add([FromBody] IngestModel model)
    {
        var result = _service.AddAndSave(model.ToEntity(_serializerOptions));
        result = _service.FindById(result.Id) ?? throw new NoContentException();
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update ingest with the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public async Task<IActionResult> UpdateAsync([FromBody] IngestModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity(_serializerOptions), true);
        result = _service.FindById(result.Id) ?? throw new NoContentException();
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendGroup, "editor", new KafkaInvocationMessage(MessageTarget.IngestUpdated, new[] { new IngestMessageModel(result) })));
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update ingest with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    [HttpPut("{id}/enabled")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public async Task<IActionResult> UpdateIngestEnabledStatusAsync(int id, bool newStatus)
    {
        var model = _service.FindById(id) ?? throw new NoContentException();
        model.IsEnabled = newStatus;
        model = _service.UpdateAndSave(model, true);
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendGroup, "editor", new KafkaInvocationMessage(MessageTarget.IngestUpdated, new[] { new IngestMessageModel(model) })));
        return new JsonResult(new IngestModel(model, _serializerOptions));
    }

    /// <summary>
    /// Reset ingest state with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id}/resetfailures")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public async Task<IActionResult> UpdateIngestResetAsync(int id)
    {
        var model = _service.FindById(id) ?? throw new NoContentException();
        if (model.State != null)
        {
            model.State.FailedAttempts = 0;
        }
        else
        {
            model.State = new Entities.IngestState(model)
            {
                FailedAttempts = 0
            };
        }
        model = _service.UpdateAndSave(model, true);
        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic,
            new KafkaHubMessage(HubEvent.SendGroup, "editor", new KafkaInvocationMessage(MessageTarget.IngestUpdated, new[] { new IngestMessageModel(model) })));
        return new JsonResult(new IngestModel(model, _serializerOptions));
    }

    /// <summary>
    /// Delete ingest with the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult Delete([FromBody] IngestModel model)
    {
        _service.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
