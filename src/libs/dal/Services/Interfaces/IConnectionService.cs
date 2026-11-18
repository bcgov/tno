
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IConnectionService : IBaseService<Connection, int>
{
    IEnumerable<Connection> FindAll();
}
