using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IReportService : IBaseService<Report, int>
{
    /// <summary>
    /// Find all reports.
    /// </summary>
    /// <param name="populateFullModel"></param>
    /// <returns></returns>
    IEnumerable<Report> FindAll(bool populateFullModel = true);

    /// <summary>
    /// Find all reports that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="populateFullModel"></param>
    /// <returns></returns>
    IEnumerable<Report> Find(ReportFilter filter, bool populateFullModel = true);

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// Makes a request for each section.
    /// If the section also references a folder it will make a request for the folder content too.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(Report report, int? requestorId);

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
}
