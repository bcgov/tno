using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Report;
using TNO.API.Config;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;
using TNO.TemplateEngine;
using TNO.Elastic.Models;
using TNO.DAL.Config;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ReportController class, provides Report endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports")]
[Route("api/[area]/reports")]
[Route("v{version:apiVersion}/[area]/reports")]
[Route("[area]/reports")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportController : ControllerBase
{
    #region Variables
    private readonly IReportService _reportService;
    private readonly IReportInstanceService _reportInstanceService;
    private readonly IUserService _userService;
    private readonly ITemplateEngine<TNO.TemplateEngine.Models.Reports.ReportTemplateModel> _templateEngine;
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ElasticOptions _elasticOptions;
    private readonly StorageOptions _storageOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportService"></param>
    /// <param name="reportInstanceService"></param>
    /// <param name="userService"></param>
    /// <param name="templateEngine"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportController(
        IReportService reportService,
        IReportInstanceService reportInstanceService,
        IUserService userService,
        ITemplateEngine<TNO.TemplateEngine.Models.Reports.ReportTemplateModel> templateEngine,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportService = reportService;
        _reportInstanceService = reportInstanceService;
        _userService = userService;
        _templateEngine = templateEngine;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _elasticOptions = elasticOptions.Value;
        _storageOptions = storageOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all reports.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_reportService.FindAll().Select(ds => new ReportModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find report for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindById(int id)
    {
        var result = _reportService.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ReportModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find all report instances for the specified report 'id' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    [HttpGet("{reportId}/instances")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindInstancesForReportId(int reportId, int? ownerId)
    {
        var result = _reportInstanceService.FindInstancesForReportId(reportId, ownerId);
        return new JsonResult(result.Select(ri => new ReportInstanceModel(ri, _serializerOptions)));
    }

    /// <summary>
    /// Add report for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Add(ReportModel model)
    {
        var result = _reportService.AddAndSave(model.ToEntity(_serializerOptions));
        var report = _reportService.FindById(result.Id) ?? throw new InvalidOperationException("Report does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Update report for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Update(ReportModel model)
    {
        var result = _reportService.UpdateAndSave(model.ToEntity(_serializerOptions));
        var report = _reportService.FindById(result.Id) ?? throw new InvalidOperationException("Report does not exist");
        return new JsonResult(new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Delete report for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Delete(ReportModel model)
    {
        _reportService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }

    /// <summary>
    /// Send the report to the specified email address.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [HttpPost("{id}/send")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> SendToAsync(int id, string to)
    {
        var report = _reportService.FindById(id);
        if (report == null) return new NoContentResult();

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        var request = new ReportRequestModel(ReportDestination.ReportingService, report.Id, new { })
        {
            RequestorId = user.Id,
            To = to,
            UpdateCache = true
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{report.Id}-test", request);
        return new JsonResult(new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Publish the report and send to all subscribers.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/publish")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> Publish(int id)
    {
        var report = _reportService.FindById(id);
        if (report == null) return new NoContentResult();

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        var request = new ReportRequestModel(ReportDestination.ReportingService, report.Id, new { })
        {
            RequestorId = user.Id
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{report.Id}", request);
        return new JsonResult(new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Preview report.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("preview")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportPreviewModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> Preview(ReportModel model)
    {
        if (model.Template == null) throw new InvalidOperationException("Report template is missing from model");
        var subjectTemplate = _templateEngine.AddOrUpdateTemplateInMemory($"report-{model.Id}-subject", model.Template?.Subject ?? throw new InvalidOperationException("Report subject template must be provided."));
        var bodyTemplate = _templateEngine.AddOrUpdateTemplateInMemory($"report-{model.Id}", model.Template?.Body ?? throw new InvalidOperationException("Report body template must be provided."));

        var results = await _reportService.FindContentWithElasticsearchAsync(_elasticOptions.PublishedIndex, model.ToEntity(_serializerOptions));

        // Link each result with the section name.
        var sections = model.Sections.ToDictionary(s => s.Name, s =>
        {
            results.TryGetValue(s.Name, out SearchResultModel<Services.Models.Content.ContentModel>? value);
            var content = value?.Hits.Hits.Select(h => new TNO.TemplateEngine.Models.Reports.ContentModel(h.Source)).ToArray()
                ?? Array.Empty<TNO.TemplateEngine.Models.Reports.ContentModel>();
            return new TNO.TemplateEngine.Models.Reports.ReportSectionModel(s, content);
        });

        var templateModel = new TNO.TemplateEngine.Models.Reports.ReportTemplateModel(sections, model.Settings, _storageOptions.GetUploadPath());

        var subject = await subjectTemplate.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Settings = templateModel.Settings;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
        });
        var body = await bodyTemplate.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Settings = templateModel.Settings;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
        });

        return new JsonResult(new ReportPreviewModel(subject, body, results));
    }
    #endregion
}
