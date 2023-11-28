using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Product;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        var publicFilter = new ProductFilter {
            IsPublic = true
        };
        var publicProducts = _productService.Find(publicFilter).Select(ds => new ProductModel(ds, user.Id));

        var privateFilter = new ProductFilter {
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var result = _productService.FindById(model.Id) ?? throw new NoContentException("Product does not exist");

        var isCurrentlySubscribed = result.SubscribersManyToMany.Exists(s => s.UserId == user.Id && s.IsSubscribed);

        if (isCurrentlySubscribed) {
            // This is the self-serve model for initial testing
            // TODO: Replace with Notification via Kafka
            await _productService.Unsubscribe(user.Id, result.Id);
        } else {
            // This is the self-serve model for initial testing
            // TODO: Replace with Notification via Kafka
            await _productService.Subscribe(user.Id, result.Id);
        }
        var productWithUpdatedSubscription = _productService.FindById(result.Id) ?? throw new NoContentException("Product does not exist");

        // TODO: Send Notification to Subscription manager via Kafka
        /*
        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, report.Id, new { })
        {
            RequestorId = user.Id,
            To = to,
            // no longer utilized, the caching mechanism
            // determines whether to update or not
            // UpdateCache = true,
            GenerateInstance = false
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{report.Id}-test", request);
        */

        return new JsonResult(new ProductModel(productWithUpdatedSubscription, user.Id));
    }

    #endregion
}
