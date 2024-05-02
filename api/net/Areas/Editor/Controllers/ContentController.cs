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
using TNO.DAL.Config;
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
    private readonly IWorkOrderService _workOrderService;
    private readonly IWorkOrderHelper _workOrderHelper;
    private readonly IUserService _userService;
    private readonly IActionService _actionService;
    private readonly ISourceService _sourceService;
    private readonly StorageOptions _storageOptions;
    private readonly IConnectionHelper _connection;
    private readonly ITopicScoreHelper _topicScoreHelper;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
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
    /// <param name="sourceService"></param>
    /// <param name="connection"></param>
    /// <param name="topicScoreHelper"></param>
    /// <param name="storageOptions"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IWorkOrderService workOrderService,
        IWorkOrderHelper workOrderHelper,
        IUserService userService,
        IActionService actionService,
        ISourceService sourceService,
        IConnectionHelper connection,
        ITopicScoreHelper topicScoreHelper,
        IOptions<StorageOptions> storageOptions,
        IOptions<ElasticOptions> elasticOptions,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _workOrderService = workOrderService;
        _workOrderHelper = workOrderHelper;
        _userService = userService;
        _actionService = actionService;
        _sourceService = sourceService;
        _storageOptions = storageOptions.Value;
        _connection = connection;
        _topicScoreHelper = topicScoreHelper;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _elasticOptions = elasticOptions.Value;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
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
    /// TODO: The model stored in Elasticsearch is a little confusing based on the controller using it.  Need to clean up.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<Services.Models.Content.ContentModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> FindWithElasticsearchAsync()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var filter = new ContentFilter(query);
        var result = await _contentService.FindWithElasticsearchAsync(_elasticOptions.UnpublishedIndex, filter);
        var page = new Paged<Services.Models.Content.ContentModel>(
            result.Items,
            result.Page,
            result.Quantity,
            result.Total);
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
        var result = await _contentService.FindWithElasticsearchAsync(includeUnpublishedContent ? _elasticOptions.UnpublishedIndex : _elasticOptions.PublishedIndex, filter);
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
    public async Task<IActionResult> AddAsync(ContentModel model)
    {
        // Always make the user who created the content the owner.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var newContent = (Content)model;
        newContent.OwnerId = user.Id;
        if (!newContent.PostedOn.HasValue &&
            (newContent.Status == ContentStatus.Publish ||
            newContent.Status == ContentStatus.Published))
            newContent.PostedOn = DateTime.UtcNow;

        // only assign a default score to content which has a source relevant to Event of the Day
        if (newContent.SourceId.HasValue)
        {
            var source = _sourceService.FindById(newContent.SourceId.Value);
            if (source != null && source.UseInTopics)
                _topicScoreHelper.SetContentScore(newContent);
        }

        var content = _contentService.AddAndSave(newContent);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentAdded, new[] { new ContentMessageModel(content) })));

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Index));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
    public async Task<IActionResult> UpdateAsync(ContentModel model)
    {
        // Always make the user who updated the content the owner if the owner is currently empty.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var updateContent = (Content)model;
        updateContent.OwnerId ??= user.Id;
        if (!updateContent.PostedOn.HasValue &&
            (updateContent.Status == ContentStatus.Publish ||
            updateContent.Status == ContentStatus.Published))
            updateContent.PostedOn = DateTime.UtcNow;

        // only assign a default score to content which has a source relevant to Event of the Day
        if (updateContent.SourceId.HasValue)
        {
            var source = _sourceService.FindById(updateContent.SourceId.Value);
            if (source != null && source.UseInTopics)
                _topicScoreHelper.SetContentScore(updateContent);
        }

        var content = _contentService.UpdateAndSave(updateContent);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(content) })));

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            // If a request is submitted to unpublish we do it regardless of the current state of the content.
            if (content.Status == ContentStatus.Unpublish)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Unpublish));
            else if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Index));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
    public async Task<IActionResult> UpdateTopicsAsync(long id, IEnumerable<ContentTopicModel> topics)
    {
        // Always make the user who updated the content the owner if the owner is currently empty.
        var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

        var updatedTopics = _contentService.AddOrUpdateContentTopics(id, topics.ToList().ConvertAll(x => x.ToEntity(id)));

        var updatedContent = _contentService.FindById(id) ?? throw new NoContentException("Failed to find content");

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(updatedContent) })));

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            if (updatedContent.Status == ContentStatus.Publish || updatedContent.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(updatedContent.Id, user.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(updatedContent.Id, user.Id, IndexAction.Index));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

        return new JsonResult(updatedTopics.ToList().ConvertAll(x => new ContentTopicModel(x)));
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
    public async Task<IActionResult> UpdateContentAsync(ContentListModel model)
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

            // Always make the user who updated the content the owner if the owner is currently empty.
            content.OwnerId ??= user.Id;
        }

        // Save all changes in a single transaction.
        _contentService.CommitTransaction();

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            foreach (var content in update)
            {
                await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new ContentMessageModel(content) })));

                // If a request is submitted to unpublish we do it regardless of the current state of the content.
                if (content.Status == ContentStatus.Unpublish)
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Unpublish));

                // Any request to publish, or if content is already published, we will republish.
                if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Publish));

                // Always index the content.
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Index));

            }
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
    public async Task<IActionResult> DeleteAsync(ContentModel model)
    {
        _contentService.DeleteAndSave((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(model.Id, user.Id, IndexAction.Delete));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
    public async Task<IActionResult> PublishAsync(ContentModel model)
    {
        if (model.Status != ContentStatus.Published) model.Status = ContentStatus.Publish;
        model.PostedOn = DateTime.UtcNow;
        var content = _contentService.UpdateAndSave((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Publish));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
    public async Task<IActionResult> UnpublishAsync(ContentModel model)
    {
        var content = _contentService.UpdateAndSave((Content)model);
        if (!new[] { ContentStatus.Published }.Contains(content.Status)) throw new InvalidOperationException("Content is an invalid status, and cannot be unpublished.");

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Unpublish));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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

        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version; // TODO: Handle concurrency before uploading the file as it will result in an orphaned file.
        if (content.FileReferences.Any()) await _fileReferenceService.UploadAsync(content, files.First(), _storageOptions.GetUploadPath());
        else await _fileReferenceService.UploadAsync(new ContentFileReference(content, files.First()), _storageOptions.GetUploadPath());

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

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
    public IActionResult DownloadFile(long id)
    {
        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault() ?? throw new NoContentException("File does not exist");
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
    public IActionResult Stream([FromQuery] string path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
        if (!safePath.FileExists()) throw new NoContentException("File does not exist");

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

            if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
            {
                var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
                var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
                if (updatedContent.Status == ContentStatus.Publish || updatedContent.Status == ContentStatus.Published)
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(updatedContent.Id, user.Id, IndexAction.Publish));
                else
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(updatedContent.Id, user.Id, IndexAction.Index));
            }
            else
                _logger.LogWarning("Kafka indexing topic not configured.");
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

            if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
            {
                var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
                var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
                if (updatedContent.Status == ContentStatus.Publish || updatedContent.Status == ContentStatus.Published)
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(updatedContent.Id, user.Id, IndexAction.Publish));
                else
                    await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(updatedContent.Id, user.Id, IndexAction.Index));
            }
            else
                _logger.LogWarning("Kafka indexing topic not configured.");

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
