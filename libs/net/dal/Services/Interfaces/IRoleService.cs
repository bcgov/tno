
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IRoleService : IBaseService<Role, int>
{
    IEnumerable<Role> FindAll();
}
