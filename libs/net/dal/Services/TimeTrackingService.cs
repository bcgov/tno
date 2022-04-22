using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TimeTrackingService : BaseService<TimeTracking, object[]>, ITimeTrackingService
{
    #region Properties
    #endregion

    #region Constructors
    public TimeTrackingService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TimeTrackingService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<TimeTracking> Find(DateTime from, DateTime to)
    {
        return this.Context.TimeTrackings
            .Include(tt => tt.User)
            .Where(tt => tt.CreatedOn >= from && tt.CreatedOn <= to);
    }
    #endregion
}
