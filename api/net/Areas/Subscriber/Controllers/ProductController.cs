using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Product;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Keycloak;

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
    /// Find all products - returns subscription status for current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ProductModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult FindMyProducts()
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        return new JsonResult(_productService.FindProductsByUser(user.Id).Select(ds => new ProductModel(ds)));
    }

    /// <summary>
    /// Find all "available" products.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ProductModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult GetProducts()
    {
        return new JsonResult(_productService.FindAll().Select(ds => new ProductModel(ds)));
    }

    /// <summary>
    /// Create a subscribe request for the current user to a product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult SubscribeCurrentUser(ProductModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var result = _productService.FindById(model.Id) ?? throw new NoContentException("Product does not exist");

        // If the current user is not already subscribed, add them to the list of subscribers.
        if (result.SubscribersManyToMany.Exists(s => s.UserId == user.Id && !s.IsSubscribed)) {
            throw new NotAuthorizedException("User is already subscribed");
        } else {
            // This is the self-serve model for initial testing
            // TODO: Replace with Notification via Kafka
            _productService.Subscribe(user.Id, result.Id);

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
        }
        var product = _productService.FindById(model.Id) ?? throw new NoContentException("Report does not exist");
        return new JsonResult(new ProductModel(product));

    }

    /// <summary>
    /// Create an un-subscribe request for the current user to a product.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Product" })]
    public IActionResult Unsubscribe(ProductModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var result = _productService.FindById(model.Id) ?? throw new NoContentException("Product does not exist");

        // Add the current user as a subscriber if they are not already subscribed.
        if (!result.SubscribersManyToMany.Exists(s => s.UserId == user.Id && s.IsSubscribed)) {
            throw new NotAuthorizedException("User not currently subscribed");
        } else {

            // This is the self-serve model for initial testing
            // TODO: Replace with Notification via Kafka
            _productService.Unsubscribe(user.Id, result.Id);

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
        }
        var product = _productService.FindById(model.Id) ?? throw new NoContentException("Report does not exist");
        return new JsonResult(model);
    }
    #endregion
}
