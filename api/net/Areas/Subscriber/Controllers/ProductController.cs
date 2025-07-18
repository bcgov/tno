using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Product;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ProductController class, provides Product endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    private readonly IImpersonationHelper _impersonate;
    
    private const ReportDistributionFormat DefaultFormat = ReportDistributionFormat.LinkOnly;
    private const EmailSentTo DefaultSendTo = EmailSentTo.To;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductController object, initializes with specified parameters.
    /// </summary>
    /// <param name="productService"></param>
    /// <param name="impersonateHelper"></param>
    /// <param name="reportService"></param>
    public ProductController(
        IProductService productService,
        IImpersonationHelper impersonateHelper,
        IReportService reportService)
    {
        _productService = productService;
        _impersonate = impersonateHelper;
        _reportService = reportService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all "available" products.
    /// For Subscribers, this means all Public products AND Non-Public Products that they have been subscribed to by an admin
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ProductModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult GetProducts()
    {
        var user = _impersonate.GetCurrentUser();
        var products = _productService.Find(new ProductFilter
        {
            IsEnabled = true,
            IsAvailableToUserId = user.Id,
        }).Select(ds => new ProductModel(ds));
        return new JsonResult(products);
    }

    /// <summary>
    /// Create a subscribe/unsubscribe request for the current user to a product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}/subscription")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserProductModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public async Task<IActionResult> UpdateSubscription([FromBody] UserProductModel model)
    {
        var user = _impersonate.GetCurrentUser();
        var originalProduct = _productService.FindById(model.ProductId) ?? throw new NoContentException("Product does not exist");
        bool createNewSubscription = false;

        var userProductSubscription = originalProduct.SubscribersManyToMany.FirstOrDefault(s => s.UserId == user.Id);
        if (userProductSubscription == null)
        {
            // Add a request to subscribe to this product.
            userProductSubscription = new UserProduct(user.Id, model.ProductId, model.Status);
            _productService.AddAndSave(userProductSubscription);
            createNewSubscription = true;
        }
        else
        {
            userProductSubscription.Status = model.Status;
            _productService.UpdateAndSave(userProductSubscription);
        }

        if (userProductSubscription.Status == ProductRequestStatus.RequestSubscription ||
           userProductSubscription.Status == ProductRequestStatus.RequestUnsubscribe)
        {
            await _productService.SendSubscriptionRequestEmailAsync(userProductSubscription);
        }

        // update the report subscription format and send to
        if (userProductSubscription.Status == ProductRequestStatus.RequestSubscription && originalProduct.ProductType == ProductType.Report)
        {
            if (createNewSubscription)
            {
                var newReportSubscription = new UserReport(user.Id, originalProduct.TargetProductId, false,
                                            model.Format ?? DefaultFormat, model.SendTo ?? DefaultSendTo);
                _reportService.AddAndSave(newReportSubscription);
            }
            else
            {
                var originalReport = _reportService.FindById(originalProduct.TargetProductId) ?? null;
                if (originalReport != null)
                {
                    var originalUserReport = originalReport.SubscribersManyToMany
                                            .Where(x => x.UserId == user.Id && x.ReportId == originalProduct.TargetProductId).FirstOrDefault();
                    if (originalUserReport != null)
                    {
                        originalUserReport.Format = model.Format ?? DefaultFormat;
                        originalUserReport.SendTo = model.SendTo ?? DefaultSendTo;
                        _reportService.UpdateAndSave(originalUserReport);
                    }
                    else
                    {
                        var newReportSubscription = new UserReport(user.Id, originalProduct.TargetProductId, false);
                        _reportService.AddAndSave(newReportSubscription);
                    }
                }
            }
        }
        return new JsonResult(new UserProductModel(userProductSubscription));
    }
    #endregion
}
