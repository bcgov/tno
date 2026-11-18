using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;


public class MinisterService : BaseService<Minister, int>, IMinisterService
{

    #region Constructors
    public MinisterService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Minister> FindAll()
    {
        return this.Context.Ministers
            .AsNoTracking()
            .Include(m => m.Organization)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public override Minister? FindById(int id)
    {
        return this.Context.Ministers
            .Include(m => m.Organization)
            .FirstOrDefault(m => m.Id == id);
    }
    #endregion

}
