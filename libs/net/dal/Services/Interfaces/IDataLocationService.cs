
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IDataLocationService : IBaseService<DataLocation, int>
{
    IEnumerable<DataLocation> FindAll();
    DataLocation? FindByName(string name);
}
