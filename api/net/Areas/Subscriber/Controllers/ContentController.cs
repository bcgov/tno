using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Content;
using TNO.API.Areas.Subscriber.Models.Storage;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.Models.Filters;

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
    private readonly StorageOptions _storageOptions;
    private readonly ElasticOptions _elasticOptions;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    /// <param name="storageOptions"></param>
    /// <param name="elasticOptions"></param>
    public ContentController(
        IContentService contentService,
        IFileReferenceService fileReferenceService,
        IOptions<StorageOptions> storageOptions,
        IOptions<ElasticOptions> elasticOptions)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _storageOptions = storageOptions.Value;
        _elasticOptions = elasticOptions.Value;
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
        var useUnpublished = filter.UseUnpublished == true;
        if (filter.Actions.Any(x => x.Contains("Commentary")) && filter.Quantity == 10)
        {
            filter.Quantity = 500;
        }
        var result = await _contentService.FindWithElasticsearchAsync(useUnpublished ? _elasticOptions.UnpublishedIndex : _elasticOptions.PublishedIndex, filter);
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
    /// Find todays front pages.
    /// </summary>
    /// <returns></returns>
    [HttpGet("frontpages")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> FindFrontPages()
    {
        var result = await _contentService.FindFrontPages(_elasticOptions.PublishedIndex);
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
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Stream([FromQuery] string path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
        if (!safePath.FileExists()) throw new NoContentException("File does not exist");

        var info = new ItemModel(safePath);
        var filestream = System.IO.File.OpenRead(safePath);
        return File(filestream, info.MimeType!);
    }


    #endregion
}
