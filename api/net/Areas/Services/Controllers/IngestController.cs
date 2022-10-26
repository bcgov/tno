using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.API.Models;
using TNO.DAL.Services;
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
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceIngestState"></param>
    /// <param name="serviceIngest"></param>
    /// <param name="serializerOptions"></param>
    public IngestController(IIngestStateService serviceIngestState, IIngestService serviceIngest, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _serviceIngestState = serviceIngestState;
        _serviceIngest = serviceIngest;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a ingest for the specified 'id'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult FindByCode(int id)
    {
        var result = _serviceIngest.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find a ingest for the specified 'topic'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("for/topic/{topic}")]
    [Produces("application/json")]
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
    [Produces("application/json")]
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<IngestModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
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
    [HttpPut("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IngestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult Update(IngestModel model)
    {
        _serviceIngestState.AddOrUpdate(model.ToEntity(_serializerOptions).State!);

        var result = _serviceIngest.FindById(model.Id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new IngestModel(result, _serializerOptions));
    }
    #endregion
}
