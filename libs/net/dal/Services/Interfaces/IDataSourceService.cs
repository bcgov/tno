
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IDataSourceService : IBaseService<DataSource, int>
{
    IEnumerable<DataSource> FindAll(bool includeConnection = false);
    IPaged<DataSource> Find(DataSourceFilter filter);
    DataSource? FindByCode(string code);
    IEnumerable<DataSource> FindByMediaType(string mediaTypeName, bool includeConnection = false);
    DataSource Update(DataSource entity, bool updateChildren = false);
}
