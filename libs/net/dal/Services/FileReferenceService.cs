using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Config;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;

public class FileReferenceService : BaseService<FileReference, long>, IFileReferenceService
{
    #region Properties
    private readonly StorageConfig _storageConfig;
    #endregion

    #region Constructors
    public FileReferenceService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        StorageConfig storageConfig,
        ILogger<FileReferenceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _storageConfig = storageConfig;
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
    /// <returns></returns>
    public FileStream Download(FileReference entity)
    {
        // TODO: Don't allow users to manually set paths to a location.
        var filePath = $"{entity.GetFilePath(this.Context, _storageConfig)}{entity.Path}";
        var stream = System.IO.File.OpenRead(filePath);

        return stream;
    }

    /// <summary>
    /// Upload the file to the configured data location and add or update the specified file reference.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<FileReference> Upload(ContentFileReference model)
    {
        // TODO: Handle different data locations.
        var path = model.GetFilePath(this.Context, _storageConfig);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (model.File?.Length > 0)
        {
            // TODO: Don't allow user to overwrite the generated filename as this will result in unexpected results.
            var filePath = $"{path}{model.Path}";
            using var stream = System.IO.File.Open(filePath, FileMode.Create);
            await model.File.CopyToAsync(stream);
            model.IsUploaded = true;
        }

        if (model.Id == 0)
        {
            this.Context.Add((FileReference)model);
        }
        else
        {
            this.Context.Update((FileReference)model);
        }

        this.Context.CommitTransaction();
        return (FileReference)model;
    }

    /// <summary>
    /// Delete the specified file reference and the file from the configured data location.
    /// </summary>
    /// <param name="entity"></param>
    public override void Delete(FileReference entity)
    {
        // TODO: Handle different data locations.
        var filePath = $"{entity.GetFilePath(this.Context, _storageConfig)}{entity.Path}";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        base.Delete(entity);
    }
    #endregion
}
