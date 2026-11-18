using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Reports;

namespace TNO.API.Areas.Reports.Controllers;

/// <summary>
/// CBRAController class, provides CBRA endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("reports")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/cbra")]
[Route("api/[area]/cbra")]
[Route("v{version:apiVersion}/[area]/cbra")]
[Route("[area]/cbra")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class CBRAController : ControllerBase
{
    #region Variables
    private readonly ITimeTrackingService _timeTrackingService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CBRAController object, initializes with specified parameters.
    /// </summary>
    /// <param name="timeTrackingService"></param>
    public CBRAController(ITimeTrackingService timeTrackingService)
    {
        _timeTrackingService = timeTrackingService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Generate an Excel document CBRA report.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "CBRA" })]
    public FileResult Generate([FromQuery] DateTime from, [FromQuery] DateTime? to)
    {
        var helper = new CBRAReport(_timeTrackingService);

        var report = helper.GenerateReport(from, to);

        using var stream = new MemoryStream();
        report.Write(stream);
        var bytes = stream.ToArray();

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cbra.xlsx");
    }
    #endregion
}
