using System.Text.Json;
using TNO.Core.Extensions;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSectionSettingsModel
{
    #region Properties
    public string Label { get; set; } = "";
    public Entities.ReportSectionType SectionType { get; set; }
    public bool ShowHeadlines { get; set; }
    public bool ShowFullStory { get; set; }
    public bool ShowImage { get; set; }
    public bool ShowCharts { get; set; }
    public bool ChartsOnTop { get; set; }
    public bool RemoveDuplicates { get; set; }
    public bool HideEmpty { get; set; }
    public string GroupBy { get; set; } = "";
    public string SortBy { get; set; } = "";
    public bool ShowContent { get; set; }
    #endregion

    #region Constructors
    public ReportSectionSettingsModel() { }

    public ReportSectionSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetDictionaryJsonValue("label", "", options)!;
        this.SectionType = settings.GetDictionaryJsonValue("sectionType", Entities.ReportSectionType.Content, options);
        this.ShowHeadlines = settings.GetDictionaryJsonValue("showHeadlines", false, options);
        this.ShowFullStory = settings.GetDictionaryJsonValue("showFullStory", false, options);
        this.ShowImage = settings.GetDictionaryJsonValue("showImage", false, options);
        this.ShowCharts = settings.GetDictionaryJsonValue("showCharts", false, options);
        this.ChartsOnTop = settings.GetDictionaryJsonValue("chartsOnTop", false, options);
        this.RemoveDuplicates = settings.GetDictionaryJsonValue("removeDuplicates", false, options)!;
        this.HideEmpty = settings.GetDictionaryJsonValue("hideEmpty", false, options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", "", options)!;
        this.SortBy = settings.GetDictionaryJsonValue("sortBy", "", options)!;
        this.ShowContent = settings.GetDictionaryJsonValue("showContent", false, options);
    }

    public ReportSectionSettingsModel(JsonDocument settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetElementValue("label", "", options)!;
        this.SectionType = settings.GetElementValue("sectionType", Entities.ReportSectionType.Content, options);
        this.ShowHeadlines = settings.GetElementValue("showHeadlines", false, options);
        this.ShowFullStory = settings.GetElementValue("showFullStory", false, options);
        this.ShowImage = settings.GetElementValue("showImage", false, options);
        this.ShowCharts = settings.GetElementValue("showCharts", false, options);
        this.ChartsOnTop = settings.GetElementValue("chartsOnTop", false, options);
        this.RemoveDuplicates = settings.GetElementValue("removeDuplicates", false, options)!;
        this.HideEmpty = settings.GetElementValue("hideEmpty", false, options)!;
        this.GroupBy = settings.GetElementValue("groupBy", "", options)!;
        this.SortBy = settings.GetElementValue("sortBy", "", options)!;
        this.ShowContent = settings.GetElementValue("showContent", false, options);
    }
    #endregion
}
