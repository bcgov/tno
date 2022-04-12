using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.DataLocation;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// DataLocationController class, provides DataLocation endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/data/locations")]
[Route("api/[area]/data/locations")]
[Route("v{version:apiVersion}/[area]/data/locations")]
[Route("[area]/data/locations")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class DataLocationController : ControllerBase
{
    #region Variables
    private readonly IDataLocationService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataLocationController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public DataLocationController(IDataLocationService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of DataLocation.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<DataLocationModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "DataLocation" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new DataLocationModel(c)));
    }
    #endregion
}
