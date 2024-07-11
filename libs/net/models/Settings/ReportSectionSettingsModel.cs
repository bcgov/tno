using System.Text.Json;
using TNO.Core.Extensions;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSectionSettingsModel
{
    #region Properties
    public string Label { get; set; } = "";
    public bool UseAllContent { get; set; }
    public bool ShowHeadlines { get; set; }
    public bool ShowFullStory { get; set; }
    public bool ShowImage { get; set; }
    public string Direction { get; set; } = "";
    public bool RemoveDuplicates { get; set; }
    public bool OverrideExcludeHistorical { get; set; }
    public bool HideEmpty { get; set; }
    public string GroupBy { get; set; } = "";
    public string SortBy { get; set; } = "";
    public string SortDirection { get; set; } = "";
    // public bool ApplyColorToValue { get; set; }
    // public string[] DatasetColors { get; set; } = Array.Empty<string>();
    // public string[] DatasetBorderColors { get; set; } = Array.Empty<string>();
    // public string[] DataLabelColors { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    public ReportSectionSettingsModel() { }

    public ReportSectionSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetDictionaryJsonValue("label", "", options)!;
        this.UseAllContent = settings.GetDictionaryJsonValue("useAllContent", false, options);
        this.ShowHeadlines = settings.GetDictionaryJsonValue("showHeadlines", false, options);
        this.ShowFullStory = settings.GetDictionaryJsonValue("showFullStory", false, options);
        this.ShowImage = settings.GetDictionaryJsonValue("showImage", false, options);
        this.Direction = settings.GetDictionaryJsonValue("direction", "", options)!;
        this.RemoveDuplicates = settings.GetDictionaryJsonValue("removeDuplicates", false, options)!;
        this.OverrideExcludeHistorical = settings.GetDictionaryJsonValue("overrideExcludeHistorical", false, options)!;
        this.HideEmpty = settings.GetDictionaryJsonValue("hideEmpty", false, options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", "", options)!;
        this.SortBy = settings.GetDictionaryJsonValue("sortBy", "", options)!;
        this.SortDirection = settings.GetDictionaryJsonValue("sortDirection", "", options)!;
        // this.DatasetColors = settings.GetDictionaryJsonValue("datasetColors", Array.Empty<string>(), options)!;
        // this.DataLabelColors = settings.GetDictionaryJsonValue("dataLabelColors", Array.Empty<string>(), options)!;
    }

    public ReportSectionSettingsModel(JsonDocument settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetElementValue("label", "", options)!;
        this.UseAllContent = settings.GetElementValue("useAllContent", false, options);
        this.ShowHeadlines = settings.GetElementValue("showHeadlines", false, options);
        this.ShowFullStory = settings.GetElementValue("showFullStory", false, options);
        this.ShowImage = settings.GetElementValue("showImage", false, options);
        this.Direction = settings.GetElementValue("direction", "", options)!;
        this.RemoveDuplicates = settings.GetElementValue("removeDuplicates", false, options)!;
        this.OverrideExcludeHistorical = settings.GetElementValue("overrideExcludeHistorical", false, options)!;
        this.HideEmpty = settings.GetElementValue("hideEmpty", false, options)!;
        this.GroupBy = settings.GetElementValue("groupBy", "", options)!;
        this.SortBy = settings.GetElementValue("sortBy", "", options)!;
        this.SortDirection = settings.GetElementValue("sortDirection", "", options)!;
        // this.DatasetColors = settings.GetElementValue("datasetColors", Array.Empty<string>(), options)!;
        // this.DataLabelColors = settings.GetElementValue("dataLabelColors", Array.Empty<string>(), options)!;
    }
    #endregion
}
