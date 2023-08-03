using System.Text.Json;
using TNO.Core.Extensions;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSectionSettingsModel
{
    #region Properties
    public string Label { get; set; } = "";
    public bool IsSummary { get; set; }
    public bool ShowContent { get; set; }
    public bool ShowCharts { get; set; }
    public bool ChartsOnTop { get; set; }
    public bool RemoveDuplicates { get; set; }
    public string SortBy { get; set; } = "";
    #endregion

    #region Constructors
    public ReportSectionSettingsModel() { }

    public ReportSectionSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetDictionaryJsonValue<string>("label", "", options)!;
        this.IsSummary = settings.GetDictionaryJsonValue<bool>("isSummary", false, options);
        this.ShowContent = settings.GetDictionaryJsonValue<bool>("showContent", false, options);
        this.ShowCharts = settings.GetDictionaryJsonValue<bool>("showCharts", false, options);
        this.ChartsOnTop = settings.GetDictionaryJsonValue<bool>("chartsOnTop", false, options);
        this.RemoveDuplicates = settings.GetDictionaryJsonValue<bool>("removeDuplicates", false, options)!;
        this.SortBy = settings.GetDictionaryJsonValue<string>("sortBy", "", options)!;
    }

    public ReportSectionSettingsModel(JsonDocument settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetElementValue<string>("label", "", options)!;
        this.IsSummary = settings.GetElementValue<bool>("isSummary", false, options);
        this.ShowContent = settings.GetElementValue<bool>("showContent", false, options);
        this.ShowCharts = settings.GetElementValue<bool>("showCharts", false, options);
        this.ChartsOnTop = settings.GetElementValue<bool>("chartsOnTop", false, options);
        this.RemoveDuplicates = settings.GetElementValue<bool>("removeDuplicates", false, options)!;
        this.SortBy = settings.GetElementValue<string>("sortBy", "", options)!;
    }
    #endregion
}
