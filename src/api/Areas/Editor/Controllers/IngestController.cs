using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Ingest;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// IngestController class, provides Ingest endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
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
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    public IngestController(IIngestService service, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
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
    /// Add a new schedule to the specified ingest.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("schedules")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ScheduleModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Ingest" })]
    public IActionResult AddSchedule([FromBody] IngestScheduleModel model)
    {
        var ingest = _service.FindById(model.IngestId) ?? throw new NoContentException();
        var schedule = model.ToEntity();
        ingest.Schedules.Add(schedule);
        _service.UpdateAndSave(ingest, true);
        return new JsonResult(new ScheduleModel(schedule));
    }
    #endregion
}
