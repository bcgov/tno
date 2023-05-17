using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Storage;
using TNO.API.Areas.Editor.Models.Content;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.DAL.Config;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Core.Extensions;
using TNO.Kafka;
using TNO.API.Config;
using TNO.Kafka.Models;
using System.Net.Mime;
using TNO.API.Helpers;
using TNO.Models.Extensions;
using System.Web;
using System.Text.Json;
using TNO.API.SignalR;
using Microsoft.AspNetCore.SignalR;
using TNO.Keycloak;
using TNO.Core.Exceptions;

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
    private readonly IUserService _userService;
    private readonly IActionService _actionService;
    private readonly StorageOptions _storageOptions;
    private readonly IConnectionHelper _connection;
    private readonly IHubContext<MessageHub> _hub;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    /// <param name="workOrderService"></param>
    /// <param name="userService"></param>
    /// <param name="actionService"></param>
    /// <param name="hub"></param>
    /// <param name="connection"></param>
    /// <param name="storageOptions"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IWorkOrderService workOrderService,
        IUserService userService,
        IActionService actionService,
        IHubContext<MessageHub> hub,
        IConnectionHelper connection,
        IOptions<StorageOptions> storageOptions,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _workOrderService = workOrderService;
        _userService = userService;
        _actionService = actionService;
        _storageOptions = storageOptions.Value;
        _hub = hub;
        _connection = connection;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
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
        var result = await _contentService.FindWithElasticsearchAsync(filter);
        var page = new Paged<Services.Models.Content.ContentModel>(
            result.Items,
            result.Page,
            result.Quantity,
            result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult FindById(long id)
    {
        var result = _contentService.FindById(id);

        if (result == null) return new NoContentResult();
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
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var newContent = (Content)model;
        newContent.OwnerId = user.Id;
        var content = _contentService.AddAndSave(newContent);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Publish));
            else
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Index));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");
        var updateContent = (Content)model;
        updateContent.OwnerId ??= user.Id;

        var content = _contentService.UpdateAndSave(updateContent);

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

        return new JsonResult(new ContentModel(content));
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
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

        var update = new List<Content>();
        var items = _contentService.FindWithDatabase(new ContentFilter()
        {
            Quantity = model.ContentIds.Count(),
            ContentIds = model.ContentIds.ToArray(),
            IncludeHidden = true
        }, false).Items;

        foreach (var content in items)
        {
            if (model.Action == ContentListAction.Publish)
            {
                if (content.Status != ContentStatus.Published && content.Status != ContentStatus.Publish)
                {
                    content.Status = ContentStatus.Publish;
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
                var currentAction = latestContent?.ActionsManyToMany.FirstOrDefault(a => a.Action!.Name == model.ActionName);
                if (currentAction == null)
                {
                    var action = (!string.IsNullOrWhiteSpace(model.ActionName) ?
                        _actionService.FindByName(model.ActionName) :
                        null) ?? throw new InvalidOperationException($"Action specified '{model.ActionName}' does not exist.");
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
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

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
        var content = _contentService.UpdateAndSave((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            var username = User.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequestModel(content.Id, user.Id, IndexAction.Publish));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
            var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException("User does not exist");

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
        var content = _contentService.FindById(id) ?? throw new InvalidOperationException("Entity does not exist");

        if (!files.Any()) throw new InvalidOperationException("No file uploaded");

        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version; // TODO: Handle concurrency before uploading the file as it will result in an orphaned file.
        if (content.FileReferences.Any()) await _fileReferenceService.UploadAsync(content, files.First(), _storageOptions.GetUploadPath());
        else await _fileReferenceService.UploadAsync(new ContentFileReference(content, files.First()), _storageOptions.GetUploadPath());

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
        var fileReference = _fileReferenceService.FindByContentId(id).FirstOrDefault() ?? throw new InvalidOperationException("File does not exist");
        var stream = _fileReferenceService.Download(fileReference, _storageOptions.GetUploadPath());
        return File(stream, fileReference.ContentType);
    }

    /// <summary>
    /// Stream the file for the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("stream")]
    [ProducesResponseType(typeof(OkObjectResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(OkObjectResult), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> StreamAsync([FromQuery] string path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
        if (!safePath.FileExists()) throw new InvalidOperationException("File does not exist");

        var info = new ItemModel(safePath);
        using var stream = System.IO.File.OpenRead(safePath);
        var fileStreamResult = File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: true);
        using var memoryStream = new MemoryStream();
        await fileStreamResult.FileStream.CopyToAsync(memoryStream);
        var result = Convert.ToBase64String(memoryStream.ToArray());

        return Ok(result);
    }

    /// <summary>
    /// Attach an existing video/audio clip as the content of this snippet.
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
    public IActionResult AttachFile([FromRoute] long contentId, [FromRoute] int locationId, [FromQuery] long version, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var content = _contentService.FindById(contentId) ?? throw new InvalidOperationException("Entity does not exist");
        content.Version = version;

        var dataLocation = _connection.GetDataLocation(locationId);
        if (dataLocation?.Connection?.ConnectionType == ConnectionType.LocalVolume)
        {
            var configuration = _connection.GetConfiguration(dataLocation.Connection);
            var locationPath = configuration.GetDictionaryJsonValue<string>("path") ?? "";

            var safePath = Path.Combine(locationPath, path);
            if (!safePath.FileExists()) throw new InvalidOperationException("File does not exist");

            var file = new FileInfo(safePath);
            // If the content has a file reference, then update it.  Otherwise, add one.
            if (content.FileReferences.Any()) _fileReferenceService.Attach(content, file, locationPath);
            else _fileReferenceService.Attach(new ContentFileReference(content, file), locationPath);

            return new JsonResult(new ContentModel(content));
        }
        else if (dataLocation?.Connection == null)
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
            if (!safePath.FileExists()) throw new InvalidOperationException("File does not exist");

            var file = new FileInfo(safePath);
            // If the content has a file reference, then update it.  Otherwise, add one.
            if (content.FileReferences.Any()) _fileReferenceService.Attach(content, file, _storageOptions.GetUploadPath(), false);
            else _fileReferenceService.Attach(new ContentFileReference(content, file), _storageOptions.GetUploadPath());

            return new JsonResult(new ContentModel(content));
        }
        throw new NotImplementedException($"Data location type '{dataLocation?.Connection?.ConnectionType}' has not been configured");
    }
    #endregion
    #endregion
}
