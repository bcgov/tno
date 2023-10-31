
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IContentReferenceService : IBaseService<ContentReference, string[]>
{
    IPaged<ContentReference> Find(ContentReferenceFilter filter);
    long[] FindContentIds(string uid);
}
