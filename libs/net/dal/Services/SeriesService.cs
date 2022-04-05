using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class SeriesService : BaseService<Series, int>, ISeriesService
{
    #region Properties
    #endregion

    #region Constructors
    public SeriesService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SeriesService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Series> FindAll()
    {
        return this.Context.Series.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
