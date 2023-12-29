namespace TNO.TemplateEngine.Models.Reports;

public class AggregationModel {
    #region Properties
    public string? Name { get; set; }
    public long? DocCountErrorUpperBound { get; set; }
    public long? SumOtherDocCount { get; set; }
    public IEnumerable<AggregationBucketModel> Buckets { get; set; } = Array.Empty<AggregationBucketModel>();
    #endregion
}
