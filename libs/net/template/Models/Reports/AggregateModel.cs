namespace TNO.TemplateEngine.Models.Reports;

public class AggregateModel {
    #region Properties
    public long? DocCount { get; set; }

    public long? OtherDocCount { get; set; }

    public bool ShowAggregateCountsAsPercentOfTotal {get; set; } = true;

    public Dictionary<string, int> Buckets { get; set; } = new Dictionary<string, int>();
    #endregion
}
