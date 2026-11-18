using System.Text.Json;
using TNO.Elastic;

namespace TNO.DAL.Services;

public class ElasticsearchService : IElasticsearchService
{
    #region Variables
    private readonly ITNOElasticClient _client;
    #endregion

    #region Constructors
    public ElasticsearchService(ITNOElasticClient client)
    {
        _client = client;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'query'.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindAsync(string index, JsonDocument query)
    {
        return await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(index, query);
    }
    #endregion
}
