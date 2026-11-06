using System.Text.Json;
using TNO.TemplateEngine.Models.Charts;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.Test.AI;

/// <summary>
/// A chart with computed data must carry that data to the Charts API, so the template never runs.
/// </summary>
public class ChartDataEngineTest
{
    [Fact]
    public void ChartRequest_CarriesComputedData()
    {
        var data = JsonDocument.Parse("""{"labels":["a"],"datasets":[]}""");
        var model = new ChartEngineContentModel { ChartData = data };

        var request = new ChartRequestModel(model, model.ChartData);

        Assert.Same(data, request.ChartData);
        Assert.Empty(model.Content);
    }

    [Fact]
    public void Section_HoldsChartDataByTemplate()
    {
        var section = new ReportSectionModel();
        section.ChartData[3] = JsonDocument.Parse("{}");
        Assert.True(section.ChartData.ContainsKey(3));
        Assert.False(section.ChartData.ContainsKey(4));
    }

    [Fact]
    public void BucketTemplates_ReadTheSameHelpersAsSections()
    {
        var aggregations = new Dictionary<string, AggregationRootModel>
        {
            ["section"] = new()
            {
                ChildAggregation = new AggregationModel
                {
                    Buckets = [new AggregationBucketModel { Key = "Issues", AggregationSum = new AggregationSumModel { Value = 7 } }],
                },
            },
        };

        Assert.Equal(7, TNO.TemplateEngine.ReportExtensions.GetTotalScoresByTopicType(aggregations)["Issues"]);
    }

    [Fact]
    public void Colours_AreShared_WithTheTemplateHelpers()
    {
        var colors = new[] { "#1", "#2", "#3" };
        Assert.Equal(TNO.API.Models.Charts.ChartColors.GetColors(colors, 1, "source", "a", "count"), TNO.TemplateEngine.ReportExtensions.GetColors(colors, 1, "source", "a", "count"));
        Assert.Equal("""["#2"]""", TNO.TemplateEngine.ReportExtensions.GetColors(colors, 1, "source", "a", "count"));
    }
}
