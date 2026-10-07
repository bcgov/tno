using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ContentAnalysis;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.API.BackgroundWorkItem;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ContentAnalysisController class, Content-Analysis administration: settings, failed jobs and
/// replay, queue counts, and shadow comparison with Quote Extraction.
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
    private readonly IBackgroundTaskQueue _backgroundWorkerQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="settingService"></param>
    /// <param name="backfillService"></param>
    /// <param name="backgroundWorkerQueue"></param>
    /// <param name="serviceScopeFactory"></param>
    public ContentAnalysisController(
        IContentAnalysisService service,
        ISettingService settingService,
        IAnalysisBackfillService backfillService,
        IBackgroundTaskQueue backgroundWorkerQueue,
        IServiceScopeFactory serviceScopeFactory)
    {
        _service = service;
        _settingService = settingService;
        _backfillService = backfillService;
        _backgroundWorkerQueue = backgroundWorkerQueue;
        _serviceScopeFactory = serviceScopeFactory;
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
    /// Job counts by status and reason.
    /// </summary>
    /// <returns></returns>
    [HttpGet("queue")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisQueueModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult GetQueue()
    {
        return new JsonResult(new AnalysisQueueModel() { Counts = _service.GetQueueCounts() });
    }

    /// <summary>
    /// Jobs with the specified status (failed by default), most recent first.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    [HttpGet("jobs")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<AnalysisJobModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult FindJobs(AnalysisJobStatus status = AnalysisJobStatus.Failed, int quantity = 100)
    {
        return new JsonResult(_service.FindJobs(status, quantity).Select(j => new AnalysisJobModel(j)));
    }

    /// <summary>
    /// Queue a failed or skipped job again.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("jobs/{id:long}/replay")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisJobModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult Replay(long id)
    {
        return new JsonResult(new AnalysisJobModel(_service.Replay(id)));
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
    /// Start a backfill: persist its criteria, then schedule its jobs in the background.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("backfills")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult StartBackfill([FromBody] AnalysisBackfillRequestModel model)
    {
        var backfill = _backfillService.Add(new AnalysisBackfill(model.StartOn.ToUniversalTime(), model.EndOn.ToUniversalTime(), model.TimeZone, model.DateField, model.Mode));
        QueueBackfill(backfill.Id);
        return new JsonResult(ToModel(_backfillService.FindProgress(backfill.Id)!));
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
    /// Stop a backfill: no further scheduling, and its unclaimed jobs are withdrawn.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("backfills/{id:long}/cancel")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult CancelBackfill(long id)
    {
        _backfillService.Cancel(id);
        return new JsonResult(ToModel(_backfillService.FindProgress(id)!));
    }

    /// <summary>
    /// Continue a cancelled, failed, or interrupted backfill from its checkpoint.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("backfills/{id:long}/resume")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AnalysisBackfillModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentAnalysis" })]
    public IActionResult ResumeBackfill(long id)
    {
        _backfillService.Resume(id);
        QueueBackfill(id);
        return new JsonResult(ToModel(_backfillService.FindProgress(id)!));
    }
    #endregion

    #region Methods
    private void QueueBackfill(long id)
    {
        _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAnalysisBackfillService>();
            await service.RunAsync(id, token);
        });
    }

    private static AnalysisBackfillModel ToModel(AnalysisBackfillProgress progress)
    {
        var backfill = progress.Backfill;
        return new AnalysisBackfillModel()
        {
            Id = backfill.Id,
            StartOn = backfill.StartOn,
            EndOn = backfill.EndOn,
            TimeZone = backfill.TimeZone,
            DateField = backfill.DateField,
            Mode = backfill.Mode,
            Status = backfill.Status,
            Total = backfill.Total,
            Scheduled = backfill.Scheduled,
            AlreadyCurrent = backfill.AlreadyCurrent,
            Analyzed = progress.Analyzed,
            Superseded = progress.Superseded,
            Deleted = progress.Deleted,
            Failed = progress.Failed,
            Remaining = progress.Remaining,
            Indexed = progress.Indexed,
            IsComplete = progress.IsComplete,
            Error = backfill.Error,
            CreatedBy = backfill.CreatedBy,
            CreatedOn = backfill.CreatedOn,
            CompletedOn = backfill.CompletedOn,
        };
    }

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
