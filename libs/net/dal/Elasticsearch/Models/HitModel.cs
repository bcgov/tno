using System.Text.Json.Serialization;

namespace TNO.DAL.Elasticsearch.Models;

public class HitModel<T>
    where T : class
{
    #region Properties
    [JsonPropertyName("_index")]
    public string Index { get; set; } = "";

    [JsonPropertyName("_type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("_id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("_score")]
    public float Score { get; set; }

    [JsonPropertyName("_source")]
    public T Source { get; set; } = default!;
    #endregion
}
