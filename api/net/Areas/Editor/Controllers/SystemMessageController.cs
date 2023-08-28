using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.SystemMessage;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// SystemMessageController class, provides system message endpoints for the admin api.
/// </summary>
[ApiController]
[Area("editor")]
[AllowAnonymous]
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
    #endregion
}
