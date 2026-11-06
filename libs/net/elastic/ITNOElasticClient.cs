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

    /// <summary>
    /// Make a request to Elasticsearch and return the unparsed response.
    /// Aggregations that are not a single fixed shape cannot be read through SearchResultModel.
    /// </summary>
    Task<JsonDocument> SearchRawAsync(string index, JsonDocument query);

    Task<ValidateResultModel> ValidateAsync(string index, JsonDocument query);
}
