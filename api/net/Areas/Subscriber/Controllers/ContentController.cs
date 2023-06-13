using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Content;
using TNO.API.Areas.Subscriber.Models.Storage;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.DAL.Config;
using TNO.Entities.Models;
using TNO.Core.Extensions;
using TNO.Kafka;
using TNO.API.Config;
using System.Net.Mime;
using TNO.API.Helpers;
using System.Web;
using System.Text.Json;
using TNO.API.SignalR;
using Microsoft.AspNetCore.SignalR;
using TNO.Keycloak;
using TNO.Elastic;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ContentController class, provides Content endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    private readonly ElasticOptions _elasticOptions;

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
    /// <param name="elasticOptions"></param>
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
        IOptions<ElasticOptions> elasticOptions,
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
        _elasticOptions = elasticOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
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
        var result = await _contentService.FindWithElasticsearchAsync(_elasticOptions.PublishedIndex, filter);
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
    /// Find todays front pages.
    /// </summary>
    /// <returns></returns>
    [HttpGet("frontpages")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> FindFrontPages()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var result = await _contentService.FindFrontPages(_elasticOptions.UnpublishedIndex);
        var page = new Paged<Services.Models.Content.ContentModel>(
            result.Items,
            result.Page,
            result.Quantity,
            result.Total);
        return new JsonResult(page);
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


    #endregion
}
