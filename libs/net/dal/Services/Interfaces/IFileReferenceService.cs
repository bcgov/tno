
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IFileReferenceService : IBaseService<FileReference, long>
{
    IEnumerable<FileReference> FindByContentId(long contentId);

    Task<FileReference> Upload(ContentFileReference model);

    FileStream Download(FileReference entity);

    Task<FileReference> Attach(ContentFileReference model);
}
