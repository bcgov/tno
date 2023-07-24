using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Elastic;
using TNO.Entities;

namespace TNO.DAL.Services;

public class AVOverviewSectionItemService : BaseService<AVOverviewSectionItem, int>, IAVOverviewSectionItemService
{
    #region Variables
    private readonly ITNOElasticClient _client;
    #endregion

    #region Constructors
    public AVOverviewSectionItemService(
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
    public IEnumerable<AVOverviewSectionItem> FindAll()
    {
        return this.Context.AVOverviewSectionItems
            .AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    public IEnumerable<AVOverviewSectionItem> FindBySectionId(int sectionId)
    {
        return this.Context.AVOverviewSectionItems
            .AsNoTracking()
            .Where(r => r.AVOverviewSectionId == sectionId)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }
    #endregion
}
