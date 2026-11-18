using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TNO.DAL.Services;

public class CacheService : BaseService<Entities.Cache, string>, ICacheService
{
    #region Properties
    #endregion

    #region Constructors
    public CacheService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<CacheService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.Cache> FindAll()
    {
        return this.Context.Cache
            .AsNoTracking()
            .ToArray();
    }
    #endregion
}
