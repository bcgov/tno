using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class DataLocationService : BaseService<DataLocation, int>, IDataLocationService
{
    #region Properties
    #endregion

    #region Constructors
    public DataLocationService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<DataLocationService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<DataLocation> FindAll()
    {
        return this.Context.DataLocations
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public DataLocation? FindByName(string name)
    {
        return this.Context.DataLocations
            .Include(d => d.Connection)
            .FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
    }
    #endregion
}
