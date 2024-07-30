using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.SystemMessage;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// SystemMessageController class, provides system message endpoints for the admin api.
/// </summary>
[ApiController]
[AllowAnonymous]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/system-message")]
[Route("api/[area]/system-messages")]
[Route("v{version:apiVersion}/[area]/system-messages")]
[Route("[area]/system-messages")]
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
    [SwaggerOperation(Tags = new[] { "System Message" })]
    public IActionResult FindAll()
    {
        var result = _service.FindAll();
        return new JsonResult(result.Select(m => new SystemMessageModel(m)));
    }
    #endregion
}
