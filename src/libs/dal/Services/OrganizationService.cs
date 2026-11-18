using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;


public class OrganizationService : BaseService<Organization, int>, IOrganizationService
{

    #region Constructors
    public OrganizationService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Organization> FindAll()
    {
        return this.Context.Organizations
            .AsNoTracking()
            .Include(o => o.Ministers)
            .Include(o => o.UsersManyToMany).ThenInclude(o => o.User)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public override Organization? FindById(int id)
    {
        return this.Context.Organizations
            .Include(m => m.Ministers)
            .Include(m => m.UsersManyToMany).ThenInclude(m => m.User)
            .FirstOrDefault(m => m.Id == id);
    }
    #endregion

}
