using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Category;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// CategoryController class, provides category endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
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
    /// Find a page of category for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<CategoryModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new CategoryModel(ds)));
    }

    /// <summary>
    /// Find a page of category for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<CategoryModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new CategoryFilter(query));
        var page = new Paged<CategoryModel>(result.Items.Select(ds => new CategoryModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find category for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new CategoryModel(result));
    }

    /// <summary>
    /// Add a category.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    public IActionResult Add(CategoryModel model)
    {
        var result = _service.Add((Category)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new CategoryModel(result));
    }

    /// <summary>
    /// Update category for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    public IActionResult Update(CategoryModel model)
    {
        var result = _service.Update((Category)model);
        return new JsonResult(new CategoryModel(result));
    }

    /// <summary>
    /// Delete category for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Category" })]
    public IActionResult Delete(CategoryModel model)
    {
        _service.Delete((Category)model);
        return new JsonResult(model);
    }
    #endregion
}
