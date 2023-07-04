using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ChartTemplateService : BaseService<ChartTemplate, int>, IChartTemplateService
{
    #region Variables
    #endregion

    #region Constructors
    public ChartTemplateService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<ChartTemplateService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the report templates.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ChartTemplate> FindAll()
    {
        return this.Context.ChartTemplates
            .AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }
    #endregion
}
