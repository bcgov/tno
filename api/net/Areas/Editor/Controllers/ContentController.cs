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

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// ContentController class, provides Content endpoints for the api.
/// </summary>
[Authorize]
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
    private readonly StorageConfig _config;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    public ContentController(IContentService contentService, IFileReferenceService fileReferenceService, IOptions<StorageConfig> options)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
        _config = options.Value;
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
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Add(ContentModel model)
    {
        var result = _contentService.Add((Content)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ContentModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Update(ContentModel model)
    {
        var result = _contentService.Update((Content)model);
        return new JsonResult(new ContentModel(result));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Delete(ContentModel model)
    {
        _contentService.Delete((Content)model);
        return new JsonResult(model);
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
        var reference = content.FileReferences.Any() ? new ContentFileReference(content.FileReferences.First(), files.First()) : new ContentFileReference(content, files.First());
        await _fileReferenceService.Upload(reference);

        return new JsonResult(new ContentModel(content));
    }

    /// <summary>
    /// Attach an existing video/audio clip as the content of this snippet.
    /// Only a single file can be linked to content, each new attachment will overwrite.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    [HttpPut("{id}/attach")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public async Task<IActionResult> AttachFile([FromRoute] long id, [FromQuery] long version, [FromQuery] string path)
    {
        var content = _contentService.FindById(id);
        if (content == null) return new JsonResult(new { Error = "Content does not exist" })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        // If the content has a file reference, then update it.  Otherwise, add one.
        content.Version = version;
        // Attachment source is always "storage"
        var safePath = System.IO.Path.Combine(_config.GetRootPath("storage"), path.MakeRelativePath());
        if (!safePath.FileExists()) throw new InvalidOperationException("Does not exist");

        var file = new System.IO.FileInfo(safePath);
        var reference = content.FileReferences.Any() ? new ContentFileReference(content.FileReferences.First(), file) : new ContentFileReference(content, file);
        await _fileReferenceService.Attach(reference);

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
        var safePath = System.IO.Path.Combine(_config.GetRootPath(location), path.MakeRelativePath());
        if (!safePath.FileExists()) throw new InvalidOperationException("Does not exist");

        var info = new ItemModel(safePath);
        var stream = System.IO.File.OpenRead(safePath);
        return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: true);
    }
    #endregion
}
