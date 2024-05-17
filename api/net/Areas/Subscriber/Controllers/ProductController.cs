using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Product;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Ches;
using TNO.Ches.Configuration;
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
    private readonly IUserService _userService;
    private readonly ISettingService _settingService;
    private readonly IImpersonationHelper _impersonate;
    private readonly IChesService _ches;
    private readonly ChesOptions _chesOptions;
    private readonly ILogger<ProductController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductController object, initializes with specified parameters.
    /// </summary>
    /// <param name="productService"></param>
    /// <param name="userService"></param>
    /// <param name="impersonateHelper"></param>
    /// <param name="settingService"></param>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="logger"></param>
    public ProductController(
        IProductService productService,
        IUserService userService,
        IImpersonationHelper impersonateHelper,
        ISettingService settingService,
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _userService = userService;
        _impersonate = impersonateHelper;
        _settingService = settingService;
        _ches = ches;
        _chesOptions = chesOptions.Value;
        _logger = logger;
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

        var publicFilter = new ProductFilter
        {
            IsPublic = true
        };
        var publicProducts = _productService.Find(publicFilter).Select(ds => new ProductModel(ds, user.Id));

        var privateFilter = new ProductFilter
        {
            SubscriberUserId = user.Id
        };
        var privateProducts = _productService.Find(privateFilter).Select(ds => new ProductModel(ds, user.Id));

        var results = publicProducts.Union(privateProducts).Distinct().ToList();

        return new JsonResult(results);
    }

    /// <summary>
    /// Create a subscribe/unsubscribe request for the current user to a product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}/togglesubscription")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public async Task<IActionResult> ToggleSubscriptionForCurrentUser(ProductModel model)
    {
        var user = _impersonate.GetCurrentUser();
        var result = _productService.FindById(model.Id) ?? throw new NoContentException("Product does not exist");

        var userProductSubscription = result.SubscribersManyToMany.FirstOrDefault(s => s.UserId == user.Id);
        if (userProductSubscription != null && userProductSubscription.RequestedIsSubscribedStatus.HasValue)
        {
            await _productService.CancelSubscriptionStatusChangeRequest(user.Id, result.Id);
            // don't think we need to send an email here...
        }
        else
        {
            string subject = string.Empty;
            StringBuilder message = new StringBuilder();
            message.AppendLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
            message.AppendLine("<HTML>");
            message.AppendLine("<BODY>");
            message.AppendLine($"<p><strong>User Name</strong>: {user.DisplayName}</p>");
            message.AppendLine($"<p><strong>User Email</strong>: {user.Email}</p>");
            message.AppendLine($"<p><strong>Product</strong>: {result.Name}</p>");

            if (userProductSubscription == null || !userProductSubscription.IsSubscribed)
            {
                await _productService.RequestSubscribe(user.Id, result.Id);

                subject = $"MMI: Product Subscription request - [{result.Name}]";
                message.AppendLine($"<p><strong>Action</strong>: SUBSCRIBE</p>");
            }
            else if (userProductSubscription.IsSubscribed)
            {
                await _productService.RequestUnsubscribe(user.Id, result.Id);

                subject = $"MMI: Product Unsubscription request - [{result.Name}]";
                message.AppendLine($"<p><strong>Action</strong>: UNSUBSCRIBE</p>");
            }

            message.AppendLine("</BODY>");
            message.AppendLine("</HTML>");

            try
            {
                var productSubscriptionManagerEmail = _settingService.FindByName(AdminConfigurableSettingNames.ProductSubscriptionManagerEmail.ToString());
                if (productSubscriptionManagerEmail != null)
                {
                    var email = new TNO.Ches.Models.EmailModel(_chesOptions.From, productSubscriptionManagerEmail.Value, subject, message.ToString());
                    var emailRequest = await _ches.SendEmailAsync(email);
                    _logger.LogInformation($"Product subscription request email to [${productSubscriptionManagerEmail.Value}] queued: ${emailRequest.TransactionId}");
                }
                else
                {
                    _logger.LogError("Couldn't send product subscription request email: [ProductSubscriptionManagerEmail] not set.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email failed to send");
            }
        }

        var productWithUpdatedSubscription = _productService.FindById(result.Id) ?? throw new NoContentException("Product does not exist");

        return new JsonResult(new ProductModel(productWithUpdatedSubscription, user.Id));
    }

    #endregion
}
