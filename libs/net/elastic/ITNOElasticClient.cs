using System.Text.Json;
using Nest;
using TNO.Elastic.Models;

namespace TNO.Elastic;

public interface ITNOElasticClient : IElasticClient
{
    Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonDocument query, string sortBy = "publishedOn:desc")
        where T : class;

    Task<SearchResultModel<T>> SearchAsync<T>(string index, JsonElement query, string sortBy = "publishedOn:desc")
        where T : class;
}
