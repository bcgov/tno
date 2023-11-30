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
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Filters;
using TNO.TemplateEngine.Models.Reports;

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
    private readonly JsonSerializerOptions _serializerOptions;
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
    /// <param name="serializerOptions"></param>
    public ReportController(
        IReportService reportService,
        IReportInstanceService reportInstanceService,
        IUserService userService,
        IReportHelper reportHelper,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportService = reportService;
        _reportInstanceService = reportInstanceService;
        _userService = userService;
        _reportHelper = reportHelper;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _serializerOptions = serializerOptions.Value;
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

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
    /// <returns></returns>
    [HttpGet("{reportId}/instances")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindInstancesForReportId(int reportId, int? ownerId)
    {
        var result = _reportInstanceService.FindInstancesForReportId(reportId, ownerId);
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var result = _reportService.FindById(model.Id) ?? throw new NoContentException("Report does not exist");
        if (result?.OwnerId != user?.Id) throw new NotAuthorizedException("Not authorized to update this report");
        _reportService.ClearChangeTracker(); // Remove the report from context.
        result = _reportService.Update(model.ToEntity(_serializerOptions));
        var instanceModel = model.Instances.FirstOrDefault();
        Entities.ReportInstance? instance = null;
        if (updateInstances && instanceModel != null)
        {
            // Only update the first instance.
            instance = _reportInstanceService.Update((Entities.ReportInstance)instanceModel);
        }
        _reportInstanceService.CommitTransaction();
        var report = _reportService.FindById(result.Id) ?? throw new NoContentException("Report does not exist");
        if (updateInstances && instance != null)
        {
            report.Instances.Add(instance);
            instance.ContentManyToMany.Clear();
            instance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(instance.Id));
        }

        if (updateInstances && instance != null)
        {
            // Inform other users of the updated content.
            var instanceContent = report.Instances.FirstOrDefault()?.ContentManyToMany.ToArray() ?? Array.Empty<Entities.ReportInstanceContent>();
            foreach (var content in instanceContent)
            {
                // Inform other users of the updated content.
                if (content.Content != null && !content.Content.IsPrivate)
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
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
    [ProducesResponseType(typeof(ReportResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> Preview(int id)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to preview this report"); // Report is not public
        var result = await _reportHelper.GenerateReportAsync(new Services.Models.Report.ReportModel(report, _serializerOptions), user.Id, true);
        return new JsonResult(result);
    }

    /// <summary>
    /// Execute the report template and generate the results for reviewing.
    /// This generates a report instance.
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
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id && // User does not own the report
            !report.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == user.Id) &&  // User is not subscribed to the report
            !report.IsPublic) throw new NotAuthorizedException("Not authorized to review this report"); // Report is not public

        var instances = _reportService.GetLatestInstances(id, user.Id);
        var currentInstance = instances.FirstOrDefault();
        if (regenerate && currentInstance != null && currentInstance.SentOn.HasValue == false)
        {
            // Generate a new instance of this report because the request asked for it, and the last instance was already sent.
            var regeneratedInstance = await _reportHelper.GenerateReportInstanceAsync(new Services.Models.Report.ReportModel(report, _serializerOptions), user.Id, currentInstance.Id);
            _reportInstanceService.ClearChangeTracker();
            currentInstance.ContentManyToMany.RemoveAll(c => c.Content?.IsPrivate == false);
            var count = 0;
            currentInstance.ContentManyToMany.ForEach(c => c.SortOrder = count++);
            var newContent = regeneratedInstance.ContentManyToMany.Select(c =>
            {
                c.SortOrder = count++;
                return c;
            });
            currentInstance.ContentManyToMany.AddRange(newContent);
            currentInstance = _reportInstanceService.UpdateAndSave(currentInstance);
            instances = _reportService.GetLatestInstances(id, user.Id);
            currentInstance.ContentManyToMany.Clear();
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
            report.Instances.Clear();
            report.Instances.Add(currentInstance);
            report.Instances.AddRange(instances.Where(i => i.Id != currentInstance.Id));
        }
        else if (currentInstance == null || currentInstance.SentOn.HasValue)
        {
            // Generate a new instance of this report if there is no current instance, or if it has already been sent.
            currentInstance = await _reportHelper.GenerateReportInstanceAsync(new Services.Models.Report.ReportModel(report, _serializerOptions), user.Id);
            currentInstance = _reportInstanceService.AddAndSave(currentInstance);
            instances = _reportService.GetLatestInstances(id, user.Id);
            currentInstance.ContentManyToMany.Clear();
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
            report.Instances.Clear();
            report.Instances.Add(currentInstance);
            report.Instances.AddRange(instances.Where(i => i.Id != currentInstance.Id));
        }
        else
        {
            // Get the content for the current instance.
            currentInstance.ContentManyToMany.AddRange(_reportInstanceService.GetContentForInstance(currentInstance.Id));
        }

        return new JsonResult(new ReportModel(report, _serializerOptions));
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> SendToAsync(int id, string to)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to send this report"); // User does not own the report

        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, report.Id, new { })
        {
            RequestorId = user.Id,
            To = to,
            // UpdateCache = true,
            GenerateInstance = false
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
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> Publish(int id)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var report = _reportService.FindById(id) ?? throw new NoContentException("Report does not exist");
        if (report.OwnerId != user.Id) throw new NotAuthorizedException("Not authorized to publish this report"); // User does not own the report

        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, report.Id, new { })
        {
            RequestorId = user.Id
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, $"report-{report.Id}", request);
        return new OkResult();
    }
    #endregion
}
