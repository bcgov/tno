using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.ReportInstance;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;
using TNO.Reports;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ReportController class, provides Report endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/report/instances")]
[Route("api/[area]/report/instances")]
[Route("v{version:apiVersion}/[area]/report/instances")]
[Route("[area]/report/instances")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportInstanceController : ControllerBase
{
    #region Variables
    private readonly IReportService _reportService;
    private readonly IReportInstanceService _reportInstanceService;
    private readonly IUserService _userService;
    private readonly IReportHelper _reportHelper;
    private readonly IImpersonationHelper _impersonate;
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportService"></param>
    /// <param name="reportInstanceService"></param>
    /// <param name="userService"></param>
    /// <param name="impersonateHelper"></param>
    /// <param name="reportHelper"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportInstanceController(
        IReportService reportService,
        IReportInstanceService reportInstanceService,
        IUserService userService,
        IImpersonationHelper impersonateHelper,
        IReportHelper reportHelper,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportService = reportService;
        _reportInstanceService = reportInstanceService;
        _userService = userService;
        _impersonate = impersonateHelper;
        _reportHelper = reportHelper;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find report instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeContent"></param>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult FindById(long id, bool includeContent = false, [FromQuery] DateTime? publishedOn = null)
    {
        var user = _impersonate.GetCurrentUser();
        var instance = publishedOn == null ? _reportInstanceService.FindById(id) : _reportInstanceService.FindInstanceForReportIdAndDate(id, (DateTime)publishedOn);
        if (instance == null) return new JsonResult(null);
        var report = instance.Report ?? throw new NoContentException("Report does not exist");
        if (instance.OwnerId != user.Id && // User does not own the report instance
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to use this report"); // Report is not public

        var model = new ReportInstanceModel(instance);
        if (includeContent)
            model.Content = _reportInstanceService.GetContentForInstance(id).Select(c => new ReportInstanceContentModel(c));
        return new JsonResult(model);
    }

    /// <summary>
    /// Add report instance for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult Add([FromBody] ReportInstanceModel model)
    {
        var user = _impersonate.GetCurrentUser();
        var report = _reportService.FindById(model.ReportId) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report instance
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to use this report"); // Report is not public
        var result = _reportInstanceService.AddAndSave((Entities.ReportInstance)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportInstanceModel(result));
    }

    /// <summary>
    /// Update report instance for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult Update([FromBody] ReportInstanceModel model)
    {
        var user = _impersonate.GetCurrentUser();
        var instance = _reportInstanceService.FindByKey(model.Id) ?? throw new NoContentException("Report does not exist");
        if (instance.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to update this report"); // Report is not public
        _reportInstanceService.ClearChangeTracker();
        _reportInstanceService.UpdateAndSave((Entities.ReportInstance)model, true);
        return FindById(model.Id);
    }

    /// <summary>
    /// Delete report instance for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ReportInstance" })]
    public IActionResult Delete([FromBody] ReportInstanceModel model)
    {
        var user = _impersonate.GetCurrentUser();
        var instance = _reportInstanceService.FindByKey(model.Id) ?? throw new NoContentException("Report does not exist");
        if (instance.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to delete this report"); // Report is not public
        _reportInstanceService.DeleteAndSave(instance);
        return new JsonResult(model);
    }

    /// <summary>
    /// Execute the report template and generate the results for viewing the specified report instance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="regenerate"></param>
    /// <returns></returns>
    [HttpPost("{id}/view")]
    [AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> ViewAsync(int id, bool regenerate = false)
    {
        var instance = _reportInstanceService.FindById(id) ?? throw new NoContentException("Report does not exist");

        if (regenerate || String.IsNullOrWhiteSpace(instance.Body))
        {
            var user = _impersonate.GetCurrentUser();
            var report = instance.Report ?? throw new NoContentException("Report does not exist");
            if (instance.OwnerId != user.Id && // User does not own the report instance
                !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
                !report.IsPublic) throw new NotAuthorizedException("Not authorized to preview this report"); // Report is not public
            instance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(id));
            var result = await _reportHelper.GenerateReportAsync(new Services.Models.ReportInstance.ReportInstanceModel(instance, _serializerOptions), false, true);
            return new JsonResult(result);
        }

        return new JsonResult(new ReportResultModel(instance));
    }

    /// <summary>
    /// Send the report to the specified email address.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [HttpPost("{id}/send")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> SendToAsync(int id, string to)
    {
        var user = _impersonate.GetCurrentUser();
        var instance = _reportInstanceService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (instance.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to send this report"); // User does not own the report

        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, instance.ReportId, instance.Id, JsonDocument.Parse("{}"))
        {
            RequestorId = user.Id,
            To = to,
            SendToSubscribers = false,
            GenerateInstance = false,
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{instance.ReportId}", request);
        return new JsonResult(new ReportInstanceModel(instance));
    }

    /// <summary>
    /// Publish the report and send to all subscribers.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="resend"></param>
    /// <returns></returns>
    [HttpPost("{id}/publish")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> PublishAsync(int id, bool resend)
    {
        var user = _impersonate.GetCurrentUser();
        var instance = _reportInstanceService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (instance.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to publish this report"); // User does not own the report

        instance.Status = new[] { Entities.ReportStatus.Pending, Entities.ReportStatus.Reopen }.Contains(instance.Status) ? Entities.ReportStatus.Submitted : instance.Status;
        instance = _reportInstanceService.UpdateAndSave(instance);

        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, instance.ReportId, instance.Id, JsonDocument.Parse("{}"))
        {
            RequestorId = user.Id,
            Resend = resend || instance.Status == Entities.ReportStatus.Reopen,
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{instance.ReportId}", request);
        return new JsonResult(new ReportInstanceModel(instance));
    }

    /// <summary>
    /// Generate an Excel document report.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/export")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public FileResult GenerateExcel(int id)
    {
        var user = _impersonate.GetCurrentUser();
        var instance = _reportInstanceService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (instance.OwnerId != user.Id) throw new NotAuthorizedException("User does not have permission to export this report");
        var content = _reportInstanceService.GetContentForInstance(id);
        instance.ContentManyToMany.AddRange(content);

        var helper = new ReportXlsExport("Report", _serializerOptions);
        var report = helper.GenerateExcel(instance);

        using var stream = new MemoryStream();
        report.Write(stream);
        var bytes = stream.ToArray();

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
    #endregion
}
