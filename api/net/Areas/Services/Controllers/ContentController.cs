using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Models;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContentController class, provides Content endpoints for the api.
/// </summary>
[Authorize]
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="fileReferenceService"></param>
    public ContentController(IContentService contentService, IFileReferenceService fileReferenceService)
    {
        _contentService = contentService;
        _fileReferenceService = fileReferenceService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find content for the specified 'uid' and 'source'.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    [HttpGet("find")]
    [Produces("application/json")]
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
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ContentModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Content" })]
    public IActionResult Add(ContentModel model)
    {
        var result = _contentService.Add((Content)model);
        return new JsonResult(new ContentModel(result));

        // TODO: Figure out how to return a 201 for a route in a different controller.
        // return CreatedAtRoute("EditorContentFindById", new { id = result.Id }, new ContentModel(result));
        // return CreatedAtRoute("EditorContentFindById", new { id = result.Id }, new ContentModel(result));
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
    #endregion
}
