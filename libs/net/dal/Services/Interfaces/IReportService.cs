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
    IEnumerable<Report> FindPublic();

    /// <summary>
    /// Find all reports that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<Report> Find(ReportFilter filter);

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'report'.
    /// Makes a request for each section.
    /// If the section also references a folder it will make a request for the folder content too.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="index">Override the index that will be used for search.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(Report report, string? index = null);

    /// <summary>
    /// Get the current instance for the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    ReportInstance? GetLatestInstance(int id, int? ownerId = null, bool? isActive = null);

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
