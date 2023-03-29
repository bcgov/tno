using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ReportService : BaseService<Report, int>, IReportService
{
    #region Properties
    #endregion

    #region Constructors
    public ReportService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Report> FindAll()
    {
        return this.Context.Reports
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
