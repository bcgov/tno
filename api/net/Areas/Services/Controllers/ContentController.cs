using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Tag;
using TNO.API.Areas.Admin.Models.Topic;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Config;
using TNO.API.Models;
using TNO.API.Models.SignalR;
using TNO.API.SignalR;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;
using TNO.Keycloak;

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
    private readonly ITopicService _topicService;
    private readonly StorageOptions _storageOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly KafkaHubConfig _kafkaHubOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IHubContext<MessageHub> _hub;
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
    /// <param name="topicService"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="kafkaHubOptions"></param>
    /// <param name="hub"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IUserService userService,
        ITagService tagService,
        ITopicService topicService,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaHubConfig> kafkaHubOptions,
        IHubContext<MessageHub> hub,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _userService = userService;
        _tagService = tagService;
        _topicService = topicService;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _kafkaHubOptions = kafkaHubOptions.Value;
        _hub = hub;
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
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindById(long id)
    {
        var result = _contentService.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ContentModel(result));
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
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindByUid([FromQuery] string uid, [FromQuery] string? source)
    {
        var result = _contentService.FindByUid(uid, source);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ContentModel(result));
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
    public async Task<IActionResult> AddAsync(ContentModel model, int? requestorId = null)
    {
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
        foreach (var tag in newTags) {
            var tagModel = new TagModel
            {
                Code = tag.Code,
                Name = tag.Name
            };
            var result = _tagService.AddAndSave((TNO.Entities.Tag)tagModel);
            tag.Id = result.Id;
        }

        var content = _contentService.AddAndSave((Content)model);

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

        return new JsonResult(new ContentModel(content));

        // TODO: Figure out how to return a 201 for a route in a different controller.
        // return CreatedAtRoute("EditorContentFindById", new { id = content.Id }, new ContentModel(content));
        // return CreatedAtRoute("EditorContentFindById", new { id = content.Id }, new ContentModel(content));
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
    public async Task<IActionResult> UpdateAsync(ContentModel model, bool index = false, int? requestorId = null)
    {
        var content = _contentService.UpdateAndSave((Content)model);

        // Send notification to user who requested the content.
        if (content.OwnerId.HasValue)
        {
            var owner = content.Owner ?? _userService.FindById(content.OwnerId.Value);
            if (!String.IsNullOrWhiteSpace(owner?.Username))
                await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendUser, owner.Username, new InvocationMessage("Content", new[] { new ContentMessageModel(content) })));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new InvocationMessage("Content", new[] { new ContentMessageModel(content) })));
        }

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

        return new JsonResult(new ContentModel(content));
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
    public IActionResult UpdateStatusAsync(ContentModel model)
    {
        var content = _contentService.UpdateStatusOnly((Content)model);

        return new JsonResult(new ContentModel(content));
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

        // Send notification to user who requested the content.
        if (content.OwnerId.HasValue)
        {
            var owner = content.Owner ?? _userService.FindById(content.OwnerId.Value);
            if (!String.IsNullOrWhiteSpace(owner?.Username))
                await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendUser, owner.Username, new InvocationMessage("Content", new[] { new ContentMessageModel(content) })));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaHubOptions.HubTopic, new KafkaHubMessage(HubEvent.SendAll, new InvocationMessage("Content", new[] { new ContentMessageModel(content) })));
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
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
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
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> GetImageFile(long id)
    {
        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault();
        if (fileReference == null)
        {
            _logger.LogWarning("The file reference (id: {id}) does not exist.", id);
            return NoContent();
        }

        var safePath = Path.Combine(
            _storageOptions.GetUploadPath(),
            HttpUtility.UrlDecode(fileReference.Path).MakeRelativePath());
        if (!safePath.FileExists())
        {
            _logger.LogWarning("The image file (id: {id}) does not exist.", id);
            return NoContent();
        }

        using var fileStream = new FileStream(safePath, FileMode.Open, FileAccess.Read);
        var imageBytes = new byte[fileStream.Length];
        await fileStream.ReadAsync(imageBytes.AsMemory(0, (int)fileStream.Length));
        return new JsonResult(Convert.ToBase64String(imageBytes));
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

    /// <summary>
    /// Find the notifications that have been sent for the specified content 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/notifications")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<NotificationInstanceModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult GetNotificationsFor(long id)
    {
        var notifications = _contentService.GetNotificationsFor(id);
        return new JsonResult(notifications.Select(n => new NotificationInstanceModel(n, _serializerOptions)));
    }
    #endregion
}
