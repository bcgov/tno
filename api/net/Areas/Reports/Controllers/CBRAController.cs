using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Reports;

namespace TNO.API.Areas.Reports.Controllers;

/// <summary>
/// CBRAController class, provides CBRA endpoints for the api.
/// </summary>
[Authorize]
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
    private readonly IContentService _contentService;
    private readonly ITimeTrackingService _timeTrackingService;
    private readonly IActionService _actionService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CBRAController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="timeTrackingService"></param>
    /// <param name="actionService"></param>
    public CBRAController(IContentService contentService, ITimeTrackingService timeTrackingService, IActionService actionService)
    {
        _contentService = contentService;
        _timeTrackingService = timeTrackingService;
        _actionService = actionService;
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
        var helper = new CBRAReport(_contentService, _timeTrackingService, _actionService);

        var report = helper.GenerateReport(from, to);

        using var stream = new MemoryStream();
        report.Write(stream);
        var bytes = stream.ToArray();

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cbra.xlsx");
    }
    #endregion
}
