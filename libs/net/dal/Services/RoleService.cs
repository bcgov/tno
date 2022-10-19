using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class RoleService : BaseService<Role, int>, IRoleService
{
    #region Properties
    #endregion

    #region Constructors
    public RoleService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<RoleService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Role> FindAll()
    {
        return this.Context.Roles
            .AsNoTracking()
            .OrderBy(a => a.Name).ToArray();
    }
    #endregion
}
