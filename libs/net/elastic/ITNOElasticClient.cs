using System.Text.Json;
using Nest;
using TNO.Elastic.Models;

namespace TNO.Elastic;

public interface ITNOElasticClient : IElasticClient
{
    Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonDocument query)
        where T : class;

    Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonElement query)
        where T : class;

    Task<ValidateResultModel> ValidateAsync(string index, JsonDocument query);
}
