using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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
    /// <summary>Prevent deleting a license while content references it. The database also enforces this against concurrent changes.</summary>
    public override void Delete(License entity)
    {
        if (this.Context.Contents.Any(c => c.LicenseId == entity.Id))
            throw new TNO.Core.Exceptions.BadRequestException("Cannot delete this license because it is used by content.");
        base.Delete(entity);
    }

    public IEnumerable<License> FindAll()
    {
        return this.Context.Licenses
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
