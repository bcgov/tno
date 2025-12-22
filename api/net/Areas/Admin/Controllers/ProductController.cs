using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Product;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ProductController class, provides Product endpoints for the api.
/// </summary>
// [ClientRoleAuthorize(ClientRole.Administrator)]
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
    private readonly IProductService _productService;
    private readonly IReportService _reportService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProductController> _logger;
    private readonly Helpers.WatchSubscriptionChange _watch;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductController object, initializes with specified parameters.
    /// </summary>
    /// <param name="productService"></param>
    /// <param name="reportService"></param>
    /// <param name="notificationService"></param>
    /// <param name="watch"></param>
    /// <param name="logger"></param>
    public ProductController(
        IProductService productService,
        IReportService reportService,
        INotificationService notificationService,
        Helpers.WatchSubscriptionChange watch,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _reportService = reportService;
        _notificationService = notificationService;
        _watch = watch;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all products.
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
        var products = _productService.Find(new TNO.Models.Filters.ProductFilter(query));
        return new JsonResult(products.Select(p => new ProductModel(p)));
    }

    /// <summary>
    /// Find product with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult FindById(int id)
    {
        var product = _productService.FindById(id, true) ?? throw new NoContentException("Product does not exist");
        return new JsonResult(new ProductModel(product));
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
    public IActionResult Add([FromBody] ProductModel model)
    {
        // TODO: Update the notification or report subscribers.
        var product = _productService.AddAndSave(model.ToEntity());
        product = _productService.FindById(product.Id, true) ?? throw new NoContentException("Product does not exist");
        return new JsonResult(new ProductModel(product));
    }

    /// <summary>
    /// Update product with the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public async Task<IActionResult> UpdateAsync([FromBody] ProductModel model)
    {
        // TODO: Update the notification or report subscribers.
        var product = model.ToEntity();
        await _watch.AlertProductSubscriptionChangedAsync(product, this.User, "API Admin Product Controller Update endpoint.");
        product = _productService.UpdateAndSave(product);
        product = _productService.FindById(product.Id, true) ?? throw new NoContentException("Product does not exist");
        return new JsonResult(new ProductModel(product));
    }

    /// <summary>
    /// Delete product with the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Delete([FromBody] ProductModel model)
    {
        _productService.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }
    #endregion
}
