using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

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
    public HitsModel<T> Hits { get; set; } = new HitsModel<T>();
    #endregion
}
