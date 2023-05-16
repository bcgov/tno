using System.Text.Json;
using Nest;
using TNO.DAL.Elasticsearch.Models;

namespace TNO.DAL.Elasticsearch;

public interface ITnoElasticClient : IElasticClient
{
    Task<SearchResultModel<T>?> SearchAsync<T>(string index, JsonDocument query)
        where T : class;
}
