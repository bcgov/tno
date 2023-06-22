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
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;
using TNO.Models.Extensions;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Models.Reports;
using TNO.TemplateEngine.Extensions;
using TNO.Elastic.Models;
using TNO.API.Areas.Services.Models.Content;
using TNO.DAL.Config;
using System.Web;

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
    private readonly IUserService _userService;
    private readonly ITemplateEngine<TemplateModel> _templateEngine;
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
    /// <param name="userService"></param>
    /// <param name="templateEngine"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportController(
        IReportService reportService,
        IUserService userService,
        ITemplateEngine<TemplateModel> templateEngine,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportService = reportService;
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
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<ReportModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_reportService.FindAll().Select(ds => new ReportModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeInstances"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindById(int id, bool includeInstances)
    {
        var result = _reportService.FindById(id, includeInstances);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ReportModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add content for the specified 'id'.
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
        var report = _reportService.FindById(result.Id);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportModel(report!, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
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
        var report = _reportService.FindById(result.Id, true);
        return new JsonResult(new ReportModel(report!, _serializerOptions));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
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
        var subjectTemplateText = model.Settings.GetDictionaryJsonValue<string>("subject") ?? "";
        var subjectTemplate = _templateEngine.AddOrUpdateTemplateInMemory($"report-{model.Id}-subject", subjectTemplateText);
        var bodyTemplate = _templateEngine.AddOrUpdateTemplateInMemory($"report-{model.Id}", model.Template);

        var results = await _reportService.FindContentWithElasticsearchAsync(_elasticOptions.PublishedIndex, model.ToEntity(_serializerOptions));
        var sections = model.ParseSections().ToDictionary(s => s.Key, s =>
        {
            results.TryGetValue(s.Key, out SearchResultModel<ContentModel>? value);
            s.Value.Content = value?.Hits.Hits.Select(h => h.Source) ?? Array.Empty<ContentModel>();
            return s.Value;
        });

        var templateModel = new TemplateModel(sections);

        foreach (var frontPage in templateModel.Content.Where(x => x.ContentType == Entities.ContentType.Image))
        {
            frontPage.ImageContent = GetImageContent(frontPage.FileReferences.FirstOrDefault()?.Path);
        }

        var subject = await subjectTemplate.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
        });
        var body = await bodyTemplate.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
        });

        return new JsonResult(new ReportPreviewModel(subject, body, results));
    }

    private string? GetImageContent(string? path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
        if (!safePath.FileExists()) return null;

        using FileStream fileStream = new(safePath, FileMode.Open, FileAccess.Read);
        var imageBytes = new byte[fileStream.Length];
        fileStream.Read(imageBytes, 0, (int)fileStream.Length);
        return Convert.ToBase64String(imageBytes);
    }
    #endregion
}
