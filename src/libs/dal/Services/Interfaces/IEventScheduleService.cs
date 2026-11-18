
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IEventScheduleService : IBaseService<EventSchedule, int>
{
    IEnumerable<EventSchedule> FindAll();
}
