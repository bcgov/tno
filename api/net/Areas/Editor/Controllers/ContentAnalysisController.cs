using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.ContentAnalysis;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// ContentAnalysisController class, a content item's analysis and field ownership for editors.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
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
    /// The content's current analysis, field ownership, and queued work.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentAnalysisDetailsModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Find(long contentId)
    {
        var analysis = _service.FindCurrent(contentId);
        var job = _service.FindJob(contentId);
        return new JsonResult(new ContentAnalysisDetailsModel()
        {
            Analysis = analysis != null ? new ContentAnalysisModel(analysis) : null,
            Ownership = _service.FindOwnership(contentId).Select(o => new ContentFieldOwnershipModel(o)),
            Job = job != null ? new AnalysisJobModel(job) : null,
        });
    }

    /// <summary>
    /// Queue the content for analysis again, even when its analysis is current.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisJobModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Reanalyze(long contentId)
    {
        return new JsonResult(new AnalysisJobModel(_service.RequestReanalysis(contentId)));
    }
    #endregion
}
