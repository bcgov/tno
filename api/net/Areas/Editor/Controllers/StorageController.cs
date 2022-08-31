using System.Net;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Storage;
using TNO.API.Models;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Helpers;

namespace TNO.API.Areas.Editor.Controllers;

// TODO: Handle multiple storage locations.
/// <summary>
/// StorageController class, provides Storage endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("editor")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/storage")]
[Route("api/[area]/storage")]
[Route("v{version:apiVersion}/[area]/storage")]
[Route("[area]/storage")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class StorageController : ControllerBase
{
    #region Variables
    private readonly StorageOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StorageController object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public StorageController(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Fetch an array of files and directories at the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpGet("{location}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult GetFolder([FromQuery] string? path, [FromRoute] string? location = "capture")
    {
        var result = new FolderModel(_options.GetRootPath(location), path.MakeRelativePath());
        return new JsonResult(result);
    }

    /// <summary>
    /// Upload a file and link it to the specified content.
    /// Only a single file can be linked to content, each upload will overwrite.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="files"></param>
    /// <param name="overwrite"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    [HttpPost("{location}/upload")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> Upload(string path, [FromForm] List<IFormFile> files, bool overwrite = false, [FromRoute] string? location = "capture")
    {
        if (files.Count == 0) throw new InvalidOperationException("File missing");
        var file = files.First();

        var safePath = Path.Combine(_options.GetRootPath(location), path.MakeRelativePath(), file.FileName);
        if (String.IsNullOrWhiteSpace(Path.GetFileName(safePath))) throw new InvalidOperationException("Filename missing");
        if (safePath.DirectoryExists()) throw new InvalidOperationException("Invalid path");

        var directory = Path.GetDirectoryName(safePath);
        if (directory?.DirectoryExists() == false)
            Directory.CreateDirectory(directory);

        if (!overwrite && safePath.FileExists()) throw new InvalidOperationException("File already exists");

        // TODO: Handle multiple files.
        using var stream = System.IO.File.Open(safePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return new JsonResult(new ItemModel(safePath));
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
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Stream(string path, [FromRoute] string? location = "capture")
    {
        var safePath = Path.Combine(_options.GetRootPath(location), path.MakeRelativePath());
        if (!safePath.FileExists()) throw new InvalidOperationException($"Stream does not exist: '{path}'");

        var info = new ItemModel(safePath);
        var stream = System.IO.File.OpenRead(safePath);
        return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: true);
    }

    /// <summary>
    /// Download the file for the specified path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpGet("download")]
    [HttpGet("{location}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Download(string path, [FromRoute] string? location = "capture")
    {
        var safePath = Path.Combine(_options.GetRootPath(location), path.MakeRelativePath());
        if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

        // TODO: download a full folder as a ZIP
        var info = new ItemModel(safePath);
        var stream = System.IO.File.OpenRead(safePath);
        return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: false);
    }

    /// <summary>
    /// Moves a file from specified 'path' to 'destination'.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpPut("move")]
    [HttpPut("{location}/move")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Move(string path, string destination, [FromRoute] string? location = "capture")
    {
        var rootPath = _options.GetRootPath(location);
        var safePath = Path.Combine(rootPath, path.MakeRelativePath());
        if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File does not exist: '{path}'");

        var safeDestination = Path.Combine(rootPath, destination.MakeRelativePath());
        if (safeDestination.FileExists()) throw new InvalidOperationException("File already exists, cannot rename");

        var directory = Path.GetDirectoryName(safeDestination);
        if (directory?.DirectoryExists() == false)
            Directory.CreateDirectory(directory);

        System.IO.File.Move(safePath, safeDestination);
        var info = new ItemModel(safeDestination);
        return new JsonResult(info);
    }

    /// <summary>
    /// Delete file at path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpDelete("{location}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Delete(string path, [FromRoute] string? location = "capture")
    {
        var safePath = Path.Combine(_options.GetRootPath(location), path.MakeRelativePath());
        if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

        var item = new ItemModel(safePath);
        System.IO.File.Delete(safePath);
        return new JsonResult(item);
    }

    /// <summary>
    /// Make a clip from the target file identified in the clip parameter 'prefix'.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="outputName"></param>
    /// <returns></returns>
    [HttpPost("clip")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> CreateClipAsync(string path, int start, int end, string outputName)
    {
        var safePath = Path.Combine(_options.CapturePath, path.MakeRelativePath());
        var file = await FfmpegHelper.CreateClipAsync(safePath, start, end, outputName);
        return new JsonResult(new ItemModel(file));
    }

    /// <summary>
    /// Make a clip from the target file identified in the clip parameter 'prefix'.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    [HttpPost("join")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> JoinClipsAsync(string path, string prefix)
    {
        var safePath = Path.Combine(_options.CapturePath, path.MakeRelativePath());
        var file = await FfmpegHelper.JoinClipsAsync(safePath, prefix);
        return new JsonResult(new ItemModel(file));
    }
    #endregion
}
