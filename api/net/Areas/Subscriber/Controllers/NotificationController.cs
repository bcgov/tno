using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.API.Areas.Subscriber.Models;
using TNO.Keycloak;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Kafka;
using TNO.API.Config;
using TNO.Kafka.Models;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// NotificationController class, provides Notification endpoints for the subscriber api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationController object, initializes with specified parameters.
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="notificationService"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    public NotificationController(IUserService userService, INotificationService notificationService, IKafkaMessenger kafkaProducer, IOptions<KafkaOptions> kafkaOptions, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _userService = userService;
        _notificationService = notificationService;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
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
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        var request = new NotificationRequestModel(NotificationDestination.NotificationService, new { })
        {
            NotificationId = notification.Id,
            ContentId = contentId,
            RequestorId = user.Id,
            To = to,
            IsPreview = true,
            IgnoreValidation = true,
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.NotificationTopic, $"notification-{notification.Id}-test", request);
        return new JsonResult(new NotificationModel(notification, _serializerOptions));
    }
    #endregion
}
