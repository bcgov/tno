using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IReportService : IBaseService<Report, int>
{
    /// <summary>
    /// Find all reports that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="populateFullModel"></param>
    /// <returns></returns>
    IEnumerable<Report> Find(ReportFilter filter, bool populateFullModel = true);

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// It will generate content for each section.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="instanceId"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(Report report, long? instanceId, int? requestorId);

    /// <summary>
    /// Find content with Elasticsearch for the specified `reportInstance` and `section`.
    /// </summary>
    /// <param name="reportInstance"></param>
    /// <param name="section"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(ReportInstance reportInstance, ReportSection section, int? requestorId);

    /// <summary>
    /// Get reports based on the filter for the dashboard.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<Report> GetDashboard(DashboardFilter filter);

    /// <summary>
    /// Get the specified report for the dashboard.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Report GetDashboardReport(int id);

    /// <summary>
    /// Generate an instance of the report.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestorId"></param>
    /// <param name="instanceId"></param>
    /// <param name="regenerate"></param>
    /// <returns></returns>
    public Task<ReportInstance> GenerateReportInstanceAsync(
        int id,
        int? requestorId = null,
        long? instanceId = null,
        bool regenerate = false);

    /// <summary>
    /// Regenerate the content for the current report instance for the specified report 'id' and 'sectionId'.
    /// This provides a way to only refresh a single section within a report.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sectionId"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    Task<ReportInstance> RegenerateReportInstanceSectionAsync(int id, int sectionId, int? requestorId = null);

    /// <summary>
    /// Add the specified 'content' to the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<Report?> AddContentToReportAsync(int id, int? ownerId, IEnumerable<ReportInstanceContent> content);

    /// <summary>
    /// Get the current instance for the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId">The owner of the instance.</param>
    /// <param name="limit">Number of instances to return.</param>
    /// <returns></returns>
    IEnumerable<ReportInstance> GetLatestInstances(int id, int? ownerId = null, int limit = 2);

    /// <summary>
    /// Find the last report instance created for the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="includeContent"></param>
    /// <returns></returns>
    ReportInstance? GetCurrentReportInstance(int reportId, int? ownerId = null, bool includeContent = false);

    /// <summary>
    /// Find the previous report instance created for the specified report 'id' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="instanceId"></param>
    /// <param name="ownerId"></param>
    /// <param name="includeContent"></param>
    /// <returns></returns>
    ReportInstance? GetPreviousReportInstance(int id, long? instanceId, int? ownerId = null, bool includeContent = false);

    /// <summary>
    /// Get the content from the current report instance for the specified 'reportId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    IEnumerable<long> GetReportInstanceContentToExclude(int reportId, int? ownerId);

    /// <summary>
    /// Get the content from the related report instances for the specified 'reportId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    IEnumerable<long> GetRelatedReportInstanceContentToExclude(int reportId);

    /// <summary>
    /// Clears all content from all folders in any section of the specified 'report'.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    Report? ClearFoldersInReport(Report report);

    /// <summary>
    /// Subscribe the specified user to the specified report
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    Task<int> Subscribe(int userId, int reportId);

    /// <summary>
    /// Unsubscribe the specified user from the specified report
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    Task<int> Unsubscribe(int userId, int reportId);

    /// <summary>
    /// Unsubscribe all reports for the specified user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> Unsubscribe(int userId);

    /// <summary>
    /// Get all content for each report belonging to the specified 'userId'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Dictionary<int, long[]> GetAllContentInMyReports(int userId);
}
