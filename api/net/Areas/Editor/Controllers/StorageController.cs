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
    private readonly StorageConfig _config;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StorageController object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    public StorageController(IOptions<StorageConfig> options)
    {
        _config = options.Value;
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
        var result = new FolderModel(_config.GetRootPath(location), path.MakeRelativePath());
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

        var safePath = System.IO.Path.Combine(_config.GetRootPath(location), path.MakeRelativePath(), file.FileName);
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
        var safePath = System.IO.Path.Combine(_config.GetRootPath(location), path.MakeRelativePath());
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
        var safePath = System.IO.Path.Combine(_config.GetRootPath(location), path.MakeRelativePath());
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
        var rootPath = _config.GetRootPath(location);
        var safePath = System.IO.Path.Combine(rootPath, path.MakeRelativePath());
        if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File does not exist: '{path}'");

        var safeDestination = System.IO.Path.Combine(rootPath, destination.MakeRelativePath());
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
        var safePath = System.IO.Path.Combine(_config.GetRootPath(location), path.MakeRelativePath());
        if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

        var info = new ItemModel(safePath);
        System.IO.File.Delete(safePath);
        return new JsonResult(info);
    }

    /// <summary>
    /// Make a clip from the target file identified in the clip parameter.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="clipNbr"></param>
    /// <returns></returns>
    [HttpGet("clip")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult CreateClip(string fileName, string directory, int start, int end, int clipNbr, string target)
    {
        var path = _config.CapturePath + directory;
        var process = GetClipProcess(fileName, path, start, end, clipNbr, target);

        process.Start();
        process.WaitForExit();

        var listing = new FolderModel(_config.CapturePath, directory);
        return new JsonResult(listing);
    }

    /// <summary>
    /// Make a clip from the target file identified in the clip parameter.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <returns></returns>
    [HttpPut("join")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult JoinClips(string fileName, string directory, string target)
    {
        var safePath = System.IO.Path.Combine(_config.GetRootPath("storage"), directory.MakeRelativePath());
        var format = Path.GetExtension(fileName).Replace(".", "");

        var muxFile = GenerateMuxfile(fileName, safePath, format, target);
        var process = GetJoinProcess(directory, fileName, safePath, muxFile, format, target);

        process.Start();
        process.WaitForExit();

        System.IO.File.Delete(muxFile);

        var listing = new FolderModel(_config.CapturePath, directory);
        return new JsonResult(listing);
    }
    #endregion

    #region Support Routines
    /// <summary>
    /// Get the process for the specified clip command.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="clipNbr"></param>
    /// <returns>System.Diagnostics.Process</returns>
    protected virtual System.Diagnostics.Process GetClipProcess(string fileName, string directory, int start, int end, int clipNbr, string target)
    {
        var cmd = GenerateClipCommand(fileName, directory, start, end, clipNbr, target);
        var process = new System.Diagnostics.Process();
        process = InitializeProcess(process, cmd, "clip");

        return process;
    }

    /// <summary>
    /// Generate the command for the clip process.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="clipNbr"></param>
    /// <returns></returns>
    protected string GenerateClipCommand(string fileName, string directory, int start, int end, int clipNbr, string target)
    {
        var format = Path.GetExtension(fileName).Replace(".", "");
        var duration = end - start;
        var input = directory + "/" + fileName;
        var output = directory + "/" + target + "_" + clipNbr + "." + format;
        var otherArgs = "-ab 64k -ar 22050 -vol 400";

        return $"ffmpeg -ss {start} -f {format} -t {duration} -i {input} {otherArgs} -y {output}";
    }

    /// <summary>
    /// Get the process for the specified clip command.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    protected virtual System.Diagnostics.Process GetJoinProcess(string directory, string fileName, string path, string muxFile, string format, string target)
    {
        var cmd = GenerateJoinCommand(directory, fileName, path, muxFile, format, target);
        var process = new System.Diagnostics.Process();
        process = InitializeProcess(process, cmd, "join");

        return process;
    }

    /// <summary>
    /// Get the process for the specified clip command.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cmd"></param>
    /// <param name="verb"></param>
    /// <returns></returns>
    protected virtual System.Diagnostics.Process InitializeProcess(System.Diagnostics.Process process, string cmd, string verb)
    {
        process.StartInfo.Verb = verb;
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"{cmd}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.EnableRaisingEvents = true;

        return process;
    }

    /// <summary>
    /// Generate the command for the clip process.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    protected string GenerateJoinCommand(string directory, string fileName, string path, string muxFile, string format, string target)
    {
        var input = directory + "/" + fileName;
        var output = path + "/" + target + "-snippet." + format;

        return $"ffmpeg -f concat -safe 0 -i {muxFile} {output}";
    }

    /// <summary>
    /// Create a muxfile that joins multiple clips into a snippet.
    /// </summary>
    /// <param name="listing"></param>
    /// <param name="fileName"></param>
    /// <param name="format"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    protected string GenerateMuxfile(string fileName, string safepath, string format, string target)
    {
        var clips = System.IO.Directory.GetFileSystemEntries(safepath, target + "_*." + format);
        var path = "/tmp/" + target + ".txt";
        Array.Sort(clips, string.CompareOrdinal);
        var sw = System.IO.File.CreateText(path);

        foreach (string file in clips)
        {
            sw.WriteLine("file " + file);
        }

        sw.Close();

        return path;
    }
    #endregion
}
