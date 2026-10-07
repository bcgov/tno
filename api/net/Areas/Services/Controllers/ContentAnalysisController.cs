using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using SignalRModels = TNO.API.Models.SignalR;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContentAnalysisController class, the Content-Analysis workers' API: claim jobs, renew leases,
/// read input, submit results, and record failures. Workers never touch the database directly.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/analysis")]
[Route("api/[area]/analysis")]
[Route("v{version:apiVersion}/[area]/analysis")]
[Route("[area]/analysis")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentAnalysisController : ControllerBase
{
    #region Variables
    private readonly IContentAnalysisService _service;
    private readonly IContentService _contentService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly ILogger<ContentAnalysisController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="contentService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="logger"></param>
    public ContentAnalysisController(
        IContentAnalysisService service,
        IContentService contentService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        ILogger<ContentAnalysisController> logger)
    {
        _service = service;
        _contentService = contentService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// The runtime settings a worker needs: the mode and the LLM.
    /// </summary>
    /// <returns></returns>
    [HttpGet("settings")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentAnalysisSettingsModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult GetSettings()
    {
        var settings = _service.GetSettings();
        return new JsonResult(new ContentAnalysisSettingsModel()
        {
            LLMId = settings.LLMId,
            ExcludedMediaTypeIds = settings.ExcludedMediaTypeIds,
            ExcludedSourceIds = settings.ExcludedSourceIds,
        });
    }

    /// <summary>
    /// Claim due jobs. Returns none while Content-Analysis is off.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("jobs/claim")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AnalysisJobModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Claim([FromBody] AnalysisClaimRequestModel model)
    {
        return new JsonResult(_service.ClaimJobs(model).Select(j => new AnalysisJobModel(j)));
    }

    /// <summary>
    /// Extend a claim's lease.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns>The job, or no content when the claim is no longer valid.</returns>
    [HttpPut("jobs/{id:long}/lease")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisJobModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult RenewLease(long id, [FromBody] AnalysisLeaseModel model)
    {
        model.JobId = id;
        var job = _service.RenewLease(model);
        return job == null ? NoContent() : new JsonResult(new AnalysisJobModel(job));
    }

    /// <summary>
    /// The current input of claimed content.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fencingToken"></param>
    /// <returns>The input, or no content when the claim is no longer valid.</returns>
    [HttpGet("jobs/{id:long}/input")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisInputModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult GetInput(long id, long fencingToken)
    {
        var input = _service.GetInput(new AnalysisLeaseModel() { JobId = id, FencingToken = fencingToken });
        return input == null ? NoContent() : new JsonResult(input);
    }

    /// <summary>
    /// Submit an analysis. Accepted results populate empty editorial fields (in populate mode) and
    /// re-index the content; editors with the story open are told to merge the change.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("jobs/{id:long}/result")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisSubmitResultModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public async Task<IActionResult> SubmitAsync(long id, [FromBody] AnalysisResultModel model)
    {
        model.JobId = id;
        var result = _service.Submit(model);
        if (result.Status == ContentAnalysisService.Accepted && result.ContentChanged)
        {
            // Best effort, after the commit; the form merges populated fields and the new version.
            var job = _service.FindJob(model.JobId);
            var content = job != null ? _contentService.FindById(job.ContentId) : null;
            if (content != null)
            {
                try
                {
                    await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll,
                        new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new SignalRModels.ContentMessageModel(content, "analysis") })));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to notify editors of analysis for content {contentId}", content.Id);
                }
            }
        }
        return new JsonResult(result);
    }

    /// <summary>
    /// Record a failed attempt.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns>The job, or no content when the claim is no longer valid.</returns>
    [HttpPost("jobs/{id:long}/failure")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisJobModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Fail(long id, [FromBody] AnalysisFailureModel model)
    {
        model.JobId = id;
        var job = _service.Fail(model);
        return job == null ? NoContent() : new JsonResult(new AnalysisJobModel(job));
    }
    #endregion
}
