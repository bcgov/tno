
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IContentReferenceService : IBaseService<ContentReference, string[]>
{
    IPaged<ContentReference> Find(ContentReferenceFilter filter);
}
