using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models.Health;
using TNO.DAL;

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
    private readonly TNOContext _dbContext;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HealthController object, initializes with specified parameters.
    /// </summary>
    /// <param name="environment"></param>
    /// <param name="dbContext"></param>
    public HealthController(IWebHostEnvironment environment, TNOContext dbContext)
    {
        _environment = environment;
        _dbContext = dbContext;
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
    /// Readiness probe: proves the app can serve a real request end-to-end (thread pool +
    /// database round-trip) within a strict timeout. Unlike the status endpoint above (a static
    /// response that keeps succeeding while the app is wedged, e.g. in a GC death spiral at the
    /// container memory limit), this fails fast so the container healthcheck can flag the
    /// instance unhealthy and it can be restarted.
    ///
    /// A slow database is reported as "degraded" (200), NOT as a failure: the container
    /// healthcheck restarts the API when this endpoint fails, and restarting cannot fix a busy
    /// database - it only drops in-flight work. Only an outright connection failure is "not-ready".
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("ready")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(StatusModel), 200)]
    [ProducesResponseType(typeof(StatusModel), 503)]
    [SwaggerOperation(Tags = new[] { "health" })]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cts.Token);
            return new JsonResult(new StatusModel("ready"));
        }
        catch (Exception ex) when (ex is OperationCanceledException || cts.IsCancellationRequested)
        {
            // The query did not finish in time. The database is congested (or the pool is
            // saturated) - the API itself is still serving requests, so report degraded.
            return new JsonResult(new StatusModel("degraded"));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new StatusModel("not-ready"));
        }
    }

    /// <summary>
    /// Liveness probe: proves the process can accept a request AND dispatch work through the
    /// thread pool within a short budget. This is what the container healthcheck uses, because
    /// failing it means the app itself is wedged (GC death spiral, thread-pool starvation,
    /// deadlock) - the only condition a restart actually fixes. It deliberately touches no
    /// external dependency, so a slow database or Elasticsearch never triggers a restart.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("live")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(StatusModel), 200)]
    [ProducesResponseType(typeof(StatusModel), 503)]
    [SwaggerOperation(Tags = new[] { "health" })]
    public async Task<IActionResult> Live(CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            // Round-trip a work item through the thread pool: when the pool is starved this does
            // not complete, which is exactly the wedged state a restart resolves.
            await Task.Run(() => { }, cts.Token);
            return new JsonResult(new StatusModel("live"));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new StatusModel("not-live"));
        }
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
