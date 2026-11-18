
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ITimeTrackingService : IBaseService<TimeTracking, object[]>
{
    IEnumerable<TimeTracking> Find(DateTime from, DateTime to);
    IEnumerable<CBRAReportTotalExcerpts> GetTotalExcerpts(DateTime from, DateTime to);
    IEnumerable<CBRAReportStaffSummary> GetStaffSummary(DateTime from, DateTime to);
    IEnumerable<CBRAReportTotalsByProgram> GetTotalsByProgram(DateTime from, DateTime to);
    IEnumerable<CBRAReportTotalsByBroadcaster> GetTotalsByBroadcaster(DateTime from, DateTime to);
    IEnumerable<CBRAReportTotalEntries> GetTotalEntries(DateTime from, DateTime to);
}
