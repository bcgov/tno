using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.NotificationTemplate;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// NotificationController class, provides Notification endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/notification/templates")]
[Route("api/[area]/notification/templates")]
[Route("v{version:apiVersion}/[area]/notification/templates")]
[Route("[area]/notification/templates")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class NotificationTemplateController : ControllerBase
{
    #region Variables
    private readonly INotificationTemplateService _notificationTemplateService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationTemplateController object, initializes with specified parameters.
    /// </summary>
    /// <param name="notificationTemplateService"></param>
    /// <param name="serializerOptions"></param>
    public NotificationTemplateController(
        INotificationTemplateService notificationTemplateService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _notificationTemplateService = notificationTemplateService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all notification templates.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<NotificationTemplateModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_notificationTemplateService.FindAll().Select(ds => new NotificationTemplateModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find notification template for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult FindById(int id)
    {
        var result = _notificationTemplateService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new NotificationTemplateModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add notification template for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationTemplateModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Add([FromBody] NotificationTemplateModel model)
    {
        var result = _notificationTemplateService.AddAndSave((NotificationTemplate)model);
        var notification = _notificationTemplateService.FindById(result.Id) ?? throw new NoContentException("Notification template does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new NotificationTemplateModel(notification, _serializerOptions));
    }

    /// <summary>
    /// Update notification template for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Update([FromBody] NotificationTemplateModel model)
    {
        var result = _notificationTemplateService.UpdateAndSave((NotificationTemplate)model);
        var notification = _notificationTemplateService.FindById(result.Id) ?? throw new NoContentException("Notification template does not exist");
        return new JsonResult(new NotificationTemplateModel(notification, _serializerOptions));
    }

    /// <summary>
    /// Delete notification template for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Delete([FromBody] NotificationTemplateModel model)
    {
        // Do not allow deleting a notification template that is used by a notification.
        if (_notificationTemplateService.IsInUse(model.Id)) throw new InvalidOperationException("Cannot delete a template in use by a notification.");
        _notificationTemplateService.DeleteAndSave((NotificationTemplate)model);
        return new JsonResult(model);
    }
    #endregion
}
