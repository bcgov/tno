using TNO.Entities;

namespace TNO.DAL.Services;

public interface IReportService : IBaseService<Report, int>
{
    IEnumerable<Report> FindAll();
    Task<IEnumerable<API.Areas.Services.Models.Content.ContentModel>> FindContentWithElasticsearchAsync(Report report);
}
