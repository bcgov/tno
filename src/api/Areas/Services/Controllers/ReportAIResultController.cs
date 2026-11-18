using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ReportAIResultController class, stores generated AI section results for the reporting service.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/ai-results")]
[Route("api/[area]/reports/ai-results")]
[Route("v{version:apiVersion}/[area]/reports/ai-results")]
[Route("[area]/reports/ai-results")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportAIResultController : ControllerBase
{
    #region Variables
    private readonly IReportAIResultService _service;
    private readonly IReportEvidenceService _evidenceService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIResultController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="evidenceService"></param>
    public ReportAIResultController(IReportAIResultService service, IReportEvidenceService evidenceService)
    {
        _evidenceService = evidenceService;
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find the result for the specified manifest hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    [HttpGet("{hash}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAIResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ReportAIResult" })]
    public IActionResult FindByHash(string hash)
    {
        var result = _service.FindByHash(hash);
        return result == null ? NoContent() : new JsonResult(new ReportAIResultModel(result));
    }

    /// <summary>
    /// Claim the right to generate a result.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>The claimed result, or no content when another generator holds it or it is complete.</returns>
    [HttpPost("claim")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAIResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ReportAIResult" })]
    public IActionResult Claim([FromBody] ReportAIResultClaimModel model)
    {
        var result = _service.TryClaim(model, User.GetUsername() ?? "service");
        return result == null ? NoContent() : new JsonResult(new ReportAIResultModel(result));
    }

    /// <summary>
    /// Record the outcome of a claimed result.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAIResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportAIResult" })]
    public IActionResult Complete(long id, [FromBody] ReportAIResultCompletionModel model)
    {
        var result = _service.Complete(id, model) ?? throw new NoContentException("AI result does not exist");
        return new JsonResult(new ReportAIResultModel(result));
    }

    /// <summary>
    /// The approved analysis evidence for the specified content, for report synthesis.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("evidence")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ContentEvidenceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ReportAIResult" })]
    public async Task<IActionResult> FindEvidenceAsync([FromBody] IEnumerable<long> contentIds, CancellationToken cancellationToken)
    {
        var evidence = await _evidenceService.FindAsync(contentIds, cancellationToken);
        return new JsonResult(evidence.Values);
    }
    #endregion
}
