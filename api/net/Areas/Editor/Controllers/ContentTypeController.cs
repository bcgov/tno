using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.ContentType;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// ContentTypeController class, provides ContentType endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/content/types")]
[Route("api/[area]/content/types")]
[Route("v{version:apiVersion}/[area]/content/types")]
[Route("[area]/content/types")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentTypeController : ControllerBase
{
    #region Variables
    private readonly IContentTypeService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentTypeController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ContentTypeController(IContentTypeService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of ContentType.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<ContentTypeModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "ContentType" })]
    [ETagCacheTableFilter("content_types")]
    [ResponseCache(Duration = 7 * 24 * 60 * 60)]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new ContentTypeModel(c)));
    }
    #endregion
}
