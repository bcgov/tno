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

    public async Task<IEnumerable<FileReference>> GetFiles(DateTime? updatedBefore = null, int limit = 100)
    {
        try
        {
            IQueryable<FileReference> query = this.Context.FileReferences;

            if (updatedBefore.HasValue)
            {
                updatedBefore = updatedBefore.Value.ToUniversalTime();
                query = query.Where(fr => fr.UpdatedOn < updatedBefore.Value);
            }

            // order by updated on descending
            query = query.OrderByDescending(fr => fr.UpdatedOn);

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
    #endregion
}
