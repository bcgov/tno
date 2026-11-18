using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ContentAnalysis;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ContentAnalysisController class, Content-Analysis administration: settings, the requests waiting
/// in Kafka, failed content and replay, and backfills (work orders the Event Handler runs).
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/analysis")]
[Route("api/[area]/analysis")]
[Route("v{version:apiVersion}/[area]/analysis")]
[Route("[area]/analysis")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public partial class ContentAnalysisController : ControllerBase
{
    #region Variables
    private readonly IContentAnalysisService _service;
    private readonly ISettingService _settingService;
    private readonly IAnalysisBackfillService _backfillService;
    private readonly IUserService _userService;
    private readonly IWorkOrderRequestSender _workOrderSender;
    private readonly IKafkaAdmin _kafkaAdmin;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<ContentAnalysisController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="settingService"></param>
    /// <param name="backfillService"></param>
    /// <param name="userService"></param>
    /// <param name="workOrderSender"></param>
    /// <param name="kafkaAdmin"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="logger"></param>
    public ContentAnalysisController(
        IContentAnalysisService service,
        ISettingService settingService,
        IAnalysisBackfillService backfillService,
        IUserService userService,
        IWorkOrderRequestSender workOrderSender,
        IKafkaAdmin kafkaAdmin,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<ContentAnalysisController> logger)
    {
        _service = service;
        _settingService = settingService;
        _backfillService = backfillService;
        _userService = userService;
        _workOrderSender = workOrderSender;
        _kafkaAdmin = kafkaAdmin;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// The Content-Analysis settings.
    /// </summary>
    /// <returns></returns>
    [HttpGet("settings")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentAnalysisSettingsModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult GetSettings()
    {
        return new JsonResult(ReadSettings());
    }

    /// <summary>
    /// Change the Content-Analysis settings. Which processes run is configured on the
    /// Content-Analysis service.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("settings")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentAnalysisSettingsModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult UpdateSettings([FromBody] ContentAnalysisSettingsModel model)
    {
        _settingService.SetValue(AdminConfigurableSettingNames.ContentAnalysisLLMId.ToString(), model.LLMId?.ToString() ?? "",
            "The LLM (llm table ID) Content-Analysis uses.");
        _settingService.SetValue(AdminConfigurableSettingNames.ContentAnalysisExcludedMediaTypeIds.ToString(), String.Join(",", model.ExcludedMediaTypeIds.Distinct()),
            "Comma-separated media type IDs Content-Analysis does not analyze.");
        _settingService.SetValue(AdminConfigurableSettingNames.ContentAnalysisExcludedSourceIds.ToString(), String.Join(",", model.ExcludedSourceIds.Distinct()),
            "Comma-separated source IDs Content-Analysis does not analyze.");
        return new JsonResult(ReadSettings());
    }

    /// <summary>
    /// The analysis requests waiting in each Kafka topic, and the number of content items whose
    /// newest request failed.
    /// </summary>
    /// <returns></returns>
    [HttpGet("queue")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisQueueModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public async Task<IActionResult> GetQueueAsync()
    {
        var topics = new[] { _kafkaOptions.AnalysisTopic, _kafkaOptions.AnalysisRetryTopic, _kafkaOptions.AnalysisBackfillTopic }
            .Where(t => !String.IsNullOrWhiteSpace(t))
            .ToArray();
        var lag = new Dictionary<string, long>();
        try
        {
            lag = new Dictionary<string, long>(await _kafkaAdmin.GetConsumerLagAsync(_kafkaOptions.AnalysisConsumerGroup, topics));
        }
        catch (Exception ex)
        {
            // The failed count is still useful when Kafka cannot be asked.
            _logger.LogWarning(ex, "Failed to read the Content-Analysis consumer lag");
        }
        return new JsonResult(new AnalysisQueueModel() { Lag = lag, Failed = _service.CountFailures() });
    }

    /// <summary>
    /// Content whose newest analysis request failed, most recent first.
    /// </summary>
    /// <param name="quantity"></param>
    /// <returns></returns>
    [HttpGet("failures")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AnalysisFailureModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult FindFailures(int quantity = 100)
    {
        return new JsonResult(_service.FindFailures(quantity).Select(f => new AnalysisFailureModel() { ContentId = f.ContentId, Headline = f.Headline, Run = f.Run }));
    }

    /// <summary>
    /// Send the content for analysis again, even when its analysis is current.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    [HttpPost("contents/{contentId:long}/replay")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisRequestModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Replay(long contentId)
    {
        // The request is sent to Kafka once the action completes.
        var request = _service.RequestAnalysis(contentId, AnalysisRequestReason.Replay);
        return new JsonResult(new AnalysisRequestModel(request.RequestId, request.ContentId, request.InputHash, request.Reason, request.Force, request.RequestedOn));
    }

    /// <summary>
    /// Count what a backfill would cover, including content left out because it has no publication date.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("backfills/preview")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillPreviewModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult PreviewBackfill([FromBody] AnalysisBackfillRequestModel model)
    {
        var preview = _backfillService.Preview(model.StartOn.ToUniversalTime(), model.EndOn.ToUniversalTime(), model.DateField, model.Mode);
        return new JsonResult(new AnalysisBackfillPreviewModel()
        {
            Matching = preview.Matching,
            Excluded = preview.Excluded,
            MissingPublicationDate = preview.MissingPublicationDate,
            AlreadyCurrent = preview.AlreadyCurrent,
        });
    }

    /// <summary>
    /// Start a backfill: a work order the Event Handler runs, sending each content item in the range
    /// to the analysis backfill topic.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("backfills")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public async Task<IActionResult> StartBackfillAsync([FromBody] AnalysisBackfillRequestModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var workOrder = _backfillService.Add(new AnalysisBackfillConfigurationModel()
        {
            StartOn = model.StartOn.ToUniversalTime(),
            EndOn = model.EndOn.ToUniversalTime(),
            TimeZone = model.TimeZone,
            DateField = model.DateField,
            Mode = model.Mode,
        }, user.Id);
        await _workOrderSender.SendAsync(workOrder, user.Username);
        return new JsonResult(ToModel(workOrder));
    }

    /// <summary>
    /// The most recent backfills with their progress.
    /// </summary>
    /// <returns></returns>
    [HttpGet("backfills")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AnalysisBackfillModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult FindBackfills()
    {
        return new JsonResult(_backfillService.FindRecent().Select(ToModel));
    }

    /// <summary>
    /// Stop a backfill.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("backfills/{id:long}/cancel")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult CancelBackfill(long id)
    {
        return new JsonResult(ToModel(_backfillService.Cancel(id)));
    }

    /// <summary>
    /// Continue a cancelled or failed backfill from its checkpoint.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("backfills/{id:long}/resume")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public async Task<IActionResult> ResumeBackfillAsync(long id)
    {
        var workOrder = _backfillService.Resume(id);
        await _workOrderSender.SendAsync(workOrder, User.GetUsername() ?? "");
        return new JsonResult(ToModel(workOrder));
    }
    #endregion

    #region Methods
    /// <summary>
    /// The backfill and its progress.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    private AnalysisBackfillModel ToModel(WorkOrder workOrder)
    {
        var configuration = AnalysisBackfillService.ReadConfiguration(workOrder);
        return new AnalysisBackfillModel()
        {
            Id = workOrder.Id,
            StartOn = configuration.StartOn,
            EndOn = configuration.EndOn,
            TimeZone = configuration.TimeZone,
            DateField = configuration.DateField,
            Mode = configuration.Mode,
            Status = workOrder.Status,
            Total = configuration.Total,
            Scheduled = configuration.Scheduled,
            AlreadyCurrent = configuration.AlreadyCurrent,
            Failed = _service.CountFailures(workOrder.Id),
            Error = configuration.Error,
            CreatedBy = workOrder.CreatedBy,
            CreatedOn = workOrder.CreatedOn,
            UpdatedOn = workOrder.UpdatedOn,
        };
    }

    /// <summary>
    /// The Content-Analysis settings.
    /// </summary>
    /// <returns></returns>
    private ContentAnalysisSettingsModel ReadSettings()
    {
        var settings = _service.GetSettings();
        return new ContentAnalysisSettingsModel()
        {
            LLMId = settings.LLMId,
            ExcludedMediaTypeIds = settings.ExcludedMediaTypeIds,
            ExcludedSourceIds = settings.ExcludedSourceIds,
        };
    }
    #endregion
}
