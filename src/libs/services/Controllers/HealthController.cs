using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TNO.Services.Models;

namespace TNO.Services.Controllers;

/// <summary>
/// HealthController class, provides health endpoints for the api.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    #region Variables
    private readonly IServiceManager _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HealthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public HealthController(IServiceManager service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return service health.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ServiceStateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ServiceStateModel), (int)HttpStatusCode.ServiceUnavailable)]
    public IActionResult Health()
    {
        // Services opt into processing-aware health without changing intentional pause/sleep behaviour.
        var failure = (_service as IServiceHealth)?.HealthFailureReason;
        return new JsonResult(_service.State)
        {
            StatusCode = failure == null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.ServiceUnavailable,
        };
    }
    #endregion
}
