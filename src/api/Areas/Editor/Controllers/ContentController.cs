using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Content;
using TNO.API.Areas.Editor.Models.Storage;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Storage;
using TNO.DAL.Config;
using TNO.DAL.Helpers;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Extensions;
using TNO.Models.Filters;
namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// ContentController class, provides Content endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/contents")]
[Route("api/[area]/contents")]
[Route("v{version:apiVersion}/[area]/contents")]
[Route("[area]/contents")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentController : ControllerBase
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly IFileReferenceService _fileReferenceService;
    private readonly IS3StorageService _s3StorageService;
    private readonly IWorkOrderService _workOrderService;
    private readonly IWorkOrderHelper _workOrderHelper;
    private readonly IUserService _userService;
    private readonly IActionService _actionService;
    private readonly StorageOptions _storageOptions;
    private readonly IConnectionHelper _connection;
    private readonly ITopicScoreService _topicScoreService;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    private readonly ElasticOptions _elasticOptions;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    /// <param name="workOrderService"></param>
    /// <param name="workOrderHelper"></param>
    /// <param name="userService"></param>
    /// <param name="actionService"></param>
    /// <param name="connection"></param>
    /// <param name="topicScoreService"></param>
    /// <param name="storageOptions"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    /// <param name="s3StorageService"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IWorkOrderService workOrderService,
        IWorkOrderHelper workOrderHelper,
        IUserService userService,
        IActionService actionService,
        IConnectionHelper connection,
        ITopicScoreService topicScoreService,
        IOptions<StorageOptions> storageOptions,
        IOptions<ElasticOptions> elasticOptions,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ContentController> logger,
        IS3StorageService s3StorageService)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _workOrderService = workOrderService;
        _workOrderHelper = workOrderHelper;
        _userService = userService;
        _actionService = actionService;
        _storageOptions = storageOptions.Value;
        _connection = connection;
        _topicScoreService = topicScoreService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _elasticOptions = elasticOptions.Value;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
        _s3StorageService = s3StorageService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("db")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<ContentModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindWithDatabase()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _contentService.FindWithDatabase(new ContentFilter(query));
        var page = new Paged<ContentModel>(result.Items.Select(i => new ContentModel(i)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <param name="includeUnpublishedContent"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("search")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> FindWithElasticsearchAsync([FromBody] JsonDocument filter, [FromQuery] bool includeUnpublishedContent = false)
    {
        var result = await _contentService.FindWithElasticsearchAsync(includeUnpublishedContent ? _elasticOptions.ContentIndex : _elasticOptions.PublishedIndex, filter);
        return new JsonResult(result);
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindById(long id)
    {
        var result = _contentService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new ContentModel(result));
    }

    /// <summary>
    /// Find the content items linked to the specified content (either direction), optionally
    /// filtered by link value; defaults to 'duplicate' links recorded by the automation dedupe.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpGet("{id}/links")]
    [Produces(MediaTypeNames.Application.Json)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindLinkedContent(long id, [FromQuery] string? value = "duplicate")
    {
        var linked = _contentService.FindLinkedContent(id, string.IsNullOrWhiteSpace(value) ? null : value);
        return new JsonResult(linked.Select(c => new
        {
            id = c.Id,
            headline = c.Headline,
            source = c.Source?.Name ?? c.OtherSource,
            publishedOn = c.PublishedOn,
            status = c.Status.ToString(),
        }));
    }

    /// <summary>
    /// Add the new content to the database.
    /// Publish message to kafka to index content in elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> AddAsync([FromBody] ContentModel model)
    {
        // Always make the user who created the content the owner.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var newContent = (Content)model;
        newContent.OwnerId = user.Id;
        if (!newContent.PostedOn.HasValue)
            newContent.PostedOn = DateTime.UtcNow;

        // Scores are calculated when the content is saved; the editor never sets them here.
        _topicScoreService.PrepareNewContentTopics(newContent, false);

        // The index request is recorded in the same transaction as the content.
        _contentService.RequestIndex(newContent, newContent.Status == ContentStatus.Publish || newContent.Status == ContentStatus.Published ? IndexRequestAction.Publish : IndexRequestAction.Index, user.Id);
        var content = _contentService.AddAndSave(newContent);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentAdded, new[] { new ContentMessageModel(content) })));

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return CreatedAtAction(nameof(FindById), new { id = content.Id }, new ContentModel(content));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// Publish message to kafka to index content in elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateAsync([FromBody] ContentModel model)
    {
        // Always make the user who updated the content the owner if the owner is currently empty.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var updateContent = (Content)model;
        updateContent.OwnerId ??= user.Id;
        if (!updateContent.PostedOn.HasValue)
            updateContent.PostedOn = DateTime.UtcNow;

        // A story saved without a topic is given "Not Applicable" when the rules now score it.
        _topicScoreService.AddSystemTopicWhenScored(updateContent);

        // If a request is submitted to unpublish we do it regardless of the current state of the content.
        _contentService.RequestIndex(updateContent, requestorId: user.Id);
        var content = _contentService.UpdateAndSave(updateContent);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(content) })));

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Update content topics for a piece of content.
    /// Publish message to kafka to index content in elasticsearch.
    /// </summary>
    /// <param name="id">id of the content item to update</param>
    /// <param name="topics">the new set of topics for the content</param>
    /// <returns></returns>
    [HttpPut("{id}/topics")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ContentTopicModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateTopicsAsync(long id, [FromBody] IEnumerable<ContentTopicModel> topics)
    {
        // Always make the user who updated the content the owner if the owner is currently empty.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        // The index action follows the content's saved status.
        _contentService.RequestIndex(id, requestorId: user.Id);
        var updatedTopics = _contentService.AddOrUpdateContentTopics(id, topics.ToList().ConvertAll(x => x.ToEntity(id)), GetCalculatedTopicScore(id));

        var updatedContent = _contentService.FindById(id) ?? throw new NoContentException("Failed to find content");

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(updatedContent) })));

        return new JsonResult(updatedTopics.ToList().ConvertAll(x => new ContentTopicModel(x)));
    }

    /// <summary>
    /// Clear the score override of a content topic, so its score is recalculated from the topic
    /// score rules.
    /// </summary>
    /// <param name="id">The content.</param>
    /// <param name="topicId">The topic.</param>
    /// <returns></returns>
    [HttpPut("{id}/topics/{topicId}/reset")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ContentTopicModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> ResetTopicScoreAsync(long id, int topicId)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        _topicScoreService.RequestIndex(id, requestorId: user.Id);
        _topicScoreService.ResetOverride(id, topicId);
        var updatedContent = _contentService.FindById(id) ?? throw new NoContentException("Failed to find content");

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(updatedContent) })));

        return new JsonResult(updatedContent.TopicsManyToMany.Select(x => new ContentTopicModel(x)));
    }

    /// <summary>
    /// The score the topic score rules give the content, or null when it is not scored.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    private int? GetCalculatedTopicScore(long contentId)
    {
        if (!_topicScoreService.IsEligible(contentId)) return null;
        var input = _topicScoreService.GetInput(contentId);
        return input == null ? null : _topicScoreService.Calculate(input).Score;
    }

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
    public async Task<IActionResult> UpdateContentAsync([FromBody] ContentListModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        var update = new List<Content>();
        var items = _contentService.FindWithDatabase(new ContentFilter()
        {
            Quantity = model.ContentIds.Count(),
            ContentIds = model.ContentIds.ToArray(),
        }, false).Items;

        foreach (var content in items)
        {
            var countBefore = update.Count;
            if (model.Action == ContentListAction.Publish)
            {
                if (content.Status != ContentStatus.Published && content.Status != ContentStatus.Publish)
                {
                    content.Status = ContentStatus.Publish;
                    content.PostedOn = DateTime.UtcNow;
                    update.Add(_contentService.Update(content));
                }
            }
            else if (model.Action == ContentListAction.Unpublish)
            {
                if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                {
                    content.Status = ContentStatus.Unpublish;
                    update.Add(_contentService.Update(content));
                }
            }
            else if (model.Action == ContentListAction.Hide)
            {
                if (!content.IsHidden)
                {
                    content.IsHidden = true;
                    update.Add(_contentService.Update(content));
                }
            }
            else if (model.Action == ContentListAction.Unhide)
            {
                if (content.IsHidden)
                {
                    content.IsHidden = false;
                    update.Add(_contentService.Update(content));
                }
            }
            else if (model.Action == ContentListAction.Action)
            {
                var latestContent = _contentService.FindById(content.Id);
                var currentAction = latestContent?.ActionsManyToMany.FirstOrDefault(a => a.Action?.Id == model.ActionId);
                if (currentAction == null)
                {
                    var action = (model.ActionId.HasValue ?
                        _actionService.FindById(model.ActionId.Value) :
                        null) ?? throw new InvalidOperationException($"Action specified '{model.ActionId}' does not exist.");
                    latestContent?.ActionsManyToMany.Add(new ContentAction(latestContent, action, model.ActionValue ?? ""));
                    if (latestContent != null) update.Add(_contentService.Update(latestContent));
                }
                else if (currentAction.Value != model.ActionValue)
                {
                    currentAction.Value = model.ActionValue ?? "";
                    if (latestContent != null) update.Add(_contentService.Update(latestContent));
                }
            }

            // Make the user who updated the content the owner if the owner is currently empty.
            // Only rows that actually changed may be touched - assigning ownership to unchanged
            // rows bumps their version without reindexing them, which desynchronizes
            // Elasticsearch and breaks optimistic concurrency for every later editor save.
            if (update.Count > countBefore) update[^1].OwnerId ??= user.Id;
        }

        foreach (var content in update)
        {
            // If a request is submitted to unpublish we do it regardless of the current state of the content.
            if (content.Status == ContentStatus.Unpublish)
                _contentService.RequestIndex(content, IndexRequestAction.Unpublish, user.Id);

            // Any request to publish, or if content is already published, we will republish.
            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                _contentService.RequestIndex(content, IndexRequestAction.Publish, user.Id);

            // Always index the content.
            _contentService.RequestIndex(content, IndexRequestAction.Index, user.Id);
        }

        // Save all changes and their index requests in a single transaction.
        _contentService.CommitTransaction();

        foreach (var content in update)
            await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(content) })));

        return new JsonResult(update.Select(c => new ContentModel(c)).ToArray());
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// Publish message to kafka to remove content from elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> DeleteAsync([FromBody] ContentModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        _contentService.RequestIndex(model.Id, IndexRequestAction.Delete, user.Id);
        _contentService.DeleteAndSave((Content)model);

        return new JsonResult(model);
    }

    /// <summary>
    /// Publish content for the specified 'id'.
    /// Publish message to kafka to index content in elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}/publish")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> PublishAsync([FromBody] ContentModel model)
    {
        if (model.Status != ContentStatus.Published) model.Status = ContentStatus.Publish;
        model.PostedOn = DateTime.UtcNow;
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var updateContent = (Content)model;
        _contentService.RequestIndex(updateContent, IndexRequestAction.Publish, user.Id);
        var content = _contentService.UpdateAndSave(updateContent);

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Unpublish content for the specified 'id'.
    /// Publish message to kafka to remove indexed content from elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}/unpublish")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult UnpublishAsync([FromBody] ContentModel model)
    {
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var updateContent = (Content)model;
        // Content saved in any other status is not unpublished (the request fails after the save, as before).
        if (updateContent.Status == ContentStatus.Published) _contentService.RequestIndex(updateContent, IndexRequestAction.Unpublish, user.Id);
        var content = _contentService.UpdateAndSave(updateContent);
        if (!new[] { ContentStatus.Published }.Contains(content.Status)) throw new InvalidOperationException("Content is an invalid status, and cannot be unpublished.");

        return new JsonResult(new ContentModel(content));
    }

    #region Files
    /// <summary>
    /// Upload a file and link it to the specified content.
    /// Only a single file can be linked to content, each upload will overwrite.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    [HttpPost("{id}/upload")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UploadFile([FromRoute] long id, [FromQuery] long version, [FromForm] List<IFormFile> files)
    {
        var content = _contentService.FindById(id) ?? throw new NoContentException("Entity does not exist");

        if (!files.Any()) throw new InvalidOperationException("No file uploaded");
        if (files.Count > 1) throw new InvalidOperationException("Only one file can be uploaded at a time");

        var file = files[0];
        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version; // TODO: Handle concurrency before uploading the file as it will result in an orphaned file.

        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        _fileReferenceService.RequestIndex(content, content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published ? IndexRequestAction.Publish : IndexRequestAction.Index, user.Id);

        // save file reference
        var updatedFileReference = content.FileReferences.Any()
            ? await _fileReferenceService.UploadAsync(content, file, _storageOptions.GetUploadPath())
            : await _fileReferenceService.UploadCleanUpAsync(new ContentFileReference(content, file), _storageOptions.GetUploadPath());

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id))
            await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        if (updatedFileReference.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ||
            updatedFileReference.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var filePath = Path.Combine(_storageOptions.GetUploadPath(), updatedFileReference.Path);
                var duration = await FfmpegHelper.GetVideoDurationAsync(filePath);
                updatedFileReference.RunningTime = (int)Math.Round(duration * 1000);
                await _fileReferenceService.UpdateAsync(updatedFileReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing duration for file: {Path}, contentId: {ContentId}", updatedFileReference.Path, updatedFileReference.ContentId);
            }
        }

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Find content for the specified 'id' and download the file it references.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> DownloadFileAsync(long id)
    {
        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault() ?? throw new NoContentException("File does not exist");
        if (fileReference.IsSyncedToS3 && !string.IsNullOrWhiteSpace(fileReference.S3Path))
        {
            var s3Stream = await _s3StorageService.DownloadFromS3Async(fileReference.S3Path);
            if (s3Stream != null)
                return File(s3Stream, fileReference.ContentType);
        }
        var stream = _fileReferenceService.Download(fileReference, _storageOptions.GetUploadPath());
        return File(stream, fileReference.ContentType);
    }

    /// <summary>
    /// Stream the file for the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> StreamAsync([FromQuery] string path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        // find file from local
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);

        if (!safePath.FileExists())
        {
            // find file from s3
            var stream = await _s3StorageService.DownloadFromS3Async(path);
            if (stream != null) return File(stream, "application/octet-stream");
            else throw new NoContentException("File does not exist");
        }

        var info = new ItemModel(safePath);
        var fileStream = System.IO.File.OpenRead(safePath);
        return File(fileStream, info.MimeType!);
    }

    /// <summary>
    /// Attach an existing video/audio clip as the file.
    /// Only a single file can be linked to content, each new attachment will overwrite.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="locationId"></param>
    /// <param name="version"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPut("{contentId}/{locationId}/attach")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> AttachFileAsync([FromRoute] long contentId, [FromRoute] int locationId, [FromQuery] long version, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var content = _contentService.FindById(contentId) ?? throw new NoContentException("Entity does not exist");
        content.Version = version;
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        // Recorded by the file reference save, in its transaction.
        _fileReferenceService.RequestIndex(content, content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published ? IndexRequestAction.Publish : IndexRequestAction.Index, user.Id);

        var dataLocation = _connection.GetDataLocation(locationId);
        if (dataLocation?.Connection?.ConnectionType == ConnectionType.LocalVolume)
        {
            var configuration = _connection.GetConfiguration(dataLocation.Connection);
            var locationPath = configuration.GetDictionaryJsonValue<string>("path") ?? "";

            var safePath = Path.Combine(locationPath, path);
            if (!safePath.FileExists()) throw new NoContentException("File does not exist");

            var file = new FileInfo(safePath);
            // If the content has a file reference, then update it.  Otherwise, add one.
            if (content.FileReferences.Any()) _fileReferenceService.Attach(content, file, locationPath);
            else _fileReferenceService.Attach(new ContentFileReference(content, file), locationPath);

            if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

            var updatedContent = new ContentModel(content);
            return new JsonResult(updatedContent);
        }
        else if (dataLocation?.Connection == null)
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
            if (!safePath.FileExists()) throw new NoContentException("File does not exist");

            var file = new FileInfo(safePath);
            // If the content has a file reference, then update it.  Otherwise, add one.
            if (content.FileReferences.Any()) _fileReferenceService.Attach(content, file, _storageOptions.GetUploadPath(), false);
            else _fileReferenceService.Attach(new ContentFileReference(content, file), _storageOptions.GetUploadPath());

            if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

            var updatedContent = new ContentModel(content);

            return new JsonResult(updatedContent);
        }
        throw new NotImplementedException($"Data location type '{dataLocation?.Connection?.ConnectionType}' has not been configured");
    }

    /// <summary>
    /// Find the notifications that have been sent for the specified content 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/notifications")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<NotificationInstanceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult GetNotificationsFor(long id)
    {
        var notifications = _contentService.GetNotificationsFor(id);
        return new JsonResult(notifications.Select(n => new NotificationInstanceModel(n, _serializerOptions)));
    }
    #endregion
    #endregion
}
