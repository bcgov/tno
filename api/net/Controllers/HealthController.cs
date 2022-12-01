using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models.Health;

namespace TNO.API.Controllers;

/// <summary>
/// HealthController class, provides health endpoints for the api.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    #region Variables
    private readonly IWebHostEnvironment _environment;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HealthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="environment"></param>
    public HealthController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return api status
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(StatusModel), 200)]
    [SwaggerOperation(Tags = new[] { "health" })]
    public IActionResult Status()
    {
        return new JsonResult(new StatusModel("running"));
    }

    /// <summary>
    /// Return environment information.
    /// </summary>
    /// <returns></returns>
    [HttpGet("env")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EnvModel), 200)]
    [SwaggerOperation(Tags = new[] { "health" })]
    public IActionResult Environment()
    {
        return new JsonResult(new EnvModel(_environment));
    }
    #endregion
}
