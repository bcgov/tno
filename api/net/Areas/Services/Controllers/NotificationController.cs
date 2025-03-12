using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Notification;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Models.Filters;

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
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var results = _service.Find(new NotificationFilter(query));
        return new JsonResult(results.Select(ds => new NotificationModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Return all notifications.
    /// </summary>
    /// <returns></returns>
    [HttpPost("find")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<NotificationModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult Find([FromBody] NotificationFilter filter)
    {
        var results = _service.Find(filter);
        return new JsonResult(results.Select(ds => new NotificationModel(ds, _serializerOptions)));
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

    /// <summary>
    /// Make a request to Elasticsearch for content that matches the specified report 'id' filter.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    [HttpGet("{id}/content")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public async Task<IActionResult> FindContentForNotificationIdAsync(int id, int? requestorId)
    {
        var notification = _service.FindById(id);
        if (notification == null) return NoContent();
        var results = await _service.FindContentWithElasticsearchAsync(notification, requestorId);
        return new JsonResult(results);
    }

    /// <summary>
    /// Get an array of any notification instances that CHES has not yet emailed.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="cutOff"></param>
    /// <returns></returns>
    [HttpGet("sent/{status}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ChesNotificationMessagesModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Notification" })]
    public IActionResult GetChesMessages(Entities.NotificationStatus status, [FromQuery] DateTime cutOff)
    {
        var messages = _service.GetChesMessageIds(status, cutOff);
        return new JsonResult(messages);
    }
    #endregion
}
