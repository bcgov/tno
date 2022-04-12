using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.SourceMetric;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// SourceMetricController class, provides SourceMetric endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/source/metrics")]
[Route("api/[area]/source/metrics")]
[Route("v{version:apiVersion}/[area]/source/metrics")]
[Route("[area]/source/metrics")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class SourceMetricController : ControllerBase
{
    #region Variables
    private readonly ISourceMetricService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SourceMetricController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public SourceMetricController(ISourceMetricService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of SourceMetric.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<SourceMetricModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "SourceMetric" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new SourceMetricModel(c)));
    }
    #endregion
}
