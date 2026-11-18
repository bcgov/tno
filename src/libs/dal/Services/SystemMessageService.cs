using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class SystemMessageService : BaseService<SystemMessage, int>, ISystemMessageService
{

    #region Constructors
    public SystemMessageService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<SystemMessage> FindAll()
    {
        return this.Context.SystemMessages.ToArray();
    }
    #endregion

}
