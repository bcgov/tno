using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.API.Areas.Subscriber.Models.Report;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.API.Models.Settings;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ReportAIController class, lets the owner of a report see, try and discuss the prompt of one of
/// its AI sections: the requests the section sends to its model, a test run with draft settings,
/// and a conversation with the model about how to change the prompt. Test runs are kept in memory
/// only (see <see cref="ReportAISectionTestCache"/>).
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/{id}/sections/{sectionId}/ai")]
[Route("api/[area]/reports/{id}/sections/{sectionId}/ai")]
[Route("v{version:apiVersion}/[area]/reports/{id}/sections/{sectionId}/ai")]
[Route("[area]/reports/{id}/sections/{sectionId}/ai")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportAIController : ControllerBase
{
    #region Variables
    private readonly IReportService _reportService;
    private readonly IReportInstanceService _reportInstanceService;
    private readonly IReportHelper _reportHelper;
    private readonly IImpersonationHelper _impersonate;
    private readonly IReportAISectionTestCache _cache;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportService"></param>
    /// <param name="reportInstanceService"></param>
    /// <param name="reportHelper"></param>
    /// <param name="impersonateHelper"></param>
    /// <param name="cache"></param>
    /// <param name="serializerOptions"></param>
    public ReportAIController(
        IReportService reportService,
        IReportInstanceService reportInstanceService,
        IReportHelper reportHelper,
        IImpersonationHelper impersonateHelper,
        IReportAISectionTestCache cache,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportService = reportService;
        _reportInstanceService = reportInstanceService;
        _reportHelper = reportHelper;
        _impersonate = impersonateHelper;
        _cache = cache;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Describe the requests the AI section would send for the report's current content with the
    /// specified (possibly unsaved) settings, without sending any.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("prompt")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAISectionPromptModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> DescribeAsync(int id, int sectionId, [FromBody] ReportSectionSettingsModel settings, CancellationToken cancellationToken)
    {
        var (_, instance) = LoadReportAndInstance(id, sectionId);
        var result = await _reportHelper.DescribeReportAISectionAsync(instance, sectionId, settings, cancellationToken);
        return new JsonResult(result);
    }

    /// <summary>
    /// Run the AI section once with the specified (possibly unsaved) settings against the report's
    /// current content. The run happens in the reporting service; the outcome is announced on the
    /// report status channel as "ai-test:{testId}" and fetched with the tests endpoint.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <param name="settings"></param>
    /// <returns>The pending run.</returns>
    [HttpPost("test")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAISectionTestResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> TestAsync(int id, int sectionId, [FromBody] ReportSectionSettingsModel settings)
    {
        var user = _impersonate.GetCurrentUser();
        var (report, instance) = LoadReportAndInstance(id, sectionId);
        if (_cache.FindPending(user.Id, sectionId) != null)
            throw new BadRequestException("A test of this section is still running. Wait for it to finish before starting another.");

        var result = _cache.Start(report.Id, instance.Id, sectionId, user.Id);
        await _reportHelper.RequestReportAISectionTestAsync(report.Id, instance.Id, result.TestId, sectionId, settings, user.Id);
        return new JsonResult(result);
    }

    /// <summary>
    /// Fetch a test run requested by the current user.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <param name="testId"></param>
    /// <returns>The run, or no content when it is unknown or has expired.</returns>
    [HttpGet("tests/{testId:guid}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAISectionTestResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindTest(int id, int sectionId, Guid testId)
    {
        var user = _impersonate.GetCurrentUser();
        var result = _cache.Find(testId);
        if (result == null || result.ReportId != id || result.SectionId != sectionId) return NoContent();
        if (result.RequestorId != user.Id) throw new NotAuthorizedException("Not authorized to view this test run");
        return new JsonResult(result);
    }

    /// <summary>
    /// Ask the section's model how to change the section's prompt. The first turn has no messages
    /// and composes the context from the settings and the test run; later turns pass the returned
    /// conversation back.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("chat")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportAIPromptChatResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> ChatAsync(int id, int sectionId, [FromBody] ReportAIPromptChatRequestModel request, CancellationToken cancellationToken)
    {
        var user = _impersonate.GetCurrentUser();
        LoadReport(id, sectionId, user.Id);
        ReportAISectionTestResultModel? test = null;
        if (request.TestId.HasValue)
        {
            test = _cache.Find(request.TestId.Value);
            if (test != null && (test.RequestorId != user.Id || test.SectionId != sectionId)) test = null;
        }
        var result = await _reportHelper.ChatAboutReportAISectionAsync(request.Settings, request, test, cancellationToken);
        return new JsonResult(result);
    }
    #endregion

    #region Helpers
    /// <summary>
    /// The report, which the current user must own, with the specified AI section.
    /// </summary>
    private Entities.Report LoadReport(int id, int sectionId, int userId)
    {
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != userId) throw new NotAuthorizedException("Not authorized to modify this report");
        if (!report.Sections.Any(s => s.Id == sectionId && s.SectionType == Entities.ReportSectionType.AI))
            throw new NoContentException("AI section does not exist");
        return report;
    }

    /// <summary>
    /// The report and its current instance with content, as the preview loads them.
    /// </summary>
    /// <exception cref="BadRequestException">The report has no instance to read content from.</exception>
    private (Entities.Report Report, Services.Models.ReportInstance.ReportInstanceModel Instance) LoadReportAndInstance(int id, int sectionId)
    {
        var user = _impersonate.GetCurrentUser();
        var report = LoadReport(id, sectionId, user.Id);
        var current = _reportService.GetLatestInstances(id, user.Id).FirstOrDefault()
            ?? throw new BadRequestException("Generate the report before viewing or testing its AI section prompts.");
        var instance = _reportInstanceService.FindById(current.Id) ?? throw new NoContentException("Report instance does not exist");
        instance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(instance.Id));
        return (report, new Services.Models.ReportInstance.ReportInstanceModel(instance, _serializerOptions));
    }
    #endregion
}
