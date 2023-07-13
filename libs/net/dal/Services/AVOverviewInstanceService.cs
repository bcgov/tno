using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Elastic;
using TNO.Entities;

namespace TNO.DAL.Services;

public class AVOverviewInstanceService : BaseService<AVOverviewInstance, int>, IAVOverviewInstanceService
{
    #region Variables
    private readonly ITNOElasticClient _client;
    #endregion

    #region Constructors
    public AVOverviewInstanceService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        ITNOElasticClient client,
        IServiceProvider serviceProvider,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _client = client;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the av overview sections.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AVOverviewInstance> FindAll()
    {
        return this.Context.AVOverviewInstances
            .AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }
    #endregion
}
