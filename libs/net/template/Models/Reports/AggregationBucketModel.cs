namespace TNO.TemplateEngine.Models.Reports;

public class AggregationBucketModel
{
    #region Properties
    public string Key { get; set; } = "";
    public long DocCount { get; set; }
    public AggregationSumModel? AggregationSum { get; set;}
    public AggregationModel? ChildAggregation { get; set;}
    #endregion
}
