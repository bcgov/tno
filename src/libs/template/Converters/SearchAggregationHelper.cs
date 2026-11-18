namespace TNO.TemplateEngine.Converters;

/// <summary>
/// Extension methods to convert between Elastic models and Report models
/// </summary>
public static class SearchAggregationHelper
{
    /// <summary> 
    /// Convert from one model to another
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static TNO.TemplateEngine.Models.Reports.AggregationRootModel Convert(this TNO.Elastic.Models.AggregationRootModel model)
    {
        var aggregationRootModel = new TNO.TemplateEngine.Models.Reports.AggregationRootModel
        {
            Name = model.Name,
            DocCount = model.DocCount
        };
        if (model.ChildAggregation != null) aggregationRootModel.ChildAggregation = model.ChildAggregation.Convert();

        return aggregationRootModel;
    }

    /// <summary>
    /// Convert from one model to another
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static TNO.TemplateEngine.Models.Reports.AggregationModel Convert(this TNO.Elastic.Models.AggregationModel model)
    {
        var aggregationModel = new TNO.TemplateEngine.Models.Reports.AggregationModel
        {
            Name = model.Name,
            DocCountErrorUpperBound = model.DocCountErrorUpperBound,
            SumOtherDocCount = model.SumOtherDocCount
        };
        if (model.Buckets.Any())
        {
            aggregationModel.Buckets = model.Buckets.Select(b => b.Convert());
        }
        return aggregationModel;
    }

    /// <summary>
    /// Convert from one model to another
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static TNO.TemplateEngine.Models.Reports.AggregationBucketModel Convert(this TNO.Elastic.Models.AggregationBucketModel model)
    {
        var aggregationBucketModel = new TNO.TemplateEngine.Models.Reports.AggregationBucketModel
        {
            Key = model.Key,
            DocCount = model.DocCount,
        };
        if (model.AggregationSum != null)
        {
            aggregationBucketModel.AggregationSum = model.AggregationSum.Convert();
        }
        if (model.ChildAggregation != null)
        {
            aggregationBucketModel.ChildAggregation = model.ChildAggregation.Convert();
        }
        return aggregationBucketModel;
    }
    /// <summary>
    /// Convert from one model to another
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static TNO.TemplateEngine.Models.Reports.AggregationSumModel Convert(this TNO.Elastic.Models.AggregationSumModel model)
    {
        var aggregationSumModel = new TNO.TemplateEngine.Models.Reports.AggregationSumModel
        {
            Value = model.Value
        };
        return aggregationSumModel;
    }
}
