using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.Entities;
using TNO.Models.Filters;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.S3;

namespace TNO.DAL.Services;

public class FileReferenceService : BaseService<FileReference, long>, IFileReferenceService
{
    #region Properties
    private readonly StorageOptions _options;
    private readonly S3Options _s3Options;
    #endregion

    #region Constructors
    public FileReferenceService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        StorageOptions options,
        IOptions<S3Options> s3Options,
        ILogger<FileReferenceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _options = options;
        _s3Options = s3Options.Value;
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

    private AmazonS3Client? CreateS3Client()
    {
        if (!_s3Options.IsS3Enabled)
        {
            return null;
        }

        try
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(_s3Options.AccessKey, _s3Options.SecretKey),
                new AmazonS3Config
                {
                    ServiceURL = _s3Options.ServiceUrl,
                    ForcePathStyle = true
                });
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UploadToS3Async(string s3Key, Stream fileStream)
    {
        if (fileStream == null)
        {
            Logger.LogError("File stream is null for S3 key: {S3Key}", s3Key);
            return false;
        }
        if (!_s3Options.IsS3Enabled || await TestS3NetworkConnectionAsync() == false)
        {
            return false;
        }

        using var s3Client = CreateS3Client();
        if (s3Client == null)
        {
            return false;
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = _s3Options.BucketName,
            Key = s3Key,
            InputStream = fileStream,
        };

        try
        {
            var response = await s3Client.PutObjectAsync(putRequest);

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

    /// <summary>
    /// Get files from the database.
    /// If publishedAfter is specified, only files' contents that are published after the specified date will be returned.
    /// If publishedBefore is specified, only files' contents that are published before the specified date will be returned.
    /// If force is true, all files will be returned, otherwise only files that are not synced to S3 will be returned.
    /// If limit is -1, all files will be returned, otherwise only the specified number of files will be returned.
    /// </summary>
    /// <param name="publishedAfter"></param>
    /// <param name="publishedBefore"></param>
    /// <param name="limit"></param>
    /// <param name="force"></param>
    /// <returns></returns>
    public async Task<IEnumerable<FileReference>> GetFiles(DateTime? publishedAfter = null, DateTime? publishedBefore = null, int limit = 100, bool force = false)
    {
        try
        {
            IQueryable<FileReference> query = this.Context.FileReferences.Include(fr => fr.Content);

            if (publishedAfter.HasValue)
            {
                publishedAfter = publishedAfter.Value.ToUniversalTime();
                query = query.Where(fr => fr.Content != null && fr.Content.PublishedOn >= publishedAfter.Value);
            }

            if (publishedBefore.HasValue)
            {
                publishedBefore = publishedBefore.Value.ToUniversalTime();
                query = query.Where(fr => fr.Content != null && fr.Content.PublishedOn < publishedBefore.Value);
            }

            if (!force)
            {
                query = query.Where(fr => !fr.IsSyncedToS3);
            }

            query = query.OrderBy(fr => fr.CreatedOn);

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

    public async Task<Stream?> DownloadFromS3Async(string s3Key)
    {
        if (!_s3Options.IsS3Enabled || await TestS3NetworkConnectionAsync() == false)
        {
            return null;
        }

        using var s3Client = CreateS3Client();
        if (s3Client == null)
        {
            return null;
        }

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _s3Options.BucketName,
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
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<FileReference> UpdateAsync(FileReference entity)
    {
        this.Context.Update(entity);
        await this.Context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Test the network connection to S3.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TestS3NetworkConnectionAsync()
    {
        if (!_s3Options.IsS3Enabled)
        {
            return false;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var request = new HttpRequestMessage(HttpMethod.Get, _s3Options.ServiceUrl);
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            var content = await response.Content.ReadAsStringAsync();

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Network connection failed: {ex.Message}");
            return false;
        }
    }

}
#endregion