using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using SignalRModels = TNO.API.Models.SignalR;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContentAnalysisController class, the Content-Analysis and Event Handler services' API: read a
/// content item's analysis input, submit results, record the outcome of requests, and page through
/// backfills. The work arrives through Kafka; services never touch the database directly.
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
    private readonly IAnalysisBackfillService _backfillService;
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
    /// <param name="backfillService"></param>
    /// <param name="contentService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="logger"></param>
    public ContentAnalysisController(
        IContentAnalysisService service,
        IAnalysisBackfillService backfillService,
        IContentService contentService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        ILogger<ContentAnalysisController> logger)
    {
        _service = service;
        _backfillService = backfillService;
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
    /// The content's current analysis input.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpGet("contents/{contentId:long}/input")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisInputModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult GetInput(long contentId)
    {
        var input = _service.GetInput(contentId);
        return input == null ? NoContent() : new JsonResult(input);
    }

    /// <summary>
    /// Submit an analysis. Accepted results populate empty editorial fields and re-index the content;
    /// editors with the story open are told to merge the change. The request's run is recorded.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("contents/{contentId:long}/result")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisSubmitResultModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public async Task<IActionResult> SubmitAsync(long contentId, [FromBody] AnalysisResultModel model)
    {
        model.ContentId = contentId;
        var result = _service.Submit(model);
        if (result.Status == ContentAnalysisService.Accepted && result.ContentChanged)
        {
            // Best effort, after the commit; the form merges populated fields and the new version.
            var content = _contentService.FindById(contentId);
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
    /// Record the outcome of an analysis request that produced no result (skipped, retrying, or
    /// failed) in the content's metadata.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("contents/{contentId:long}/runs")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisMetadata), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult RecordRun(long contentId, [FromBody] AnalysisRunRequestModel model)
    {
        var metadata = _service.RecordRun(contentId, model.ToRun(model.Status, model.Error));
        return metadata == null ? NoContent() : new JsonResult(metadata);
    }

    /// <summary>
    /// The next page of a backfill after the specified content ID: the content to send for analysis.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <param name="after"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    [HttpGet("backfills/{workOrderId:long}/page")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillPageModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult FindBackfillPage(long workOrderId, long after = 0, int quantity = 500)
    {
        var workOrder = _backfillService.FindById(workOrderId);
        if (workOrder == null) return NoContent();
        return new JsonResult(_backfillService.FindPage(AnalysisBackfillService.ReadConfiguration(workOrder), after, quantity));
    }
    #endregion
}
