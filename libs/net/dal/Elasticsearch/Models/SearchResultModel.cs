using System.Text.Json.Serialization;

namespace TNO.DAL.Elasticsearch.Models;

public class SearchResultModel<T>
    where T : class
{
    #region Properties
    [JsonPropertyName("took")]
    public int Took { get; set; }

    [JsonPropertyName("time_out")]
    public bool TimeOut { get; set; }

    [JsonPropertyName("_shards")]
    public ShardsModel? Shards { get; set; }

    [JsonPropertyName("hits")]
    public SearchResultHitsModel<T> Hits { get; set; } = new SearchResultHitsModel<T>();
    #endregion
}
