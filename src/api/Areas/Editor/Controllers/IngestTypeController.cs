using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.IngestType;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// IngestTypeController class, provides IngestType endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/ingest/types")]
[Route("api/[area]/ingest/types")]
[Route("v{version:apiVersion}/[area]/ingest/types")]
[Route("[area]/ingest/types")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class IngestTypeController : ControllerBase
{
    #region Variables
    private readonly IIngestTypeService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestTypeController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public IngestTypeController(IIngestTypeService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of IngestType.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<IngestTypeModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    [ETagCacheTableFilter("ingest_types")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new IngestTypeModel(c)));
    }
    #endregion
}
