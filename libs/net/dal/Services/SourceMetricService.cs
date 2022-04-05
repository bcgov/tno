using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class SourceMetricService : BaseService<SourceMetric, int>, ISourceMetricService
{
    #region Properties
    #endregion

    #region Constructors
    public SourceMetricService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SourceMetricService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<SourceMetric> FindAll()
    {
        return this.Context.SourceMetrics.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
