using System.Net;
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
    private readonly IDataSourceManager _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HealthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public HealthController(IDataSourceManager service)
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ServiceStateModel), (int)HttpStatusCode.OK)]
    public IActionResult Health()
    {
        return new JsonResult(_service.State);
    }
    #endregion
}
