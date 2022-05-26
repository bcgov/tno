
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IDataServiceService : IBaseService<DataService, int>
{
    public DataService AddOrUpdate(DataService entity);
}
