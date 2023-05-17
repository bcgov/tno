using System.Text.Json;
using Nest;
using TNO.Elastic.Models;

namespace TNO.Elastic;

public interface ITNOElasticClient : IElasticClient
{
    Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonDocument query)
        where T : class;
}
