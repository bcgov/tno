using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Report;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ReportController class, provides Report endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    private readonly IReportHelper _reportHelper;
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly IImpersonationHelper _impersonate;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<ReportController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportService"></param>
    /// <param name="reportInstanceService"></param>
    /// <param name="userService"></param>
    /// <param name="reportHelper"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="impersonateHelper"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ReportController(
        IReportService reportService,
        IReportInstanceService reportInstanceService,
        IUserService userService,
        IReportHelper reportHelper,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IImpersonationHelper impersonateHelper,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _reportInstanceService = reportInstanceService;
        _userService = userService;
        _reportHelper = reportHelper;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _impersonate = impersonateHelper;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all "my" reports.
    /// </summary>
    /// <returns></returns>
    [HttpGet("my-reports")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindMyReports()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var user = _impersonate.GetCurrentUser();

        var filter = new ReportFilter(query)
        {
            OwnerId = user.Id
        };
        return new JsonResult(_reportService.Find(filter).Select(ds => new ReportModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find all "available" reports.
    /// </summary>
    /// <returns></returns>
    [HttpGet("public")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindPublicReports()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        var filter = new ReportFilter(query)
        {
            IsPublic = true
        };
        return new JsonResult(_reportService.Find(filter).Select(ds => new ReportModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find report for the specified 'id'.
    /// Also includes the user's unsent report instance.
    /// If request includes 'generate=true' then make sure to return an unsent instance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeContent">Include content for the most recent instance.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindById(int id, bool includeContent = false)
    {
        var user = _impersonate.GetCurrentUser();

        var report = _reportService.FindById(id) ?? throw new NoContentException();
        if (report.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to view this report");

        var instances = _reportService.GetLatestInstances(id, user.Id);
        report.Instances.Clear();
        report.Instances.AddRange(instances);
        if (instances.Any() && includeContent)
        {
            var instance = instances.First();
            instance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(instance.Id));
        }

        return new JsonResult(new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Find all report instances for the specified report 'id' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="page"></param>
    /// <param name="qty"></param>
    /// <returns></returns>
    [HttpGet("{reportId}/instances")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindInstancesForReportId(int reportId, int? ownerId, int? page, int? qty)
    {
        var skip = page > 0 ? page.Value - 1 : 0;
        var take = qty > 0 ? qty.Value : 10;
        var result = _reportInstanceService.FindInstancesForReportId(reportId, ownerId, skip, take);
        return new JsonResult(result.Select(ri => new ReportInstanceModel(ri)));
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
        var user = _impersonate.GetCurrentUser();
        model.OwnerId = user.Id;

        // If there are no subscribers, add the owner as a subscriber.
        if (!model.Subscribers.Any())
        {
            model.Subscribers = new[] { new UserReportModel() { UserId = user.Id, ReportId = model.Id, IsSubscribed = true } };
        }

        var result = _reportService.AddAndSave(model.ToEntity(_serializerOptions));
        var report = _reportService.FindById(result.Id) ?? throw new NoContentException("Report does not exist");
        return CreatedAtAction(nameof(FindById), new { id = report.Id }, new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Update report for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateInstances"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> UpdateAsync(ReportModel model, [FromQuery] bool updateInstances = false)
    {
        var user = _impersonate.GetCurrentUser();
        var result = _reportService.FindById(model.Id) ?? throw new NoContentException("Report does not exist");
        if (result?.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to update this report");
        result = _reportService.Update(model.ToEntity(_serializerOptions));
        var instanceModel = model.Instances.FirstOrDefault();
        Entities.ReportInstance? instance = null;
        if (updateInstances && instanceModel != null)
        {
            // Only update the first instance.
            instance = _reportInstanceService.Update((Entities.ReportInstance)instanceModel, true);
        }
        _reportInstanceService.CommitTransaction();
        var report = _reportService.FindById(result.Id) ?? throw new NoContentException("Report does not exist");
        if (updateInstances && instance != null && instanceModel != null)
        {
            var instances = _reportService.GetLatestInstances(model.Id, user.Id).Select(i => i.Id == instance.Id ? instance : i);
            report.Instances.Clear();
            report.Instances.AddRange(instances);
            instance.ContentManyToMany.Clear();
            instance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(instance.Id));

            // Inform other users of the updated content.
            foreach (var content in instance.ContentManyToMany)
            {
                var contentModel = instanceModel.Content.FirstOrDefault(c => c.ContentId == content.ContentId && c.InstanceId == content.InstanceId && c.SectionName == content.SectionName);
                // Inform other users of the updated content.
                if (content.Content != null &&
                    contentModel?.Content != null &&
                    !content.Content.IsPrivate &&
                    content.Content.Version != contentModel.Content.Version)
                    await _kafkaProducer.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(content.Content) })));
            }
        }
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
        var user = _impersonate.GetCurrentUser();
        var result = _reportService.FindById(model.Id) ?? throw new NoContentException("Report does not exist");
        if (result?.OwnerId != user?.Id) throw new NotAuthorizedException("Not authorized to delete this report");
        _reportService.ClearChangeTracker(); // Remove the report from context.

        _reportService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }

    /// <summary>
    /// Execute the report template and generate the results for previewing.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/preview")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TNO.TemplateEngine.Models.Reports.ReportResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> Preview(int id)
    {
        var user = _impersonate.GetCurrentUser();
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to preview this report"); // Report is not public
        var result = await _reportHelper.GenerateReportAsync(new Services.Models.Report.ReportModel(report, _serializerOptions), user.Id, false, true);
        return new JsonResult(result);
    }

    /// <summary>
    /// Execute the report template and generate the results for reviewing.
    /// This generates a report instance if one does not currently exist.
    /// If an instance already exists the 'regenerate' option controls the behaviour.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="regenerate"></param>
    /// <returns></returns>
    [HttpPost("{id}/generate")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> Generate(int id, [FromQuery] bool regenerate = false)
    {
        var user = _impersonate.GetCurrentUser();
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to review this report"); // Report is not public

        var instances = _reportService.GetLatestInstances(id, user.Id);
        var currentInstance = instances.FirstOrDefault();
        if (currentInstance == null)
        {
            // Generate a new instance of this report if there is no current instance, or if it has already been sent.
            currentInstance = await _reportService.GenerateReportInstanceAsync(id, user.Id);
            _reportInstanceService.ClearChangeTracker();
            currentInstance = _reportInstanceService.AddAndSave(currentInstance);
            instances = _reportService.GetLatestInstances(id, user.Id);
            currentInstance.ContentManyToMany.Clear();
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
            report.Instances.Clear();
            report.Instances.AddRange(instances);
        }
        else if (regenerate && currentInstance.SentOn.HasValue)
        {
            // Generate a new instance because the prior was sent to CHES.
            currentInstance = await _reportService.GenerateReportInstanceAsync(id, user.Id);
            _reportInstanceService.ClearChangeTracker();
            currentInstance = _reportInstanceService.AddAndSave(currentInstance);
            instances = _reportService.GetLatestInstances(id, user.Id);
            currentInstance.ContentManyToMany.Clear();
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
            report.Instances.Clear();
            report.Instances.AddRange(instances);
        }
        else if (regenerate && currentInstance.SentOn.HasValue == false)
        {
            // Regenerate the current instance, but do not create a new instance.
            var regeneratedInstance = await _reportService.GenerateReportInstanceAsync(id, user.Id, currentInstance.Id);
            _reportInstanceService.ClearChangeTracker();
            currentInstance.ContentManyToMany.Clear();
            var count = 0;
            var newContent = regeneratedInstance.ContentManyToMany.Select(c =>
            {
                c.SortOrder = count++;
                return c;
            });
            currentInstance.ContentManyToMany.AddRange(newContent);
            currentInstance = _reportInstanceService.UpdateAndSave(currentInstance, true);
            instances = _reportService.GetLatestInstances(id, user.Id);
            currentInstance.ContentManyToMany.Clear();
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
            report.Instances.Clear();
            report.Instances.AddRange(instances);
        }
        else
        {
            // Get the content for the current instance.
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
        }

        return new JsonResult(new ReportModel(report, _serializerOptions));
    }

    /// <summary>
    /// Regenerate the current report instance for the specified 'sectionId' in the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException"></exception>
    /// <exception cref="NoContentException"></exception>
    [HttpPost("{id}/sections/{sectionId}/regenerate")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> RegenerateSection(int id, int sectionId)
    {
        var user = _impersonate.GetCurrentUser();
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to review this report"); // Report is not public

        var instance = await _reportService.RegenerateReportInstanceSectionAsync(id, sectionId, user.Id);
        _reportInstanceService.ClearChangeTracker();
        instance = _reportInstanceService.UpdateAndSave(instance, true);
        instance.ContentManyToMany.Clear();
        instance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(instance.Id));

        return new JsonResult(new ReportInstanceModel(instance));
    }

    /// <summary>
    /// Add the specified content to the current report instance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException"></exception>
    /// <exception cref="NoContentException"></exception>
    [HttpPost("{id}/content")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> AddContentToReportAsync(int id, [FromBody] IEnumerable<ReportInstanceContentModel> content)
    {
        var user = _impersonate.GetCurrentUser();
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to review this report"); // Report is not public

        var result = await _reportService.AddContentToReportAsync(id, user.Id, content.Select((c) => (Entities.ReportInstanceContent)c)) ?? throw new NoContentException("Report does not exist");

        var instances = _reportService.GetLatestInstances(id, user.Id);
        var currentInstance = instances.FirstOrDefault() ?? throw new InvalidOperationException("Unable to add content to a report without an instance");
        currentInstance.ContentManyToMany.Clear();
        currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
        result.Instances.Clear();
        result.Instances.AddRange(instances);

        return new JsonResult(new ReportModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find all content currently in any of 'my' reports current instances.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotAuthorizedException"></exception>
    [HttpGet("all-content")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Dictionary<int, long[]>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult GetAllContentInMyReports()
    {
        var user = _impersonate.GetCurrentUser();
        var result = _reportService.GetAllContentInMyReports(user.Id);
        return new JsonResult(result);
    }
    #endregion
}
