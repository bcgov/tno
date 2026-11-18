using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ContentAnalysisController class, read-only analysis for subscribers.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/contents/{contentId:long}/analysis")]
[Route("api/[area]/contents/{contentId:long}/analysis")]
[Route("v{version:apiVersion}/[area]/contents/{contentId:long}/analysis")]
[Route("[area]/contents/{contentId:long}/analysis")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentAnalysisController : ControllerBase
{
    #region Variables
    private readonly IContentAnalysisService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ContentAnalysisController(IContentAnalysisService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// The content's current analysis for subscribers.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Find(long contentId)
    {
        var analysis = _service.FindCurrent(contentId);
        if (analysis == null) return new JsonResult(null);
        return new JsonResult(new
        {
            analysis.ContentId,
            analysis.IsMetadataOnly,
            analysis.Model,
            analysis.Summary,
            KeyFacts = analysis.KeyFacts.RootElement,
            Entities = analysis.Entities.RootElement,
            Places = analysis.Places.RootElement,
            Topics = analysis.Topics.RootElement,
            StaffTopic = analysis.AnalysisTopic?.Topic?.Name,
            Events = analysis.Events.RootElement,
            analysis.AnalyzedOn,
        });
    }

    #endregion
}
