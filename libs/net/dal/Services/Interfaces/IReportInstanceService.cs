using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportInstanceService : IBaseService<ReportInstance, long>
{
    /// <summary>
    /// Find all report instances for the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    IEnumerable<ReportInstance> FindInstancesForReportId(int reportId, int? ownerId);
}
