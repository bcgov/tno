using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class LicenseService : BaseService<License, int>, ILicenseService
{
    #region Properties
    #endregion

    #region Constructors
    public LicenseService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<LicenseService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<License> FindAll()
    {
        return this.Context.Licenses.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
