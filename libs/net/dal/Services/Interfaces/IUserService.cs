
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IUserService : IBaseService<User, int>
{
    IEnumerable<User> FindAll();

    IPaged<User> Find(UserFilter filter);

    User? FindByUserKey(string key);

    User? UpdatePreferences(User user);

    User? FindByUsername(string username);
    IEnumerable<User> FindByEmail(string email);
    IEnumerable<User> FindByRoles(IEnumerable<string> roles);
    User? TransferAccount(API.Areas.Admin.Models.User.TransferAccountModel account);
}
