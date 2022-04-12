using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Series;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// SeriesController class, provides Series endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/series")]
[Route("api/[area]/series")]
[Route("v{version:apiVersion}/[area]/series")]
[Route("[area]/series")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class SeriesController : ControllerBase
{
    #region Variables
    private readonly ISeriesService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SeriesController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public SeriesController(ISeriesService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Series.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<SeriesModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Series" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new SeriesModel(c)));
    }
    #endregion
}
