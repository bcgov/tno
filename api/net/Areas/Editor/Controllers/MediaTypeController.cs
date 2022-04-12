using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.MediaType;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// MediaTypeController class, provides MediaType endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/media/types")]
[Route("api/[area]/media/types")]
[Route("v{version:apiVersion}/[area]/media/types")]
[Route("[area]/media/types")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class MediaTypeController : ControllerBase
{
    #region Variables
    private readonly IMediaTypeService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MediaTypeController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public MediaTypeController(IMediaTypeService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of MediaType.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "MediaType" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new MediaTypeModel(c)));
    }
    #endregion
}
