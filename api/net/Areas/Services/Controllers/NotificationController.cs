using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Notification;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// NotificationController class, provides Notification endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
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
    private readonly INotificationService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    public NotificationController(INotificationService service, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return all notifications.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<NotificationModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new NotificationModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new NotificationModel(result, _serializerOptions));
    }
    #endregion
}
