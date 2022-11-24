using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Category;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// CategoryController class, provides Category endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/categories")]
[Route("api/[area]/categories")]
[Route("v{version:apiVersion}/[area]/categories")]
[Route("[area]/categories")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class CategoryController : ControllerBase
{
    #region Variables
    private readonly ICategoryService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CategoryController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public CategoryController(ICategoryService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Category.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<CategoryModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    [ETagCacheTableFilter("categories")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(c => new CategoryModel(c)));
    }
    #endregion
}
