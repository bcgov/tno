using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.SystemMessage;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// SystemMessageController class, provides system message endpoints for the admin api.
/// </summary>
[ApiController]
[ClientRoleAuthorize(ClientRole.Administrator)]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/system-message")]
[Route("api/[area]/system-message")]
[Route("v{version:apiVersion}/[area]/system-message")]
[Route("[area]/system-message")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class SystemMessageController : ControllerBase
{
    #region Variables
    private readonly ISystemMessageService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SystemMessageController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public SystemMessageController(ISystemMessageService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of system message for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<SystemMessageModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "System Message" })]
    public IActionResult FindSystemMessage()
    {
        var result = _service.FindSystemMessage() ?? throw new NoContentException();
        return new JsonResult(new SystemMessageModel(result));
    }

    /// <summary>
    /// Add system message for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SystemMessageModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "System Message" })]
    public IActionResult Add(SystemMessageModel model)
    {
        var result = _service.AddAndSave((SystemMessage)model);
        return CreatedAtAction(nameof(FindSystemMessage), new { id = result.Id }, new SystemMessageModel(result));
    }

    /// <summary>
    /// Update system message for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SystemMessageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "System Message" })]
    public IActionResult Update(SystemMessageModel model)
    {
        var result = _service.UpdateAndSave((SystemMessage)model);
        return new JsonResult(new SystemMessageModel(result));
    }

    /// <summary>
    /// Delete system message for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SystemMessageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "System Message" })]
    public IActionResult Delete(SystemMessageModel model)
    {
        _service.DeleteAndSave((SystemMessage)model);
        return new JsonResult(model);
    }
    #endregion
}
