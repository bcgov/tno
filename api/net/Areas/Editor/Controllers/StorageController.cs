using System.Net;
using System.Net.Mime;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Storage;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Helpers;
using TNO.Entities;
using TNO.Keycloak;
using TNO.Models.Extensions;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// StorageController class, provides Storage endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor)]
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
    private readonly IConnectionHelper _connection;
    private readonly StorageOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StorageController object, initializes with specified parameters.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="options"></param>
    public StorageController(IConnectionHelper connection, IOptions<StorageOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Fetch an array of files and directories at the specified path.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpGet("{locationId:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FolderModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult GetFolder([FromRoute] int? locationId, [FromQuery] string? path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var files = client.ListDirectory(Path.Combine(locationPath, path));
                    return new JsonResult(new FolderModel(files));
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                return new JsonResult(new FolderModel(_options.GetCapturePath(), path));
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            return new JsonResult(new FolderModel(_options.GetUploadPath(), path));
        }
    }

    /// <summary>
    /// Fetch an array of files and directories at the specified path.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("exists")]
    [HttpGet("{locationId:int}/exists")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult FolderExists([FromRoute] int? locationId, [FromQuery] string? path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    return client.Exists(Path.Combine(locationPath, path)) ? new OkResult() : new NoContentResult();
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                return Directory.Exists(Path.Combine(_options.GetCapturePath(), path)) ? new OkResult() : new NoContentResult();
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            return Directory.Exists(Path.Combine(_options.GetUploadPath(), path)) ? new OkResult() : new NoContentResult();
        }

    }

    /// <summary>
    /// Upload a file and link it to the specified content.
    /// Only a single file can be linked to content, each upload will overwrite.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <param name="files"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    [HttpPost("{locationId:int}/upload")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> Upload([FromRoute] int? locationId, [FromQuery] string path, [FromForm] List<IFormFile> files, bool overwrite = false)
    {
        if (files.Count == 0) throw new InvalidOperationException("File missing");
        var file = files.First();
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) client.CreateDirectory(safePath);

                    var fullPath = Path.Combine(safePath, file.FileName);
                    if (!overwrite && client.Exists(fullPath)) throw new InvalidOperationException("File already exists");

                    using var stream = System.IO.File.Open(safePath, FileMode.Create);
                    client.UploadFile(stream, fullPath);
                    return new JsonResult(new ItemModel(file.FileName, client.GetAttributes(fullPath)));
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_options.GetCapturePath(), path, file.FileName);
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
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_options.GetUploadPath(), path, file.FileName);
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
    }

    /// <summary>
    /// Stream the file for the specified path.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [AllowAnonymous] // TODO: Temporary to test HTML 5 video
    [HttpGet("stream")]
    [HttpGet("{locationId:int}/stream")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Stream([FromRoute] int? locationId, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) throw new InvalidOperationException($"Stream does not exist: '{path}'");

                    var info = new ItemModel(Path.GetFileName(safePath), client.GetAttributes(safePath));
                    var tmpPath = _connection.CopyFile(client, safePath, _options.GetCapturePath(), this.User);
                    var stream = System.IO.File.OpenRead(tmpPath);
                    return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: false);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_options.GetCapturePath(), path);
                if (!safePath.FileExists()) throw new InvalidOperationException($"Stream does not exist: '{path}'");

                var info = new ItemModel(safePath);
                var stream = System.IO.File.OpenRead(safePath);
                return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: true);
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_options.GetUploadPath(), path);
            if (!safePath.FileExists()) throw new InvalidOperationException($"Stream does not exist: '{path}'");

            var info = new ItemModel(safePath);
            var stream = System.IO.File.OpenRead(safePath);
            return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: true);
        }
    }

    /// <summary>
    /// Download the file for the specified path.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("download")]
    [HttpGet("{locationId:int}/download")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Download([FromRoute] int? locationId, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

                    var info = new ItemModel(Path.GetFileName(safePath), client.GetAttributes(safePath));
                    var tmpPath = _connection.CopyFile(client, safePath, _options.GetCapturePath(), this.User);
                    var stream = System.IO.File.OpenRead(tmpPath);
                    return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: false);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_options.GetCapturePath(), path);
                if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

                // TODO: download a full folder as a ZIP
                var info = new ItemModel(safePath);
                var stream = System.IO.File.OpenRead(safePath);
                return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: false);
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_options.GetUploadPath(), path);
            if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

            // TODO: download a full folder as a ZIP
            var info = new ItemModel(safePath);
            var stream = System.IO.File.OpenRead(safePath);
            return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: false);
        }
    }

    /// <summary>
    /// Moves a file from specified 'path' to 'destination'.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    [HttpPut("move")]
    [HttpPut("{locationId:int}/move")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Move([FromRoute] int? locationId, [FromQuery] string path, [FromQuery] string destination)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) throw new InvalidOperationException($"File does not exist: '{path}'");

                    var safeDestPath = Path.Combine(locationPath, destination.MakeRelativePath());
                    if (client.Exists(safeDestPath)) throw new InvalidOperationException($"File already exists, cannot rename: '{destination}'");

                    var directory = Path.GetDirectoryName(safeDestPath);
                    if (!client.Exists(directory)) client.CreateDirectory(directory);

                    client.RenameFile(safePath, safeDestPath);
                    var info = new ItemModel(Path.GetFileName(safeDestPath), client.GetAttributes(safeDestPath));
                    return new JsonResult(info);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var rootPath = _options.GetCapturePath();
                var safePath = Path.Combine(rootPath, path.MakeRelativePath());
                if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File does not exist: '{path}'");

                var safeDestination = Path.Combine(rootPath, destination.MakeRelativePath());
                if (safeDestination.FileExists()) throw new InvalidOperationException($"File already exists, cannot rename: '{destination}'");

                var directory = Path.GetDirectoryName(safeDestination);
                if (directory?.DirectoryExists() == false)
                    Directory.CreateDirectory(directory);

                System.IO.File.Move(safePath, safeDestination);
                var info = new ItemModel(safeDestination);
                return new JsonResult(info);
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var rootPath = _options.GetUploadPath();
            var safePath = Path.Combine(rootPath, path.MakeRelativePath());
            if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File does not exist: '{path}'");

            var safeDestination = Path.Combine(rootPath, destination.MakeRelativePath());
            if (safeDestination.FileExists()) throw new InvalidOperationException($"File already exists, cannot rename: '{destination}'");

            var directory = Path.GetDirectoryName(safeDestination);
            if (directory?.DirectoryExists() == false)
                Directory.CreateDirectory(directory);

            System.IO.File.Move(safePath, safeDestination);
            var info = new ItemModel(safeDestination);
            return new JsonResult(info);
        }
    }

    /// <summary>
    /// Delete file at path.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpDelete]
    [HttpDelete("{locationId:int}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Delete([FromRoute] int? locationId, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) throw new InvalidOperationException($"File does not exist: '{path}'");

                    var info = new ItemModel(Path.GetFileName(safePath), client.GetAttributes(safePath));
                    client.Delete(safePath);
                    return new JsonResult(info);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_options.GetCapturePath(), path.MakeRelativePath());
                if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

                // TODO: Only certain users should be allowed to delete certain files/folders.
                var item = new ItemModel(safePath);
                if (item.IsDirectory) Directory.Delete(safePath);
                else System.IO.File.Delete(safePath);
                return new JsonResult(item);
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_options.GetUploadPath(), path.MakeRelativePath());
            if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

            // TODO: Only certain users should be allowed to delete certain files/folders.
            var item = new ItemModel(safePath);
            if (item.IsDirectory) Directory.Delete(safePath);
            else System.IO.File.Delete(safePath);
            return new JsonResult(item);
        }
    }

    /// <summary>
    /// Make a clip from the target file identified in the clip parameter 'prefix'.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="outputName"></param>
    /// <returns></returns>
    [HttpPost("clip")]
    [HttpPost("{locationId:int}/clip")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> CreateClipAsync([FromRoute] int? locationId, [FromQuery] string path, [FromQuery] int start, [FromQuery] int end, [FromQuery] string outputName)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var capturePath = Path.Combine(locationPath, path);
                    if (!client.Exists(capturePath)) throw new InvalidOperationException($"File does not exist: '{path}'");

                    // Horrible implementation to download the whole capture file just to create a clip.
                    var tmpPath = _connection.CopyFile(client, capturePath, _options.GetCapturePath(), this.User);
                    var resultPath = await FfmpegHelper.CreateClipAsync(tmpPath, start, end, outputName);

                    // Upload clip to remote location.
                    using var clipStream = System.IO.File.OpenRead(resultPath);
                    var clipPath = Path.Combine(capturePath.GetDirectoryPath(), Path.GetFileName(resultPath));
                    client.UploadFile(clipStream, clipPath);
                    clipStream.Close();

                    var info = new ItemModel(Path.GetFileName(clipPath), client.GetAttributes(clipPath));

                    // Delete temporary files.
                    System.IO.File.Delete(tmpPath);
                    System.IO.File.Delete(resultPath);

                    return new JsonResult(info);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_options.GetCapturePath(), path.MakeRelativePath());
                var file = await FfmpegHelper.CreateClipAsync(safePath, start, end, outputName);
                return new JsonResult(new ItemModel(file));
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_options.GetUploadPath(), path.MakeRelativePath());
            var file = await FfmpegHelper.CreateClipAsync(safePath, start, end, outputName);
            return new JsonResult(new ItemModel(file));
        }
    }

    /// <summary>
    /// Make a clip from the target file identified in the clip parameter 'prefix'.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    [HttpPost("join")]
    [HttpPost("{locationId:int}/join")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ItemModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> JoinClipsAsync([FromRoute] int? locationId, [FromQuery] string path, [FromQuery] string prefix)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).GetDirectoryPath().MakeRelativePath();

        if (locationId != null)
        {
            // TODO: Handle multiple storage locations.
            var connection = _connection.GetConnection(locationId.Value);
            if (connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(connection);
                var locationPath = configuration.GetConfigurationValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) throw new InvalidOperationException($"Folder does not exist: '{path}'");

                    // Horrible implementation to copy all the clips temporarily to join them.
                    var tmpPath = Path.Combine(_options.GetCapturePath(), "_tmp", this.User.GetUsername() ?? "");
                    var files = client.ListDirectory(safePath).Where(f => f.Name.StartsWith(prefix));
                    foreach (var file in files)
                    {
                        _connection.CopyFile(client, file.FullName, _options.GetCapturePath(), this.User);
                    }

                    var resultPath = await FfmpegHelper.JoinClipsAsync(tmpPath, prefix);

                    // Upload clip to remote location.
                    using var joinStream = System.IO.File.OpenRead(resultPath);
                    var joinPath = Path.Combine(safePath, Path.GetFileName(resultPath));
                    client.UploadFile(joinStream, joinPath);
                    joinStream.Close();

                    var info = new ItemModel(Path.GetFileName(resultPath), client.GetAttributes(joinPath));

                    // Delete temporary files.
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(Path.Combine(tmpPath, file.Name));
                    }
                    System.IO.File.Delete(resultPath);

                    return new JsonResult(info);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (connection == null || connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_options.GetCapturePath(), path.MakeRelativePath());
                var file = await FfmpegHelper.JoinClipsAsync(safePath, prefix);
                return new JsonResult(new ItemModel(file));
            }
            else throw new NotImplementedException($"Location connection type '{connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_options.GetUploadPath(), path.MakeRelativePath());
            var file = await FfmpegHelper.JoinClipsAsync(safePath, prefix);
            return new JsonResult(new ItemModel(file));
        }
    }
    #endregion
}
