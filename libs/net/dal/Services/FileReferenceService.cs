using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.Entities;
using TNO.Models.Filters;
using Amazon.S3;
using Amazon.S3.Model;

namespace TNO.DAL.Services;

public class FileReferenceService : BaseService<FileReference, long>, IFileReferenceService
{
    #region Properties
    private readonly StorageOptions _options;
    #endregion

    #region Constructors
    public FileReferenceService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        StorageOptions options,
        ILogger<FileReferenceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _options = options;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all file references for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public IEnumerable<FileReference> FindByContentId(long contentId)
    {
        return this.Context.FileReferences
            .Include(fr => fr.Content)
            .Where(fr => fr.ContentId == contentId).ToArray();
    }

    /// <summary>
    /// Open the file for reading and return the stream.
    /// Note - this does not close the stream.  You need to do this.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public FileStream Download(FileReference entity, string folderPath)
    {
        return File.OpenRead(Path.Combine(folderPath, entity.Path.MakeRelativePath()));
    }

    /// <summary>
    /// Delete the original file and upload the new file for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public async Task<FileReference> UploadAsync(Content content, IFormFile file, string folderPath)
    {
        if (!content.FileReferences.Any()) throw new InvalidOperationException("Content does not have a file to replace");

        var original = content.FileReferences.First();
        var path = Path.Combine(folderPath, original.Path.MakeRelativePath());

        // Delete the original file if it exists.
        if (File.Exists(path))
            File.Delete(path);

        var fileReference = new ContentFileReference(original, file);
        var result = await UploadAsync(fileReference, folderPath);

        return result;
    }

    /// <summary>
    /// Upload the file to the configured data location and add or update the specified file reference.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public async Task<FileReference> UploadAsync(ContentFileReference model, string folderPath)
    {
        // TODO: Handle different data locations.
        var path = Path.Combine(folderPath, model.Path.MakeRelativePath());
        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory) && !String.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        if (model.File?.Length > 0)
        {
            using var stream = File.Open(path, FileMode.Create);
            await model.File.CopyToAsync(stream);
            model.IsUploaded = true;
        }

        var entity = (FileReference)model;
        if (model.Id == 0)
            this.Context.Add(entity);
        else
            this.Context.Update(entity);

        this.Context.CommitTransaction();
        return entity;
    }

    /// <summary>
    /// Delete the original file and upload the new file for the content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="file"></param>
    /// <param name="folderPath"></param>
    /// <param name="deleteOriginal"></param>
    /// <returns></returns>
    public FileReference Attach(Content content, FileInfo file, string folderPath, bool deleteOriginal = true)
    {
        if (!content.FileReferences.Any()) throw new InvalidOperationException("Content does not have a file to replace");

        var original = content.FileReferences.First();
        var path = Path.Combine(folderPath, original.Path.MakeRelativePath());

        // Delete the original file if it exists.
        if (deleteOriginal && File.Exists(path))
            File.Delete(path);

        var fileReference = new ContentFileReference(original, file);
        var result = Attach(fileReference, folderPath);

        return result;
    }

    /// <summary>
    /// Copy the file to the configured data location and add or update the specified file reference.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public FileReference Attach(ContentFileReference model, string folderPath)
    {
        // TODO: Handle different data locations.
        var path = Path.Combine(folderPath, model.Path.MakeRelativePath());
        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory) && !String.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var file = new FileInfo(model.SourceFile);
        file.CopyTo(path, true);

        var entity = (FileReference)model;
        if (model.Id == 0)
            this.Context.Add(entity);
        else
            this.Context.Update(entity);

        this.Context.CommitTransaction();
        return entity;
    }

    /// <summary>
    /// Delete the specified file reference and the file from the configured data location.
    /// </summary>
    /// <param name="entity"></param>
    public override void DeleteAndSave(FileReference entity)
    {
        var path = Path.Combine(_options.GetUploadPath(), entity.Path.MakeRelativePath());
        if (File.Exists(path))
            File.Delete(path);

        base.DeleteAndSave(entity);
    }

    public async Task<bool> UploadToS3Async(string s3Key, Stream fileStream)
    {
        if (fileStream == null)
        {
            Logger.LogError("File stream is null for S3 key: {S3Key}", s3Key);
            return false;
        }

        var accessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? throw new InvalidOperationException("S3_ACCESS_KEY environment variable is not set");
        var secretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? throw new InvalidOperationException("S3_SECRET_KEY environment variable is not set");
        var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? throw new InvalidOperationException("S3_BUCKET_NAME environment variable is not set");
        var serviceUrl = Environment.GetEnvironmentVariable("S3_SERVICE_URL") ?? throw new InvalidOperationException("S3_SERVICE_URL environment variable is not set");

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        using var s3Client = new AmazonS3Client(accessKey, secretKey, config);

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = s3Key,
            InputStream = fileStream,
        };

        try
        {
            var response = await s3Client.PutObjectAsync(putRequest);

            // Check if the request was successful
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.LogInformation("File uploaded to S3 successfully: {S3Key}", s3Key);
                return true;
            }
            else
            {
                Logger.LogError("Failed to upload file to S3: {S3Key}", s3Key);
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while uploading file to S3: {S3Key}", s3Key);
            return false;
        }
    }

    public async Task<IEnumerable<FileReference>> GetFiles(DateTime? updatedBefore = null, int limit = 100, bool force = false)
    {
        try
        {
            IQueryable<FileReference> query = this.Context.FileReferences;

            if (updatedBefore.HasValue)
            {
                updatedBefore = updatedBefore.Value.ToUniversalTime();
                query = query.Where(fr => fr.UpdatedOn < updatedBefore.Value);
            }

            if (!force)
            {
                query = query.Where(fr => !fr.IsSyncedToS3);
            }

            query = query.OrderBy(fr => fr.UpdatedOn);

            // if limit is not -1, apply the limit, means get all
            if (limit != -1)
            {
                query = query.Take(limit);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting files");
            return Enumerable.Empty<FileReference>();
        }
    }

    public async Task<Stream> DownloadFromS3Async(string s3Key)
    {
        var accessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? throw new InvalidOperationException("S3_ACCESS_KEY environment variable is not set");
        var secretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? throw new InvalidOperationException("S3_SECRET_KEY environment variable is not set");
        var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? throw new InvalidOperationException("S3_BUCKET_NAME environment variable is not set");
        var serviceUrl = Environment.GetEnvironmentVariable("S3_SERVICE_URL") ?? throw new InvalidOperationException("S3_SERVICE_URL environment variable is not set");

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        using var s3Client = new AmazonS3Client(accessKey, secretKey, config);

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key
            };

            var response = await s3Client.GetObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            else
            {
                Logger.LogError("Failed to download file from S3: {S3Key}", s3Key);
                throw new Exception($"Failed to download file from S3: {s3Key}");
            }
        }
        catch (AmazonS3Exception ex)
        {
            Logger.LogError(ex, "Error retrieving file from S3: {S3Key}", s3Key);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error when downloading file from S3: {S3Key}", s3Key);
            throw;
        }
    }

    public async Task<FileReference?> GetByS3PathAsync(string s3Path)
    {
        return await this.Context.FileReferences
            .FirstOrDefaultAsync(fr => fr.S3Path == s3Path);
    }

    public async Task<FileReference?> GetByPathAsync(string path)
    {
        return await this.Context.FileReferences
            .FirstOrDefaultAsync(fr => fr.Path == path);
    }


    public async Task<(Stream? Stream, string? FileName, string? ContentType)> GetFileStreamAsync(string path)
    {
        var fileReference = await GetByS3PathAsync(path);
        if (fileReference == null)
        {
            fileReference = await GetByPathAsync(path);
        }
        if (fileReference == null)
        {
            Logger.LogInformation("File reference not found for path: {Path}", path);
            return (null, null, null);
        }

        if (fileReference.IsSyncedToS3)
        {
            try
            {
                var stream = await DownloadFromS3Async(path);
                return (stream, fileReference.FileName, fileReference.ContentType);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error on stream file from S3: {Path}", path);
            }
        }

        var safePath = Path.Combine(_options.GetUploadPath(), path);
        if (!File.Exists(safePath))
        {
            Logger.LogInformation("File does not exist: {Path}", safePath);
            return (null, null, null);
        }

        var fileStream = File.OpenRead(safePath);
        return (fileStream, fileReference.FileName, fileReference.ContentType);
    }

    public async Task<FileReference> UpdateAsync(FileReference entity)
    {
        this.Context.Update(entity);
        await this.Context.SaveChangesAsync();
        return entity;
    }

    public async Task<int> DeleteOldLocalFilesAsync(DateTime beforeDate)
    {
        Logger.LogInformation("Starting DeleteOldLocalFilesAsync with beforeDate: {BeforeDate}", beforeDate);

        var filesToDelete = await this.Context.FileReferences
            .Where(fr => fr.CreatedOn < beforeDate.ToUniversalTime()
                         && fr.IsSyncedToS3
                         && (fr.ContentType.StartsWith("audio/") || fr.ContentType.StartsWith("video/"))
                         && !string.IsNullOrEmpty(fr.Path))
            .ToListAsync();

        Logger.LogInformation("Found {Count} files to delete", filesToDelete.Count);

        int deletedCount = 0;

        foreach (var file in filesToDelete)
        {
            var localPath = Path.Combine(_options.GetUploadPath(), file.Path.MakeRelativePath());
            Logger.LogDebug("Checking file: {Path}", localPath);

            if (File.Exists(localPath))
            {
                try
                {
                    File.Delete(localPath);
                    deletedCount++;
                    Logger.LogInformation("Deleted local file: {Path}", localPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error deleting local file: {Path}", localPath);
                }
            }
            else
            {
                Logger.LogWarning("File not found: {Path}", localPath);
            }
        }

        Logger.LogInformation("DeleteOldLocalFilesAsync completed. Deleted {Count} files", deletedCount);
        return deletedCount;
    }
}
#endregion