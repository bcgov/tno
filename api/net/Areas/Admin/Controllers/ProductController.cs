using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Product;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ProductController class, provides Product endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/products")]
[Route("api/[area]/products")]
[Route("v{version:apiVersion}/[area]/products")]
[Route("[area]/products")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ProductController : ControllerBase
{
    #region Variables
    private readonly IProductService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ProductController(IProductService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<ProductModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new ProductModel(ds)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);

        if (result == null) return new NoContentResult();
        return new JsonResult(new ProductModel(result));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Add(ProductModel model)
    {
        var result = _service.AddAndSave(model.ToEntity());
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ProductModel(result));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Update(ProductModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity());
        return new JsonResult(new ProductModel(result));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Delete(ProductModel model)
    {
        _service.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
