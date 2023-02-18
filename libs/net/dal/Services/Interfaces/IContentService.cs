
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IContentService : IBaseService<Content, long>
{
    IPaged<Content> Find(ContentFilter filter, bool asNoTracking = true);
    Task<IPaged<Content>> FindAsync(ContentFilter filter);
    Content? FindByUid(string uid, string? source);
}
