using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Content;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.API.Config;
using TNO.Kafka.Models;
using TNO.Keycloak;
using TNO.API.Areas.Editor.Models.MorningReport;
using System.Net.Mime;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// MorningReportController class, provides Content endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/morning/reports")]
[Route("api/[area]/morning/reports")]
[Route("v{version:apiVersion}/[area]/morning/reports")]
[Route("[area]/morning/reports")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class MorningReportController : ControllerBase
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly IActionService _actionService;
    private readonly IUserService _userService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MorningReportController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="actionService"></param>
    /// <param name="userService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="logger"></param>
    public MorningReportController(
        IContentService contentService,
        IActionService actionService,
        IUserService userService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<MorningReportController> logger)
    {
        _contentService = contentService;
        _actionService = actionService;
        _userService = userService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Perform the specified 'action' to the specified array of content.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel[]), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Morning-Report" })]
    public async Task<IActionResult> UpdateContentAsync(ContentListModel model)
    {
        var items = _contentService.Find(new ContentFilter()
        {
            Quantity = model.ContentIds.Count(),
            ContentIds = model.ContentIds.ToArray()
        }, false).Items;

        var update = new List<Content>();
        var action = !String.IsNullOrWhiteSpace(model.ActionName) ? _actionService.FindByName(model.ActionName) : null;
        foreach (var content in items)
        {
            if (model.Action == ContentListAction.Publish)
            {
                if (content.Status != ContentStatus.Published)
                {
                    content.Status = ContentStatus.Publish;
                    _contentService.Update(content);
                    update.Add(content);
                }
            }
            else if (model.Action == ContentListAction.Unpublish)
            {
                if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                {
                    content.Status = ContentStatus.Unpublish;
                    _contentService.Update(content);
                    update.Add(content);
                }
            }
            else if (model.Action == ContentListAction.Remove)
            {
                if (!content.IsHidden)
                {
                    content.IsHidden = true;
                    _contentService.Update(content);
                    update.Add(content);
                }
            }
            else if (model.Action == ContentListAction.Action)
            {
                if (action == null) throw new InvalidOperationException($"Action specified '{model.ActionName}' does not exist.");
                _contentService.FindById(content.Id);
                var currentAction = content.ActionsManyToMany.FirstOrDefault(a => a.Action!.Name == model.ActionName);
                if (currentAction == null)
                {
                    content.ActionsManyToMany.Add(new ContentAction(content, action, model.ActionValue ?? ""));
                    _contentService.Update(content);
                    update.Add(content);
                }
                else if (currentAction.Value != model.ActionValue)
                {
                    currentAction.Value = model.ActionValue ?? "";
                    _contentService.Update(content);
                    update.Add(content);
                }
            }
        }

        // Save all changes in a single transaction.
        _contentService.CommitTransaction();

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            foreach (var content in update)
            {
                // If a request is submitted to unpublish we do it regardless of the current state of the content.
                if (content.Status == ContentStatus.Unpublish)
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, IndexAction.Unpublish));

                // Any request to publish, or if content is already published, we will republish.
                if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, IndexAction.Publish));

                // Always index the content.
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, IndexAction.Index));
            }
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

        return new JsonResult(update.Select(c => new ContentModel(c)).ToArray());
    }
    #endregion
}
