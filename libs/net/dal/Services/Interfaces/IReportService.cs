using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportService : IBaseService<Report, int>
{
    IEnumerable<Report> FindAll();
    Report? FindById(int id, bool includeInstances);
    Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindContentWithElasticsearchAsync(string index, Report report);
}
