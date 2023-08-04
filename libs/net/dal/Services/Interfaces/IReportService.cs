using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportService : IBaseService<Report, int>
{
    IEnumerable<Report> FindAll();

    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentWithElasticsearchAsync(string index, Report report);

    /// <summary>
    /// Get the content from the current report instance for the specified 'reportId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    IEnumerable<long> GetReportInstanceContentToExclude(int reportId);

    /// <summary>
    /// Get the content from the related report instances for the specified 'reportId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    IEnumerable<long> GetRelatedReportInstanceContentToExclude(int reportId);
}
