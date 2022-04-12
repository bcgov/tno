using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class UserService : BaseService<User, int>, IUserService
{
    #region Properties
    #endregion

    #region Constructors
    public UserService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<UserService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<User> FindAll()
    {
        return this.Context.Users.OrderBy(a => a.Username).ThenBy(a => a.LastName).ThenBy(a => a.FirstName).ToArray();
    }

    public User? FindByKey(Guid? key)
    {
        return this.Context.Users.Where(u => u.Key == key).FirstOrDefault();
    }

    public override User? FindById(int id)
    {
        return this.Context.Users
            .Include(u => u.RolesManyToMany).ThenInclude(u => u.Role)
            .FirstOrDefault(c => c.Id == id);
    }
    #endregion
}
