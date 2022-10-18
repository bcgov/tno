using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TonePoolService : BaseService<TonePool, int>, ITonePoolService
{
    #region Properties
    #endregion

    #region Constructors
    public TonePoolService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TonePoolService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<TonePool> FindAll()
    {
        return this.Context.TonePools.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
