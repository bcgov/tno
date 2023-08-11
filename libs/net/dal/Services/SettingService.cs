using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace TNO.DAL.Services;

public class SettingService : BaseService<Entities.Setting, int>, ISettingService
{
    #region Properties
    #endregion

    #region Constructors
    public SettingService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SettingService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.Setting> FindAll()
    {
        return this.Context.Settings
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public Entities.Setting? FindByName(string name)
    {
        return this.Context.Settings
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }
    #endregion
}
