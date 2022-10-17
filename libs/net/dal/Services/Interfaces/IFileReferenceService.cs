
using Microsoft.AspNetCore.Http;
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IFileReferenceService : IBaseService<FileReference, long>
{
    IEnumerable<FileReference> FindByContentId(long contentId);

    Task<FileReference> UploadAsync(ContentFileReference model);

    Task<FileReference> UploadAsync(Content content, IFormFile file);

    FileStream Download(FileReference entity);

    FileReference Attach(ContentFileReference model);

    FileReference Attach(Content content, FileInfo file);
}
