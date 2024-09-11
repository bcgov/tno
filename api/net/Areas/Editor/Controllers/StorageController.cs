using System.Net;
using System.Net.Mime;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Storage;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Helpers;
using TNO.DAL.Services;
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
    private readonly StorageOptions _storageOptions;
    private readonly ApiOptions _apiOptions;
    private readonly IFileReferenceService _fileReferenceService;
    private readonly ILogger<StorageController> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StorageController object, initializes with specified parameters.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="storageOptions"></param>
    /// <param name="apiOptions"></param>
    /// <param name="fileReferenceService"></param>
    /// <param name="logger"></param>
    public StorageController(IConnectionHelper connection, IOptions<StorageOptions> storageOptions, IOptions<ApiOptions> apiOptions, IFileReferenceService fileReferenceService, ILogger<StorageController> logger)
    {
        _connection = connection;
        _storageOptions = storageOptions.Value;
        _apiOptions = apiOptions.Value;
        _fileReferenceService = fileReferenceService;
        _logger = logger;
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
    [ProducesResponseType(typeof(DirectoryModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult GetDirectory([FromRoute] int? locationId, [FromQuery] string? path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            // TODO: Handle multiple storage locations.
            if (dataLocation.Connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(dataLocation.Connection);
                var locationPath = configuration.GetDictionaryJsonValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();

                    if (!client.Exists(Path.Combine(locationPath, path))) return new NoContentResult();

                    var files = client.ListDirectory(Path.Combine(locationPath, path));
                    return new JsonResult(new DirectoryModel(files, _apiOptions.DataLocation == dataLocation?.Name));
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                return new JsonResult(new DirectoryModel(_storageOptions.GetCapturePath(), path, true));
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            return new JsonResult(new DirectoryModel(_storageOptions.GetUploadPath(), path, true));
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult DirectoryExists([FromRoute] int? locationId, [FromQuery] string? path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            // TODO: Handle multiple storage locations.
            if (dataLocation.Connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(dataLocation.Connection);
                var locationPath = configuration.GetDictionaryJsonValue<string>("path") ?? "";
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
            else if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                return Directory.Exists(Path.Combine(_storageOptions.GetCapturePath(), path)) ? new OkResult() : new NoContentResult();
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            return Directory.Exists(Path.Combine(_storageOptions.GetUploadPath(), path)) ? new OkResult() : new NoContentResult();
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
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_storageOptions.GetCapturePath(), path, file.FileName);
                if (String.IsNullOrWhiteSpace(Path.GetFileName(safePath))) throw new InvalidOperationException("Filename missing");
                if (safePath.DirectoryExists()) throw new InvalidOperationException("Invalid path");

                var directory = Path.GetDirectoryName(safePath);
                if (directory?.DirectoryExists() == false)
                    Directory.CreateDirectory(directory);

                if (!overwrite && safePath.FileExists()) throw new InvalidOperationException("File already exists");

                // TODO: Handle multiple files.
                using var stream = System.IO.File.Open(safePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return new JsonResult(new ItemModel(safePath, true));
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path, file.FileName);
            if (String.IsNullOrWhiteSpace(Path.GetFileName(safePath))) throw new InvalidOperationException("Filename missing");
            if (safePath.DirectoryExists()) throw new InvalidOperationException("Invalid path");

            var directory = Path.GetDirectoryName(safePath);
            if (directory?.DirectoryExists() == false)
                Directory.CreateDirectory(directory);

            if (!overwrite && safePath.FileExists()) throw new InvalidOperationException("File already exists");

            // TODO: Handle multiple files.
            using var stream = System.IO.File.Open(safePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return new JsonResult(new ItemModel(safePath, true));
        }
    }

    /// <summary>
    /// Stream the file for the specified path.
    /// </summary>
    /// <param name="locationId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpGet("stream")]
    [HttpGet("{locationId:int}/stream")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Stream([FromRoute] int? locationId, [FromQuery] string path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_storageOptions.GetCapturePath(), path);
                return GetResult(safePath, path);
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
            return GetResult(safePath, path);
        }
    }

    private FileStreamResult GetResult(string safePath, string path)
    {
        if (!safePath.FileExists()) throw new NoContentException($"Stream does not exist: '{path}'");

        var info = new ItemModel(safePath, true);
        var fileStream = System.IO.File.OpenRead(safePath);
        return File(fileStream, info.MimeType!);
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Download([FromRoute] int? locationId, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_storageOptions.GetCapturePath(), path);
                if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

                // TODO: download a full folder as a ZIP
                var info = new ItemModel(safePath, true);
                var stream = System.IO.File.OpenRead(safePath);
                return File(stream, contentType: info.MimeType!, fileDownloadName: info.Name, enableRangeProcessing: false);
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path);
            if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

            // TODO: download a full folder as a ZIP
            var info = new ItemModel(safePath, true);
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Move([FromRoute] int? locationId, [FromQuery] string path, [FromQuery] string destination)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var rootPath = _storageOptions.GetCapturePath();
                var safePath = Path.Combine(rootPath, path.MakeRelativePath());
                if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File does not exist: '{path}'");

                var safeDestination = Path.Combine(rootPath, destination.MakeRelativePath());
                if (safeDestination.FileExists()) throw new InvalidOperationException($"File already exists, cannot rename: '{destination}'");

                var directory = Path.GetDirectoryName(safeDestination);
                if (directory?.DirectoryExists() == false)
                    Directory.CreateDirectory(directory);

                System.IO.File.Move(safePath, safeDestination);
                var info = new ItemModel(safeDestination, true);
                return new JsonResult(info);
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var rootPath = _storageOptions.GetUploadPath();
            var safePath = Path.Combine(rootPath, path.MakeRelativePath());
            if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File does not exist: '{path}'");

            var safeDestination = Path.Combine(rootPath, destination.MakeRelativePath());
            if (safeDestination.FileExists()) throw new InvalidOperationException($"File already exists, cannot rename: '{destination}'");

            var directory = Path.GetDirectoryName(safeDestination);
            if (directory?.DirectoryExists() == false)
                Directory.CreateDirectory(directory);

            System.IO.File.Move(safePath, safeDestination);
            var info = new ItemModel(safeDestination, true);
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public IActionResult Delete([FromRoute] int? locationId, [FromQuery] string path)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            // TODO: Handle multiple storage locations.
            if (dataLocation.Connection?.ConnectionType == ConnectionType.SSH)
            {
                var configuration = _connection.GetConfiguration(dataLocation.Connection);
                var locationPath = configuration.GetDictionaryJsonValue<string>("path") ?? "";
                using var client = _connection.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var safePath = Path.Combine(locationPath, path);
                    if (!client.Exists(safePath)) throw new InvalidOperationException($"File does not exist: '{path}'");

                    var info = new ItemModel(Path.GetFileName(safePath), client.GetAttributes(safePath), _apiOptions.DataLocation == dataLocation?.Name);
                    client.Delete(safePath);
                    return new JsonResult(info);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_storageOptions.GetCapturePath(), path.MakeRelativePath());
                if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

                // TODO: Only certain users should be allowed to delete certain files/folders.
                var item = new ItemModel(safePath, true);
                if (item.IsDirectory) Directory.Delete(safePath);
                else System.IO.File.Delete(safePath);
                return new JsonResult(item);
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path.MakeRelativePath());
            if (!safePath.FileExists() && !safePath.DirectoryExists()) throw new InvalidOperationException($"File/folder does not exist: '{path}'");

            // TODO: Only certain users should be allowed to delete certain files/folders.
            var item = new ItemModel(safePath, true);
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> CreateClipAsync([FromRoute] int? locationId, [FromQuery] string path, [FromQuery] int start, [FromQuery] int end, [FromQuery] string outputName)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_storageOptions.GetCapturePath(), path.MakeRelativePath());
                var file = await FfmpegHelper.CreateClipAsync(safePath, start, end, outputName);
                return new JsonResult(new ItemModel(file, true));
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path.MakeRelativePath());
            var file = await FfmpegHelper.CreateClipAsync(safePath, start, end, outputName);
            return new JsonResult(new ItemModel(file, true));
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> JoinClipsAsync([FromRoute] int? locationId, [FromQuery] string path, [FromQuery] string prefix)
    {
        path = String.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).GetDirectoryPath().MakeRelativePath();
        var dataLocation = locationId.HasValue ? _connection.GetDataLocation(locationId.Value) : null;
        if (dataLocation != null)
        {
            if (dataLocation.Connection == null || dataLocation.Connection?.ConnectionType == ConnectionType.LocalVolume)
            {
                var safePath = Path.Combine(_storageOptions.GetCapturePath(), path.MakeRelativePath());
                var file = await FfmpegHelper.JoinClipsAsync(safePath, prefix);
                return new JsonResult(new ItemModel(file, true));
            }
            else throw new NotImplementedException($"Location connection type '{dataLocation.Connection?.ConnectionType}' not implemented yet.");
        }
        else
        {
            var safePath = Path.Combine(_storageOptions.GetUploadPath(), path.MakeRelativePath());
            var file = await FfmpegHelper.JoinClipsAsync(safePath, prefix);
            return new JsonResult(new ItemModel(file, true));
        }
    }
    /// <summary>
    /// upload files to s3
    /// </summary>
    /// <param name="updatedBefore">optional, only upload files updated before the specified date</param>
    /// <param name="limit">optional, only upload limit files</param>
    /// <returns>uploaded files and failed uploads</returns>
    [HttpPost("upload-files-to-s3")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Dictionary<string, List<string>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Storage" })]
    public async Task<IActionResult> UploadFilesToS3([FromQuery] DateTime? updatedBefore = null, [FromQuery] int? limit = null)
    {
        _logger.LogInformation("upload-files-to-s3");
        var fileReferences = await _fileReferenceService.GetFiles(updatedBefore, limit ?? 100);
        var uploadedFiles = new List<string>();
        var failedUploads = new List<string>();
        // check if s3 credentials are set
        var accessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
        var secretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");
        var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
        var serviceUrl = Environment.GetEnvironmentVariable("S3_SERVICE_URL");
        var hasS3Credentials = !string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(bucketName) && !string.IsNullOrEmpty(serviceUrl);

        if (!hasS3Credentials)
        {
            _logger.LogError("S3 credentials are not set");
            return BadRequest("S3 credentials are not set");
        }

        foreach (var fileReference in fileReferences)
        {
            try
            {
                var filePath = Path.Combine(_storageOptions.GetUploadPath(), fileReference.Path);

                if (System.IO.File.Exists(filePath))
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    // use relative path as S3 key
                    var s3Key = fileReference.Path.Replace("\\", "/"); // make sure use forward slash as path separator
                    _logger.LogInformation($"uploading: {s3Key}");
                    await _fileReferenceService.UploadToS3Async(s3Key, fileStream);
                    uploadedFiles.Add(s3Key);
                }
                else
                {
                    failedUploads.Add($"{fileReference.Path} (file not found)");
                }
            }
            catch (Exception ex)
            {
                failedUploads.Add($"{fileReference.Path} (error: {ex.Message})");
            }
        }

        _logger.LogInformation("finished upload-all-to-s3");
        return Ok(new
        {
            UploadedFiles = uploadedFiles,
            FailedUploads = failedUploads,
        });
    }
    #endregion
}
