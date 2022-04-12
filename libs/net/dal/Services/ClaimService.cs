using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace TNO.DAL.Services;

public class ClaimService : BaseService<Entities.Claim, int>, IClaimService
{
    #region Properties
    #endregion

    #region Constructors
    public ClaimService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ClaimService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.Claim> FindAll()
    {
        return this.Context.Claims.OrderBy(a => a.Name).ToArray();
    }
    #endregion
}
