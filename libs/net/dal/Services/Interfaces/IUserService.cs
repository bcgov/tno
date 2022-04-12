
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IUserService : IBaseService<User, int>
{
    IEnumerable<User> FindAll();

    User? FindByKey(Guid? key);
}
