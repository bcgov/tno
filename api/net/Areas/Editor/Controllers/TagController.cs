using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Tag;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// TagController class, provides Tag endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/tags")]
[Route("api/[area]/tags")]
[Route("v{version:apiVersion}/[area]/tags")]
[Route("[area]/tags")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TagController : ControllerBase
{
    #region Variables
    private readonly ITagService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TagController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public TagController(ITagService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Tag.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<TagModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Tag" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new TagModel(c)));
    }
    #endregion
}
