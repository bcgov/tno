using Microsoft.AspNetCore.Http;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IFileReferenceService : IBaseService<FileReference, long>
{
    IEnumerable<FileReference> FindByContentId(long contentId);

    Task<FileReference> UploadAsync(ContentFileReference model, string folderPath);

    Task<FileReference> UploadAsync(Content content, IFormFile file, string folderPath);

    FileStream Download(FileReference entity, string folderPath);

    FileReference Attach(ContentFileReference model, string folderPath);

    FileReference Attach(Content content, FileInfo file, string folderPath, bool deleteOriginal = true);

    Task<IEnumerable<FileReference>> GetFiles(DateTime? updatedBefore = null, int limit = 3);

    Task<bool> UploadToS3Async(string s3Key, Stream fileStream);
}
