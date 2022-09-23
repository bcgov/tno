using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IIngestTypeService : IBaseService<IngestType, int>
{
    IEnumerable<IngestType> FindAll();
    IPaged<IngestType> Find(IngestTypeFilter filter);
    IngestType? FindByName(string name);
}
