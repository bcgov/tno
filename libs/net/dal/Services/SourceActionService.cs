using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class SourceActionService : BaseService<SourceAction, int>, ISourceActionService
{
    #region Properties
    #endregion

    #region Constructors
    public SourceActionService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SourceActionService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<SourceAction> FindAll()
    {
        return this.Context.SourceActions.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
