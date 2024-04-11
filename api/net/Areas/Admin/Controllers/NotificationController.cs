using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Notification;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;
using TNO.Models.Filters;
using TNO.TemplateEngine.Models.Notifications;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// NotificationController class, provides Notification endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/notifications")]
[Route("api/[area]/notifications")]
[Route("v{version:apiVersion}/[area]/notifications")]
[Route("[area]/notifications")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class NotificationController : ControllerBase
{
    #region Variables
    private readonly INotificationService _notificationService;
    private readonly IContentService _contentService;
    private readonly IUserService _userService;
    private readonly INotificationHelper _notificationHelper;
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationController object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationService"></param>
    /// <param name="contentService"></param>
    /// <param name="userService"></param>
    /// <param name="notificationHelper"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    public NotificationController(
        INotificationService notificationService,
        IContentService contentService,
        IUserService userService,
        INotificationHelper notificationHelper,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _notificationService = notificationService;
        _contentService = contentService;
        _userService = userService;
        _notificationHelper = notificationHelper;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all notifications for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<NotificationModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var results = _notificationService.Find(new NotificationFilter(query));
        return new JsonResult(results.Select(ds => new NotificationModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find notification for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult FindById(int id)
    {
        var result = _notificationService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new NotificationModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add notification.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Add(NotificationModel model)
    {
        var result = _notificationService.AddAndSave(model.ToEntity(_serializerOptions, true));
        result = _notificationService.FindById(result.Id) ?? throw new NoContentException();
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new NotificationModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update notification for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Update(NotificationModel model)
    {
        var result = _notificationService.UpdateAndSave(model.ToEntity(_serializerOptions, true));
        result = _notificationService.FindById(result.Id) ?? throw new NoContentException();
        return new JsonResult(new NotificationModel(result, _serializerOptions));
    }

    /// <summary>
    /// Delete notification for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Delete(NotificationModel model)
    {
        _notificationService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }

    /// <summary>
    /// Execute the notification template and generate the results for previewing.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("{id}/preview/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public async Task<IActionResult> Preview(NotificationModel model, long contentId)
    {
        var content = _contentService.FindById(contentId) ?? throw new NoContentException();
        var notification = new Areas.Services.Models.Notification.NotificationModel(model.ToEntity(_serializerOptions, true), _serializerOptions);
        var contentModel = new TNO.TemplateEngine.Models.ContentModel(content);
        var result = await _notificationHelper.GenerateNotificationAsync(notification, contentModel, true);
        return new JsonResult(result);
    }

    /// <summary>
    /// Send the notification to the specified email address.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="contentId"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [HttpPost("{id}/send")]
    [HttpPost("{id}/send/{contentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public async Task<IActionResult> SendToAsync(int id, long? contentId, string to)
    {
        var notification = _notificationService.FindById(id) ?? throw new NoContentException();

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        var request = new NotificationRequestModel(NotificationDestination.NotificationService, new { })
        {
            NotificationId = notification.Id,
            ContentId = contentId,
            RequestorId = user.Id,
            To = to,
            IsPreview = true,
            IgnoreValidation = true,
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.NotificationTopic, request);
        return new JsonResult(new NotificationModel(notification, _serializerOptions));
    }
    #endregion
}
