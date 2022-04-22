
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ITimeTrackingService : IBaseService<TimeTracking, object[]>
{
    IEnumerable<TimeTracking> Find(DateTime from, DateTime to);
}
