using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Product;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// ProductController class, provides Product endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
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
    private readonly IProductService _productService;
    private readonly IUserService _userService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductController object, initializes with specified parameters.
    /// </summary>
    /// <param name="productService"></param>
    /// <param name="userService"></param>
    public ProductController(
        IProductService productService,
        IUserService userService)
    {
        _productService = productService;
        _userService = userService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find an array of product with the specified query filter.
    /// Only returns products owned by the current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ProductModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var filter = new ProductFilter(query)
        {
            OwnerId = user.Id
        };
        return new JsonResult(_productService.Find(filter).Select(ds => new ProductModel(ds)));
    }

    /// <summary>
    /// Find product with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult FindById(int id)
    {
        var result = _productService.FindById(id) ?? throw new NoContentException();
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        if (result.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to view this product");

        return new JsonResult(new ProductModel(result));
    }

    /// <summary>
    /// Add product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Add(ProductModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        model.OwnerId = user.Id;
        var result = _productService.AddAndSave(model.ToEntity());
        var product = _productService.FindById(result.Id) ?? throw new NoContentException("Product does not exist");
        return CreatedAtAction(nameof(FindById), new { id = product.Id }, new ProductModel(product));
    }

    /// <summary>
    /// Update product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Update(ProductModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var result = _productService.FindById(model.Id) ?? throw new NoContentException("Product does not exist");
        if (result.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to update this product");
        _productService.ClearChangeTracker(); // Remove the product from context.

        result = _productService.UpdateAndSave(model.ToEntity());
        var product = _productService.FindById(result.Id) ?? throw new NoContentException("Product does not exist");
        return new JsonResult(new ProductModel(product));
    }

    /// <summary>
    /// Delete product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Delete(ProductModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var result = _productService.FindById(model.Id) ?? throw new NoContentException("Product does not exist");
        if (result.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to delete this product");
        _productService.ClearChangeTracker(); // Remove the product from context.

        _productService.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }

    #endregion
}
