using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class EventScheduleService : BaseService<EventSchedule, int>, IEventScheduleService
{
    #region Properties
    #endregion

    #region Constructors
    public EventScheduleService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ScheduleService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<EventSchedule> FindAll()
    {
        return this.Context.EventSchedules.ToArray();
    }

    public override EventSchedule? FindById(int id)
    {
        return this.Context.EventSchedules
            .Include(m => m.Schedule)
            .FirstOrDefault(m => m.Id == id);
    }
    #endregion
}
