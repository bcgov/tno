using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// SourceController class, provides Source endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/sources")]
[Route("api/[area]/sources")]
[Route("v{version:apiVersion}/[area]/sources")]
[Route("[area]/sources")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class SourceController : ControllerBase
{
    #region Variables
    private readonly ISourceService _serviceSource;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SourceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceSource"></param>
    public SourceController(ISourceService serviceSource)
    {
        _serviceSource = serviceSource;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a source for the specified 'code'.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{code}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult FindByCode(string code)
    {
        var result = _serviceSource.FindByCode(code);
        if (result == null) return new NoContentResult();
        return new JsonResult(new SourceModel(result));
    }

    /// <summary>
    /// Get an array of sources.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<SourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Source" })]
    public IActionResult GetSources()
    {
        var result = _serviceSource.FindAll();
        return new JsonResult(result.Select(ds => new SourceModel(ds)));
    }
    #endregion
}
