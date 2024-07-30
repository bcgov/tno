using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.AVOverview;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// AVOverviewController class, provides endpoints to manage evening overviews.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports/av/overviews")]
[Route("api/[area]/reports/av/overviews")]
[Route("v{version:apiVersion}/[area]/reports/av/overviews")]
[Route("[area]/reports/av/overviews")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class AVOverviewController : ControllerBase
{
    #region Variables
    private readonly IAVOverviewInstanceService _overviewInstanceService;
    private readonly IAVOverviewTemplateService _overviewTemplateService;
    private readonly IUserService _userService;
    private readonly IReportHelper _reportHelper;
    private readonly IKafkaMessenger _kafkaProducer;
    private readonly KafkaOptions _kafkaOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewController object, initializes with specified parameters.
    /// </summary>
    /// <param name="overviewInstanceService"></param>
    /// <param name="overviewTemplateService"></param>
    /// <param name="userService"></param>
    /// <param name="reportHelper"></param>
    /// <param name="kafkaProducer"></param>
    /// <param name="kafkaOptions"></param>
    public AVOverviewController(
        IAVOverviewInstanceService overviewInstanceService,
        IAVOverviewTemplateService overviewTemplateService,
        IUserService userService,
        IReportHelper reportHelper,
        IKafkaMessenger kafkaProducer,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _overviewInstanceService = overviewInstanceService;
        _overviewTemplateService = overviewTemplateService;
        _userService = userService;
        _reportHelper = reportHelper;
        _kafkaProducer = kafkaProducer;
        _kafkaOptions = kafkaOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find evening overviews for the specified 'publishedOn' or latest if no date passed.
    /// If one does not exist it will generate a new model based on the configured template.
    /// </summary>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindByDate([FromQuery] DateTime? publishedOn = null)
    {
        Entities.AVOverviewInstance? instance;
        if (publishedOn != null)
        {
            instance = _overviewInstanceService.FindByDate((DateTime)publishedOn);

            // If an evening overview does not exist it will generate a new one based on the template, but not save it.
            if (instance == null)
            {
                var type = ((DateTime)publishedOn).DayOfWeek == DayOfWeek.Sunday || ((DateTime)publishedOn).DayOfWeek == DayOfWeek.Saturday ? Entities.AVOverviewTemplateType.Weekend : Entities.AVOverviewTemplateType.Weekday;
                var template = _overviewTemplateService.FindById(type) ?? throw new NoContentException($"A template for '{type.GetName()}' does not exist.");
                return new JsonResult(new AVOverviewInstanceModel(template, (DateTime)publishedOn));
            }
        }
        else
        {
            instance = _overviewInstanceService.FindLatest();
            if (instance == null) return new NoContentResult();
        }
        return new JsonResult(new AVOverviewInstanceModel(instance));
    }

    /// <summary>
    /// Find evening overview for the specified 'instanceId'.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpGet("{instanceId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult FindById(long instanceId)
    {
        var result = _overviewInstanceService.FindById(instanceId) ?? throw new NoContentException();
        return new JsonResult(new AVOverviewInstanceModel(result));
    }

    /// <summary>
    /// Add new evening overview.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Add(AVOverviewInstanceModel model)
    {
        var result = _overviewInstanceService.AddAndSave((Entities.AVOverviewInstance)model);
        var instance = _overviewInstanceService.FindById(result.Id) ?? throw new NoContentException("Overview Section does not exist");
        return CreatedAtAction(nameof(FindById), new { instanceId = result.Id }, new AVOverviewInstanceModel(instance));
    }

    /// <summary>
    /// Update evening overview for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Update(AVOverviewInstanceModel model)
    {
        var result = _overviewInstanceService.UpdateAndSave((Entities.AVOverviewInstance)model);
        var instance = _overviewInstanceService.FindById(result.Id) ?? throw new NoContentException("Overview Section does not exist");
        return new JsonResult(new AVOverviewInstanceModel(instance));
    }

    /// <summary>
    /// Delete evening overview for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AVOverviewInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public IActionResult Delete(AVOverviewInstanceModel model)
    {
        _overviewInstanceService.DeleteAndSave((Entities.AVOverviewInstance)model);
        return new JsonResult(model);
    }

    /// <summary>
    /// Execute the report template and generate the results for previewing.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpPost("{instanceId}/view")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TemplateEngine.Models.Reports.ReportResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public async Task<IActionResult> View(long instanceId)
    {
        var instance = _overviewInstanceService.FindById(instanceId) ?? throw new NoContentException($"AV overview instance '{instanceId}' not found");
        var model = new TemplateEngine.Models.Reports.AVOverviewInstanceModel(instance);
        var result = await _reportHelper.GenerateReportAsync(model, false);
        return new JsonResult(result);
    }

    /// <summary>
    /// Publish the AV evening overview report and send to all subscribers.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpPost("{instanceId}/publish")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Evening Overview" })]
    public async Task<IActionResult> Publish(long instanceId)
    {
        var instance = _overviewInstanceService.FindById(instanceId) ?? throw new NoContentException($"AV overview instance '{instanceId}' not found");
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        var request = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.AVOverview, 0, instance.Id, new { })
        {
            RequestorId = user.Id
        };
        await _kafkaProducer.SendMessageAsync(_kafkaOptions.ReportingTopic, request);
        return new OkResult();
    }
    #endregion
}
