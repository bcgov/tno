using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportInstanceService : IBaseService<ReportInstance, long>
{
    /// <summary>
    /// Find all report instances for the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    IEnumerable<ReportInstance> FindInstancesForReportId(int reportId, int? ownerId, int skip = 0, int take = 10);

    /// <summary>
    /// Find report instances for the specified 'reportId' and 'date'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    ReportInstance? FindInstanceForReportIdAndDate(long reportId, DateTime date);

    /// <summary>
    /// Get all the content items for the specified instance.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerable<ReportInstanceContent> GetContentForInstance(long id);

    /// <summary>
    /// Update the report instance in the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    ReportInstance Update(ReportInstance entity, bool updateChildren = false);

    /// <summary>
    /// Update the report instance in the context, but do not save to the database yet.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    ReportInstance UpdateAndSave(ReportInstance entity, bool updateChildren = false);
    
}
