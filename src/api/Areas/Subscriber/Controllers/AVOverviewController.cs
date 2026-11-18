using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.AVOverview;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// AVOverviewController class, provides endpoints to manage evening overviews.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/av/overviews")]
[Route("api/[area]/reports/av/overviews")]
[Route("v{version:apiVersion}/[area]/reports/av/overviews")]
[Route("[area]/reports/av/overviews")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AVOverviewController : ControllerBase
{
    #region Variables
    private readonly IAVOverviewInstanceService _overviewInstanceService;
    private readonly IReportHelper _reportHelper;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewController object, initializes with specified parameters.
    /// </summary>
    /// <param name="overviewInstanceService"></param>
    /// <param name="reportHelper"></param>
    public AVOverviewController(
        IAVOverviewInstanceService overviewInstanceService,
        IReportHelper reportHelper)
    {
        _overviewInstanceService = overviewInstanceService;
        _reportHelper = reportHelper;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find evening overviews for the specified 'publishedOn' or latest if no date passed.
    /// </summary>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindByDate([FromQuery] DateTime? publishedOn = null)
    {
        Entities.AVOverviewInstance? instance;
        if (publishedOn != null)
        {
            instance = _overviewInstanceService.FindByDate((DateTime)publishedOn);
        }
        else
        {
            instance = _overviewInstanceService.FindLatest();
        }

        if (instance == null) return new NoContentResult();
        return new JsonResult(new AVOverviewInstanceModel(instance));
    }

    /// <summary>
    /// Execute the report template and generate the results for previewing.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpPost("{instanceId}/view")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TemplateEngine.Models.Reports.ReportResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> View(int instanceId)
    {
        var instance = _overviewInstanceService.FindById(instanceId) ?? throw new NoContentException($"AV overview instance '{instanceId}' not found");
        var model = new TemplateEngine.Models.Reports.AVOverviewInstanceModel(instance);
        var result = await _reportHelper.GenerateReportAsync(model, false);
        return new JsonResult(result);
    }
    #endregion
}
