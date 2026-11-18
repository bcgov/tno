using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSectionChartTemplateSettingsModel
{
    #region Properties
    public string ChartTypes { get; set; } = "";
    public bool IsOverTime { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
    public string Graph { get; set; } = "";
    public string GroupBy { get; set; } = "";
    public string GroupBySection { get; set; } = "";
    public object Options { get; set; } = new();
    #endregion

    #region Constructors
    public ReportSectionChartTemplateSettingsModel() { }

    public ReportSectionChartTemplateSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ChartTypes = settings.GetDictionaryJsonValue<string>("chartTypes", "", options)!;
        this.IsOverTime = settings.GetDictionaryJsonValue<bool>("isOverTime", false, options)!;
        this.Height = settings.GetDictionaryJsonValue<int?>("height", 200, options)!;
        this.Width = settings.GetDictionaryJsonValue<int?>("width", 200, options)!;
        this.Graph = settings.GetDictionaryJsonValue<string>("graph", "", options)!;
        this.GroupBy = settings.GetDictionaryJsonValue<string>("groupBy", "", options)!;
        this.GroupBySection = settings.GetDictionaryJsonValue<string>("groupBySection", "", options)!;
        this.Options = settings.GetDictionaryJsonValue<object>("options", new { }, options)!;
    }
    #endregion
}
