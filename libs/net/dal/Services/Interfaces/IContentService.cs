
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IContentService : IBaseService<Content, long>
{
    IPaged<Content> FindWithDatabase(ContentFilter filter, bool asNoTracking = true);
    Task<IPaged<Content>> FindWithElasticsearchAsync(ContentFilter filter);
    Content? FindByUid(string uid, string? source);
}
