using System.Text.Json;
using TNO.Elastic.Models;

namespace TNO.Elastic;

public interface ITNOElasticClient
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

    /// <summary>
    /// Make any request to Elasticsearch and return the unparsed response. An unsuccessful status
    /// is returned, not thrown; check ElasticResponse.Success.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path relative to the cluster URL, with any query string (e.g. "content/_search", "_pit?keep_alive=5m").</param>
    /// <param name="body">A JSON body.</param>
    /// <param name="opaqueId">An X-Opaque-Id header, which Elasticsearch copies onto any task the request starts.</param>
    /// <param name="cancellationToken"></param>
    Task<ElasticResponse> RequestAsync(HttpMethod method, string path, string? body = null, string? opaqueId = null, CancellationToken cancellationToken = default);
}
