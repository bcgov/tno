using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.History;
using TNO.API.Config;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// HistoryController class, provides history retention endpoints for the scheduled purge.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/history")]
[Route("api/[area]/history")]
[Route("v{version:apiVersion}/[area]/history")]
[Route("[area]/history")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class HistoryController : ControllerBase
{
    #region Variables
    private readonly IHistoryRetentionService _service;
    private readonly ApiOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a HistoryController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="options"></param>
    public HistoryController(IHistoryRetentionService service, IOptions<ApiOptions> options)
    {
        _service = service;
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Purge report history older than the 'ReportRetentionDays' setting.
    /// </summary>
    /// <param name="dryRun">Count what would be deleted without deleting it.</param>
    /// <returns></returns>
    [HttpPost("reports/purge")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(HistoryPurgeModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "History" })]
    public IActionResult PurgeReports(bool dryRun = false)
    {
        return new JsonResult(_service.PurgeReports(_service.GetReportRetentionDays(), dryRun));
    }

    /// <summary>
    /// Purge notification history older than the 'NotificationRetentionDays' setting.
    /// </summary>
    /// <param name="dryRun">Count what would be deleted without deleting it.</param>
    /// <returns></returns>
    [HttpPost("notifications/purge")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(HistoryPurgeModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "History" })]
    public IActionResult PurgeNotifications(bool dryRun = false)
    {
        return new JsonResult(_service.PurgeNotifications(_service.GetNotificationRetentionDays(), _options.NotificationPublishedBeforeOffset, dryRun));
    }
    #endregion
}
