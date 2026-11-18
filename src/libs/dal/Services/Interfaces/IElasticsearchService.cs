using System.Text.Json;

namespace TNO.DAL.Services;

public interface IElasticsearchService
{
    Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindAsync(string index, JsonDocument query);
}
