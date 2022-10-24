using System.Net;
using Microsoft.AspNetCore.Authorization;
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
using TNO.Keycloak;

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
    private readonly StorageOptions _storageOptions;
    private readonly IKafkaMessenger _kafkaMessenger;
    private readonly KafkaOptions _kafkaOptions;
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
    /// <param name="storageOptions"></param>
    /// <param name="kafkaMessenger"></param>
    /// <param name="kafkaOptions"></param>
    /// <param name="logger"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IWorkOrderService workOrderService,
        IUserService userService,
        IOptions<StorageOptions> storageOptions,
        IKafkaMessenger kafkaMessenger,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<ContentController> logger)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _workOrderService = workOrderService;
        _userService = userService;
        _storageOptions = storageOptions.Value;
        _kafkaMessenger = kafkaMessenger;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IPaged<ContentModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _contentService.Find(new ContentFilter(query));
        var page = new Paged<ContentModel>(result.Items.Select(i => new ContentModel(i)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces("application/json")]
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> AddAsync(ContentModel model)
    {
        var content = _contentService.Add((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Publish));

            // Always index the content.
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Index));
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UpdateAsync(ContentModel model)
    {
        var content = _contentService.Update((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            // If a request is submitted to unpublish we do it regardless of the current state of the content.
            if (content.Status == ContentStatus.Unpublish)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Unpublish));

            // Any request to publish, or if content is already published, we will republish.
            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
                await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Publish));

            // Always index the content.
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Index));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// Publish message to kafka to remove content from elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> DeleteAsync(ContentModel model)
    {
        _contentService.Delete((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(model.Id, IndexAction.Delete));
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> PublishAsync(ContentModel model)
    {
        if (model.Status != ContentStatus.Published) model.Status = ContentStatus.Publish;
        var content = _contentService.Update((Content)model);

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Publish));
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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UnpublishAsync(ContentModel model)
    {
        var content = _contentService.Update((Content)model);
        if (!new[] { ContentStatus.Published }.Contains(content.Status)) throw new InvalidOperationException("Content is an invalid status, and cannot be unpublished.");

        if (!String.IsNullOrWhiteSpace(_kafkaOptions.IndexingTopic))
        {
            await _kafkaMessenger.SendMessageAsync(_kafkaOptions.IndexingTopic, new IndexRequest(content.Id, IndexAction.Unpublish));
        }
        else
            _logger.LogWarning("Kafka indexing topic not configured.");

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
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> UploadFile([FromRoute] long id, [FromQuery] long version, [FromForm] List<IFormFile> files)
    {
        var content = _contentService.FindById(id) ?? throw new InvalidOperationException("Entity does not exist");

        if (!files.Any()) throw new InvalidOperationException("No file uploaded");

        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version; // TODO: Handle concurrency before uploading the file as it will result in an orphaned file.
        if (content.FileReferences.Any()) await _fileReferenceService.UploadAsync(content, files.First());
        else await _fileReferenceService.UploadAsync(new ContentFileReference(content, files.First()));

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Attach an existing video/audio clip as the content of this snippet.
    /// Only a single file can be linked to content, each new attachment will overwrite.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPut("{id}/attach")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult AttachFile([FromRoute] long id, [FromQuery] long version, [FromQuery] string path)
    {
        var content = _contentService.FindById(id) ?? throw new InvalidOperationException("Entity does not exist");

        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version;
        var safePath = Path.Combine(_storageOptions.GetRootPath("storage"), path.MakeRelativePath());
        if (!safePath.FileExists()) throw new InvalidOperationException("File does not exist");

        var file = new FileInfo(safePath);
        if (content.FileReferences.Any()) _fileReferenceService.Attach(content, file);
        else _fileReferenceService.Attach(new ContentFileReference(content, file));

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
        var stream = _fileReferenceService.Download(fileReference);
        return File(stream, fileReference.ContentType);
    }

    /// <summary>
    /// Stream the file for the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [AllowAnonymous] // TODO: Temporary to test HTML 5 video
    [HttpGet("stream")]
    [HttpGet("{location}/stream")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Stream(string path, [FromRoute] string? location = "capture")
    {
        var safePath = System.IO.Path.Combine(_storageOptions.GetRootPath(location), path.MakeRelativePath());
        if (!safePath.FileExists()) throw new InvalidOperationException("File does not exist");

        var info = new ItemModel(safePath);
        var stream = System.IO.File.OpenRead(safePath);
        return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: true);
    }
    #endregion
}
