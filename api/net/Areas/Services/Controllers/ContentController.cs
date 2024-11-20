using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Tag;
using TNO.API.Areas.Admin.Models.Topic;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;
using TNO.Models.Filters;
using SignalRModels = TNO.API.Models.SignalR;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContentController class, provides Content endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
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
    private readonly IUserService _userService;
    private readonly ITagService _tagService;
    private readonly IQuoteService _quoteService;
    private readonly ITopicService _topicService;
    private readonly ISourceService _sourceService;
    private readonly ITopicScoreHelper _topicScoreHelper;
    private readonly IWorkOrderHelper _workOrderHelper;
    private readonly StorageOptions _storageOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    /// <param name="userService"></param>
    /// <param name="tagService"></param>
    /// <param name="quoteService"></param>
    /// <param name="topicService"></param>
    /// <param name="sourceService"></param>
    /// <param name="topicScoreHelper"></param>
    /// <param name="workOrderHelper"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IUserService userService,
        ITagService tagService,
        IQuoteService quoteService,
        ITopicService topicService,
        ISourceService sourceService,
        ITopicScoreHelper topicScoreHelper,
        IWorkOrderHelper workOrderHelper,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _userService = userService;
        _tagService = tagService;
        _quoteService = quoteService;
        _topicService = topicService;
        _sourceService = sourceService;
        _topicScoreHelper = topicScoreHelper;
        _workOrderHelper = workOrderHelper;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _storageOptions = storageOptions.Value;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeUserNotifications"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindById(long id, bool includeUserNotifications = false)
    {
        var result = _contentService.FindById(id, includeUserNotifications);
        if (result == null) return NoContent();
        return new JsonResult(new ContentModel(result, _serializerOptions));
    }

    /// <summary>
    /// Find content for the specified 'uid' and 'source'.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    [HttpGet("find")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindByUid([FromQuery] string uid, [FromQuery] string? source)
    {
        var result = _contentService.FindByUid(uid, source);
        if (result == null) return NoContent();
        return new JsonResult(new ContentModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add new content to the database.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> AddAsync([FromBody] ContentModel model, int? requestorId = null)
    {
        AddNewContentModelData(model);

        // only assign a default score to content which has a source relevant to Event of the Day
        if (model.SourceId.HasValue)
        {
            var source = _sourceService.FindById(model.SourceId.Value);
            if (source != null && source.UseInTopics)
                _topicScoreHelper.SetContentScore(model);
        }

        if (!model.PostedOn.HasValue)
            model.PostedOn = DateTime.UtcNow;

        var content = _contentService.AddAndSave((Content)model);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentAdded, new[] { new SignalRModels.ContentMessageModel(content) })));

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            Entities.User? user = null;
            if (requestorId.HasValue)
            {
                user = _userService.FindById(requestorId.Value);
            }
            else
            {
                var username = User.GetUsername();
                if (!String.IsNullOrWhiteSpace(username))
                    user = _userService.FindByUsername(username);
            }

            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Index));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return new JsonResult(new ContentModel(content, _serializerOptions));

        // TODO: Figure out how to return a 201 for a route in a different controller.
        // return CreatedAtRoute("EditorContentFindById", new { id = content.Id }, new ContentModel(content));
    }

    private void AddNewContentModelData(ContentModel model)
    {
        if (model == null) return;
        var newTopics = model.Topics.Where(t => t.Id == 0);
        foreach (var topic in newTopics)
        {
            var topicModel = new TopicModel
            {
                IsEnabled = false,
                Name = topic.Name,
                TopicType = topic.TopicType
            };
            var result = _topicService.AddAndSave((TNO.Entities.Topic)topicModel);
            topic.Id = result.Id;
        }

        var newTags = model.Tags.Where(t => t.Id == 0);
        foreach (var tag in newTags)
        {
            var tagModel = new TagModel
            {
                Code = tag.Code,
                Name = tag.Name
            };
            var result = _tagService.AddAndSave((TNO.Entities.Tag)tagModel);
            tag.Id = result.Id;
        }
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// Publish message to kafka to index content in elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="index">Be careful this can result in a indexing loop.</param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateAsync([FromBody] ContentModel model, bool index = false, int? requestorId = null)
    {
        AddNewContentModelData(model);
        // Published content should also have a posted date.
        if (!model.PostedOn.HasValue &&
            (model.Status == ContentStatus.Publish ||
            model.Status == ContentStatus.Published))
            model.PostedOn = DateTime.UtcNow;
        var content = _contentService.UpdateAndSave((Content)model);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new SignalRModels.ContentMessageModel(content) })));

        if (index && !String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            Entities.User? user = null;
            if (requestorId.HasValue)
            {
                user = _userService.FindById(requestorId.Value);
            }
            else
            {
                var username = User.GetUsername();
                if (!String.IsNullOrWhiteSpace(username))
                    user = _userService.FindByUsername(username);
            }

            // If a request is submitted to unpublish we do it regardless of the current state of the content.
            if (content.Status == ContentStatus.Unpublish)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Unpublish));
            else if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Index));
        }
        else if (index)
            _logger.LogWarning("Kafka indexing topic not configured.");


        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return new JsonResult(new ContentModel(content, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// Publish message to kafka to index content in elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="index">Be careful this can result in a indexing loop.</param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    [HttpPut("{id}/file")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateFileAsync([FromBody] ContentModel model, bool index = false, int? requestorId = null)
    {
        var content = (Content)model;
        var fileRef = model.FileReferences.FirstOrDefault() ?? throw new InvalidOperationException("File missing");
        _fileReferenceService.UpdateAndSave((FileReference)fileRef);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new SignalRModels.ContentMessageModel(content, "file") })));

        if (index && !String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            Entities.User? user = null;
            if (requestorId.HasValue)
            {
                user = _userService.FindById(requestorId.Value);
            }
            else
            {
                var username = User.GetUsername();
                if (!String.IsNullOrWhiteSpace(username))
                    user = _userService.FindByUsername(username);
            }

            // If a request is submitted to unpublish we do it regardless of the current state of the content.
            if (content.Status == ContentStatus.Unpublish)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Unpublish));
            else if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Index));
        }
        else if (index)
            _logger.LogWarning("Kafka indexing topic not configured.");


        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return new JsonResult(new ContentModel(content, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// Will not trigger any re-index or audit trail update
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}/status")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateStatusAsync([FromBody] ContentModel model)
    {
        var content = _contentService.UpdateStatusOnly((Content)model);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new SignalRModels.ContentMessageModel(content, "status") })));

        return new JsonResult(new ContentModel(content, _serializerOptions));
    }

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
        var content = _contentService.FindById(id);
        if (content == null) return new JsonResult(new { Error = "Content does not exist" })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        if (!files.Any()) return new JsonResult(new { Error = "No file uploaded." })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version; // TODO: Handle concurrency before uploading the file as it will result in an orphaned file.
        if (content.FileReferences.Any()) await _fileReferenceService.UploadAsync(content, files.First(), _storageOptions.GetUploadPath());
        else await _fileReferenceService.UploadAsync(new ContentFileReference(content, files.First()), _storageOptions.GetUploadPath());

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new SignalRModels.ContentMessageModel(content, "file") })));

        if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, IndexAction.Publish));
        else
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, IndexAction.Index));

        if (_workOrderHelper.ShouldAutoTranscribe(content.Id)) await _workOrderHelper.RequestTranscriptionAsync(content.Id);

        return new JsonResult(new ContentModel(content, _serializerOptions));
    }

    /// <summary>
    /// Attach quotes to a piece of content.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="quotes"></param>
    /// <returns></returns>
    [HttpPost("{id}/quotes")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> AttachQuotes([FromRoute] long id, [FromBody] IEnumerable<QuoteModel> quotes)
    {
        var content = _contentService.FindById(id);
        if (content == null) return new JsonResult(new { Error = "Content does not exist" })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        // no quotes passed in so just return the existing content model
        if (!quotes.Any()) return new JsonResult(new ContentModel(content, _serializerOptions));

        _quoteService.Attach(Array.ConvertAll(quotes.ToArray(), item => (Quote)item));

        content = _contentService.FindById(id);
        if (content == null) return new JsonResult(new { Error = "Content does not exist" })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentUpdated, new[] { new SignalRModels.ContentMessageModel(content, "quotes") })));

        return new JsonResult(new ContentModel(content, _serializerOptions));
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
        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault();

        if (fileReference == null) return new JsonResult(new { Error = "File does not exist" })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        var stream = _fileReferenceService.Download(fileReference, _storageOptions.GetUploadPath());
        return File(stream, fileReference.ContentType);
    }

    /// <summary>
    /// Find content for the specified 'id' and get the file content it references.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/image")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> GetImageFile(long id)
    {
        var result = new JsonResult(new { Error = "File does not exist" })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault();
        if (fileReference == null) return result;

        var safePath = Path.Combine(
            _storageOptions.GetUploadPath(),
            HttpUtility.UrlDecode(fileReference.Path).MakeRelativePath());
        if (!safePath.FileExists()) return result;

        using var fileStream = new FileStream(safePath, FileMode.Open, FileAccess.Read);
        var imageBytes = new byte[fileStream.Length];
        await fileStream.ReadExactlyAsync(imageBytes.AsMemory(0, (int)fileStream.Length));
        return new JsonResult(Convert.ToBase64String(imageBytes));
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

    /// <summary>
    /// Update content action.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}/actions/{actionId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentActionModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateContentActionAsync([FromBody] ContentActionModel model)
    {
        var action = _contentService.AddOrUpdateContentAction((ContentAction)model);

        await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new KafkaInvocationMessage(MessageTarget.ContentActionUpdated, new[] { new SignalRModels.ContentActionMessageModel(action), })));

        return new JsonResult(new ContentActionModel(action));
    }

    /// <summary>
    /// Re-index all content in the database.
    /// </summary>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    [HttpPost("reindex")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> ReindexAsync(int? requestorId = null)
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            Entities.User? user = null;
            if (requestorId.HasValue)
            {
                user = _userService.FindById(requestorId.Value);
            }
            else
            {
                var username = User.GetUsername();
                if (!String.IsNullOrWhiteSpace(username))
                    user = _userService.FindByUsername(username);
            }

            var index = true;
            var filter = new ContentFilter(query)
            {
                Page = 0,
                Quantity = 500,
            };

            while (index)
            {
                filter.Page++;
                var results = _contentService.FindWithDatabase(filter, true);
                foreach (var content in results.Items)
                {
                    if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                        await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Publish));
                    else
                        await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user?.Id, IndexAction.Index));
                }

                index = results.Items.Count > 0;
            }

            return new OkResult();
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

        return new BadRequestResult();
    }
    #endregion
}
