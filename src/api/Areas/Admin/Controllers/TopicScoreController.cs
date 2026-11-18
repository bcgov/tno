using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.TopicScoreRule;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Scoring;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// TopicScoreController class, provides the topic score administration endpoints: sources that use
/// topics, per-source rule ordering, the rule tester, and bulk rescore.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/topics/scores")]
[Route("api/[area]/topics/scores")]
[Route("v{version:apiVersion}/[area]/topics/scores")]
[Route("[area]/topics/scores")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class TopicScoreController : ControllerBase
{
    #region Variables
    private readonly ITopicScoreService _scoreService;
    private readonly ITopicScoreRuleService _ruleService;
    private readonly IUserService _userService;
    private readonly IWorkOrderRequestSender _workOrderSender;
    private readonly ILogger<TopicScoreController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicScoreController object, initializes with specified parameters.
    /// </summary>
    /// <param name="scoreService"></param>
    /// <param name="ruleService"></param>
    /// <param name="userService"></param>
    /// <param name="workOrderSender"></param>
    /// <param name="logger"></param>
    public TopicScoreController(
        ITopicScoreService scoreService,
        ITopicScoreRuleService ruleService,
        IUserService userService,
        IWorkOrderRequestSender workOrderSender,
        ILogger<TopicScoreController> logger)
    {
        _scoreService = scoreService;
        _ruleService = ruleService;
        _userService = userService;
        _workOrderSender = workOrderSender;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Sources that use topics, with their rule count and default score.
    /// </summary>
    /// <returns></returns>
    [HttpGet("sources")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicScoreSourceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult FindSources()
    {
        return new JsonResult(_scoreService.FindSourceSummaries().Select(ToModel));
    }

    /// <summary>
    /// Set a source's default score, used when none of its rules match.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("sources/{sourceId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreSourceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult UpdateSourceDefaultScore(int sourceId, [FromBody] TopicScoreSourceModel model)
    {
        return new JsonResult(ToModel(_scoreService.UpdateSourceDefaultScore(sourceId, model.TopicDefaultScore)));
    }

    /// <summary>
    /// Remove a source from topic scoring, preserving its source record, rules, and default score.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    [HttpDelete("sources/{sourceId}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult RemoveSource(int sourceId)
    {
        _scoreService.RemoveSource(sourceId);
        return NoContent();
    }

    /// <summary>
    /// A source's rules in evaluation order.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    [HttpGet("sources/{sourceId}/rules")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicScoreRuleModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult FindRules(int sourceId)
    {
        return new JsonResult(_ruleService.FindForSource(sourceId).Select(r => new TopicScoreRuleModel(r)));
    }

    /// <summary>
    /// Sections known for a source, for the rule form's section combobox.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    [HttpGet("sources/{sourceId}/sections")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult FindSections(int sourceId)
    {
        return new JsonResult(_scoreService.FindKnownSections(sourceId));
    }

    /// <summary>
    /// Save a new order for a source's rules.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="ruleIds">Every rule of the source, in the new order.</param>
    /// <returns></returns>
    [HttpPut("sources/{sourceId}/rules/order")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicScoreRuleModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult ReorderRules(int sourceId, [FromBody] int[] ruleIds)
    {
        return new JsonResult(_ruleService.Reorder(sourceId, ruleIds).Select(r => new TopicScoreRuleModel(r)));
    }

    /// <summary>
    /// Test the rules against a saved content item or entered values, explaining the result.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("test")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicScoreTestResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult Test([FromBody] TopicScoreTestRequestModel model)
    {
        TopicScoreInput input;
        bool isEligible;
        if (model.ContentId.HasValue)
        {
            input = _scoreService.GetInput(model.ContentId.Value) ?? throw new NoContentException("Content does not exist");
            isEligible = _scoreService.IsEligible(model.ContentId.Value);
        }
        else
        {
            if (!model.SourceId.HasValue) throw new ArgumentException("A content ID or a source is required.");
            input = new TopicScoreInput(model.SourceId, model.SeriesId, model.Section, model.Page, model.HasImage, model.PublishedOn, model.CharacterCount);
            isEligible = _scoreService.IsEligible(Entities.ContentType.PrintContent, model.SourceId, model.SeriesId);
        }

        var result = _scoreService.Calculate(input, true);
        return new JsonResult(new TopicScoreTestResultModel()
        {
            IsEligible = isEligible,
            Score = result.Score,
            RuleId = result.RuleId,
            IsSourceDefault = result.IsSourceDefault,
            Input = new TopicScoreTestRequestModel()
            {
                ContentId = model.ContentId,
                SourceId = input.SourceId,
                SeriesId = input.SeriesId,
                Section = input.Section,
                Page = input.Page,
                HasImage = input.HasImage,
                PublishedOn = input.PublishedOn,
                CharacterCount = input.CharacterCount,
            },
            Evaluations = result.Evaluations.Select(e => new TopicScoreRuleEvaluationModel()
            {
                RuleId = e.RuleId,
                SortOrder = e.SortOrder,
                Score = e.Score,
                IsMatch = e.IsMatch,
                FailedCondition = e.FailedCondition?.ToString(),
                Reason = e.Reason,
            }),
        });
    }

    /// <summary>
    /// Count the content whose calculated score a rescore would change.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("rescore/preview")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicRescorePreviewModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult PreviewRescore([FromBody] TopicRescoreRequestModel model)
    {
        if (model.EndOn <= model.StartOn) throw new ArgumentException("The end date must be after the start date.");
        var preview = _scoreService.PreviewRescore(model.StartOn.ToUniversalTime(), model.EndOn.ToUniversalTime(), model.SourceIds);
        return new JsonResult(new TopicRescorePreviewModel() { Total = preview.Total, Changed = preview.Changed });
    }

    /// <summary>
    /// Start a bulk rescore: a work order the Event Handler runs a page at a time, recalculating the
    /// calculated scores in the range and re-indexing the content whose score changed.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("rescore")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicRescoreJobModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public async Task<IActionResult> RescoreAsync([FromBody] TopicRescoreRequestModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var workOrder = _scoreService.AddRescore(model.StartOn.ToUniversalTime(), model.EndOn.ToUniversalTime(), model.SourceIds, user.Id);
        await _workOrderSender.SendAsync(workOrder, user.Username);
        return new JsonResult(ToModel(workOrder));
    }

    /// <summary>
    /// The most recent rescore jobs.
    /// </summary>
    /// <returns></returns>
    [HttpGet("rescore")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TopicRescoreJobModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult FindRescoreJobs()
    {
        return new JsonResult(_scoreService.FindRescores().Select(ToModel));
    }

    /// <summary>
    /// A rescore job and its progress.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("rescore/{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TopicRescoreJobModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TopicScore" })]
    public IActionResult FindRescoreJob(long id)
    {
        var workOrder = _scoreService.FindRescore(id) ?? throw new NoContentException("Rescore does not exist");
        return new JsonResult(ToModel(workOrder));
    }
    #endregion

    #region Methods
    /// <summary>
    /// The bulk rescore and its progress.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    private static TopicRescoreJobModel ToModel(WorkOrder workOrder)
        => new(workOrder, TopicScoreService.ReadRescoreConfiguration(workOrder));

    private static TopicScoreSourceModel ToModel(TNO.DAL.Models.TopicScoreSourceSummary summary)
    {
        return new TopicScoreSourceModel()
        {
            Id = summary.Id,
            Code = summary.Code,
            Name = summary.Name,
            UseInTopics = summary.UseInTopics,
            SeriesUseInTopics = summary.SeriesUseInTopics,
            RuleCount = summary.RuleCount,
            TopicDefaultScore = summary.TopicDefaultScore,
        };
    }
    #endregion
}
