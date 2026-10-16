
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IContentReferenceService : IBaseService<ContentReference, string[]>
{
    IPaged<ContentReference> Find(ContentReferenceFilter filter);
    long[] FindContentIds(string uid);
    Dictionary<(string Source, string Uid), (long Id, string Headline)> FindContents(IEnumerable<ContentReference> references);
}
