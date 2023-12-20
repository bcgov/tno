using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class AggregationBucketModel
{
    #region Properties
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("doc_count")]
    public int Count { get; set; }
    #endregion
}
