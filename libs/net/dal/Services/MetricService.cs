using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class MetricService : BaseService<Metric, int>, IMetricService
{
    #region Properties
    #endregion

    #region Constructors
    public MetricService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<MetricService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Metric> FindAll()
    {
        return this.Context.Metrics.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
