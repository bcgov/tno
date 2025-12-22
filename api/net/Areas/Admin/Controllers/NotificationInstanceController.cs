using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.NotificationInstance;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// NotificationInstanceController class, provides NotificationInstance endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/notification/instances")]
[Route("api/[area]/notification/instances")]
[Route("v{version:apiVersion}/[area]/notification/instances")]
[Route("[area]/notification/instances")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class NotificationInstanceController : ControllerBase
{
    #region Variables
    private readonly INotificationInstanceService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly Helpers.WatchSubscriptionChange _watch;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationInstanceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="watch"></param>
    /// <param name="serializerOptions"></param>
    public NotificationInstanceController(
        INotificationInstanceService service,
        Helpers.WatchSubscriptionChange watch,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _watch = watch;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "NotificationInstance" })]
    public IActionResult FindById(long id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new NotificationInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationInstanceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "NotificationInstance" })]
    public IActionResult Add([FromBody] NotificationInstanceModel model)
    {
        var result = _service.AddAndSave(model.ToEntity(_serializerOptions));
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new NotificationInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "NotificationInstance" })]
    public async Task<IActionResult> UpdateAsync([FromBody] NotificationInstanceModel model)
    {
        var notificationInstance = model.ToEntity(_serializerOptions);
        // TODO: This shouldn't occur, but if it does we want to know.
        if (notificationInstance.Notification != null)
            await _watch.AlertNotificationSubscriptionChangedAsync(notificationInstance.Notification, this.User, "API Admin Notification Instance Controller Update endpoint.");
        var result = _service.UpdateAndSave(notificationInstance);
        return new JsonResult(new NotificationInstanceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(NotificationInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "NotificationInstance" })]
    public IActionResult Delete([FromBody] NotificationInstanceModel model)
    {
        _service.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
