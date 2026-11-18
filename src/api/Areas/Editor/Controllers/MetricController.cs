using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Metric;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// MetricController class, provides Metric endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/metrics")]
[Route("api/[area]/metrics")]
[Route("v{version:apiVersion}/[area]/metrics")]
[Route("[area]/metrics")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class MetricController : ControllerBase
{
    #region Variables
    private readonly IMetricService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MetricController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public MetricController(IMetricService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Metric.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<MetricModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "Metric" })]
    [ETagCacheTableFilter("metrics")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new MetricModel(c)));
    }
    #endregion
}
