using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IIngestTypeService : IBaseService<IngestType, int>
{
    IEnumerable<IngestType> FindAll();
    IPaged<IngestType> Find(IngestTypeFilter filter);
    IngestType? FindByName(string name);
}
