
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IDataSourceService : IBaseService<DataSource, int>
{
    IEnumerable<DataSource> FindAll();
    IPaged<DataSource> Find(DataSourceFilter filter);
}
