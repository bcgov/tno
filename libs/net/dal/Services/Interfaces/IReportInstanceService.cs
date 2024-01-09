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


    /// <summary>
    /// Get all the content items for the specified instance.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerable<ReportInstanceContent> GetContentForInstance(long id);

    /// <summary>
    /// Update only the instance and not the content.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="instanceOnly"></param>
    /// <returns></returns>
    ReportInstance UpdateAndSave(ReportInstance instance, bool instanceOnly = false);
}
