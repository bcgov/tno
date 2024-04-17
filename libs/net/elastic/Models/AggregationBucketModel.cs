using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class AggregationBucketModel
{
    #region Properties
    public string Key { get; set; } = "";

    public long DocCount { get; set; } = 0;

    public AggregationSumModel? AggregationSum { get; set;}

    public AggregationModel? ChildAggregation { get; set;}
    #endregion
}
