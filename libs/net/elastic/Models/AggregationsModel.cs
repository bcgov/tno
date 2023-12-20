using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TNO.Elastic.Models.Converters;

namespace TNO.Elastic.Models;

[JsonConverter(typeof(AggregationsModelConverter))]
public class AggregationsModel {
    #region Properties
    [JsonPropertyName("doc_count")]
    public long? DocCount { get; set; }

    [JsonPropertyName("sum_other_doc_count")]
    public long? OtherDocCount { get; set; }

    [JsonPropertyName("buckets")]
    public IEnumerable<AggregationBucketModel>? Buckets { get; set; }
    #endregion
}
