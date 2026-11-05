using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ChartTemplateSettingsModel
{
    #region Properties
    public IEnumerable<string> ChartTypes { get; set; } = Array.Empty<string>();
    public IEnumerable<string> GroupBy { get; set; } = Array.Empty<string>();
    public IEnumerable<string> Dataset { get; set; } = Array.Empty<string>();
    public IEnumerable<string> DatasetValue { get; set; } = Array.Empty<string>();
    public JsonDocument Options { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Whether the chart data comes from the built-in Elasticsearch aggregations, instead of the Razor template.
    /// </summary>
    public bool Aggregate { get; set; }

    /// <summary>
    /// get/set - An Elasticsearch 'aggs' block. When set, the Razor template renders from the
    /// aggregation buckets (ChartAggregations) and receives no content.
    /// </summary>
    public JsonDocument? Aggregations { get; set; }
    #endregion

    #region Constructors
    public ChartTemplateSettingsModel() { }

    public ChartTemplateSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ChartTypes = settings.GetDictionaryJsonValue("chartTypes", Array.Empty<string>(), options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", Array.Empty<string>(), options)!;
        this.Dataset = settings.GetDictionaryJsonValue("dataset", Array.Empty<string>(), options)!;
        this.DatasetValue = settings.GetDictionaryJsonValue("datasetValue", Array.Empty<string>(), options)!;
        this.Options = settings.GetDictionaryJsonValue("options", JsonDocument.Parse("{}"), options)!;
        this.Aggregate = settings.GetDictionaryJsonValue("aggregate", false, options);
        this.Aggregations = settings.GetDictionaryJsonValue<JsonDocument?>("aggregations", null, options);
    }
    #endregion
}
