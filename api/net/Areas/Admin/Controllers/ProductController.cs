using System.Net;
using System.Net.Mime;
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
    private readonly ILogger<ProductController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductController object, initializes with specified parameters.
    /// </summary>
    /// <param name="productService"></param>
    /// <param name="reportService"></param>
    /// <param name="logger"></param>
    public ProductController(
        IProductService productService,
        IReportService reportService,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _reportService = reportService;
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
    public IActionResult FindAll()
    {
        var products = _productService.FindAll();
        var reportIds = products.Where(p => p.ProductType == Entities.ProductType.Report).Select(p => p.TargetProductId).ToArray();
        var reports = reportIds.Any() ? _reportService.Find(new TNO.Models.Filters.ReportFilter() { Ids = reportIds }) : Array.Empty<Entities.Report>();
        return new JsonResult(products.Select(ds => new ProductModel(ds, ds.ProductType == Entities.ProductType.Report ? reports.FirstOrDefault(r => r.Id == ds.TargetProductId) : null)));
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
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult FindById(int id)
    {
        var result = _productService.FindById(id);
        if (result == null) return new NoContentResult();
        var report = result.ProductType == Entities.ProductType.Report ? _reportService.FindById(result.TargetProductId) : null;
        return new JsonResult(new ProductModel(result, report));
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
        // TODO: Update the notification or report subscribers.
        var result = _productService.AddAndSave(model.ToEntity());
        var product = _productService.FindById(result.Id) ?? throw new NoContentException("Product does not exist");
        var report = result.ProductType == Entities.ProductType.Report ? _reportService.FindById(result.TargetProductId) : null;
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ProductModel(product, report));
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
    public IActionResult Update(ProductModel model)
    {
        // TODO: Update the notification or report subscribers.
        var result = _productService.UpdateAndSave(model.ToEntity());
        var product = _productService.FindById(result.Id) ?? throw new NoContentException("Product does not exist");
        var report = result.ProductType == Entities.ProductType.Report ? _reportService.FindById(result.TargetProductId) : null;
        return new JsonResult(new ProductModel(product, report));
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
    public IActionResult Delete(ProductModel model)
    {
        _productService.DeleteAndSave(model.ToEntity());
        return new JsonResult(model);
    }

    #endregion
}
