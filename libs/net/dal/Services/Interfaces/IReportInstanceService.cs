using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportInstanceService : IBaseService<ReportInstance, long>
{
    /// <summary>
    /// Find the instances of the report published within [start, end), newest first, with their content links.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IEnumerable<ReportInstance> FindInstancesInRange(int reportId, DateTime start, DateTime end);

    /// <summary>
    /// Find the non-private content published within [start, end) that has a topic or is in an instance of the report.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IEnumerable<Content> FindTopicContentInRange(int reportId, DateTime start, DateTime end);

    /// <summary>
    /// Replace the instance's output and content links, keeping its publication and delivery details.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="version">The version the output was generated from; a different stored version is rejected.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    ReportInstance SaveReportOutput(ReportInstance instance, long version, string subject, string body, IEnumerable<ReportInstanceContent> content);

    /// <summary>
    /// Create a report instance for a day within [start, end) that has none, with its generated output and content links.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    ReportInstance AddReportOutput(ReportInstance instance, DateTime start, DateTime end, IEnumerable<ReportInstanceContent> content);

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
    IEnumerable<ReportInstanceContent> GetContentForInstance(long id, IEnumerable<(long ContentId, string SectionName)> contentKeys);
    IEnumerable<ReportInstanceContent> GetContentForInstanceBasic(long id);

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

    UserReportInstance? GetUserReportInstance(long instanceId, int userId);
    IEnumerable<UserReportInstance> GetUserReportInstances(long instanceId);
    UserReportInstance Add(UserReportInstance entity);
    UserReportInstance AddAndSave(UserReportInstance entity);
    UserReportInstance Update(UserReportInstance entity);
    UserReportInstance UpdateAndSave(UserReportInstance entity);
    IEnumerable<UserReportInstance> UpdateAndSave(IEnumerable<UserReportInstance> entities);
    UserReportInstance Delete(UserReportInstance entity);
    UserReportInstance DeleteAndSave(UserReportInstance entity);
}
