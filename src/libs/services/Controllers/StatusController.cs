using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TNO.Services.Models;

namespace TNO.Services.Controllers;

/// <summary>
/// StatusController class, provides health endpoints for the api.
/// </summary>
[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    #region Variables
    private readonly IServiceManager _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StatusController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public StatusController(IServiceManager service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Request service to pause.
    /// </summary>
    /// <returns></returns>
    [HttpPut("pause")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ServiceStateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public IActionResult PauseService()
    {
        if (_service.State.Status != ServiceStatus.Running)
            return new BadRequestResult();

        _service.State.Pause();
        return new JsonResult(_service.State);
    }

    /// <summary>
    /// Request service to resume.
    /// </summary>
    /// <returns></returns>
    [HttpPut("resume")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ServiceStateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public IActionResult ResumeService()
    {
        if (!new[] { ServiceStatus.Sleeping, ServiceStatus.Paused }.Contains(_service.State.Status))
            return new BadRequestResult();

        _service.State.Resume();
        return new JsonResult(_service.State);
    }
    #endregion
}
