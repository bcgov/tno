
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface ISourceService : IBaseService<Source, int>
{
    IEnumerable<Source> FindAll();
    IPaged<Source> Find(SourceFilter filter);
    Source? FindByCode(string code);
    Source Update(Source entity, bool updateChildren = false);
}
