using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ChartTemplateSettingsModel
{
    #region Properties
    public IEnumerable<string> ChartTypes { get; set; } = Array.Empty<string>();
    public bool IsOverTime { get; set; }
    public IEnumerable<string> Graph { get; set; } = Array.Empty<string>();
    public IEnumerable<string> GroupBy { get; set; } = Array.Empty<string>();
    public IEnumerable<string> GroupBySection { get; set; } = Array.Empty<string>();
    public object Options { get; set; } = new();
    #endregion

    #region Constructors
    public ChartTemplateSettingsModel() { }

    public ChartTemplateSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ChartTypes = settings.GetDictionaryJsonValue<string[]>("chartTypes", Array.Empty<string>(), options)!;
        this.IsOverTime = settings.GetDictionaryJsonValue<bool>("isOverTime", false, options)!;
        this.Graph = settings.GetDictionaryJsonValue<string[]>("graph", Array.Empty<string>(), options)!;
        this.GroupBy = settings.GetDictionaryJsonValue<string[]>("groupBy", Array.Empty<string>(), options)!;
        this.GroupBySection = settings.GetDictionaryJsonValue<string[]>("groupBySection", Array.Empty<string>(), options)!;
        this.Options = settings.GetDictionaryJsonValue<object>("options", new { }, options)!;
    }
    #endregion
}
