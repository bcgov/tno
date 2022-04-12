using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace TNO.DAL.Services;

public class ActionService : BaseService<Action, int>, IActionService
{
    #region Properties
    #endregion

    #region Constructors
    public ActionService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ActionService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.Action> FindAll()
    {
        return this.Context.Actions.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
