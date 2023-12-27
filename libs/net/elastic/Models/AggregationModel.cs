using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TNO.Elastic.Models.Converters;

namespace TNO.Elastic.Models;

public class AggregationModel {
    #region Properties
    public string? Name { get; set; }
    public long? DocCountErrorUpperBound { get; set; }
    public long? SumOtherDocCount { get; set; }
    public IEnumerable<AggregationBucketModel> Buckets { get; set; } = Array.Empty<AggregationBucketModel>();
    #endregion
}
