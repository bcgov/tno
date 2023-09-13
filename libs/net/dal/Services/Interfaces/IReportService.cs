using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportService : IBaseService<Report, int>
{
    /// <summary>
    /// Find all reports.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Report> FindAll();

    /// <summary>
    /// Find all public reports.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Report> GetPublic();

    /// <summary>
    /// Find all reports that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<Report> Find(ReportFilter filter);

    /// <summary>
    /// Find content for the specified report with Elasticsearch.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(string index, Report report);

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
    /// Find all my reports.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Report> FindMyReports(int userId);

    /// <summary>
    /// Clears all content from all folders in any section of the specified 'report'.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    Report? ClearFoldersInReport(Report report);

}
