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
    public bool? ConvertToBase64Image { get; set; }
    public bool? CacheData { get; set; }
    public string Direction { get; set; } = "";
    public bool RemoveDuplicates { get; set; }
    public bool OverrideExcludeHistorical { get; set; }
    public bool? InTableOfContents { get; set; }
    public bool HideEmpty { get; set; }
    public string GroupBy { get; set; } = "";
    public string SortBy { get; set; } = "";
    public string SortDirection { get; set; } = "";
    public string Url { get; set; } = "";
    public string? UrlCache { get; set; }
    public bool Preload { get; set; }
    public string? DataType { get; set; }
    public string? DataProperty { get; set; }
    public string? DataTemplate { get; set; }
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
        this.ConvertToBase64Image = settings.GetDictionaryJsonValue("convertToBase64Image", false, options);
        this.CacheData = settings.GetDictionaryJsonValue("cacheData", false, options);
        this.Direction = settings.GetDictionaryJsonValue("direction", "", options)!;
        this.RemoveDuplicates = settings.GetDictionaryJsonValue("removeDuplicates", false, options)!;
        this.OverrideExcludeHistorical = settings.GetDictionaryJsonValue("overrideExcludeHistorical", false, options)!;
        this.InTableOfContents = settings.GetDictionaryJsonValue<bool?>("inTableOfContents", null, options)!;
        this.HideEmpty = settings.GetDictionaryJsonValue("hideEmpty", false, options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", "", options)!;
        this.SortBy = settings.GetDictionaryJsonValue("sortBy", "", options)!;
        this.SortDirection = settings.GetDictionaryJsonValue("sortDirection", "", options)!;
        this.Url = settings.GetDictionaryJsonValue("url", "", options)!;
        this.UrlCache = settings.GetDictionaryJsonValue<string?>("urlCache", null, options)!;
        this.Preload = settings.GetDictionaryJsonValue("preload", false, options)!;
        this.DataType = settings.GetDictionaryJsonValue<string?>("dataType", null, options)!;
        this.DataProperty = settings.GetDictionaryJsonValue<string?>("dataProperty", null, options)!;
        this.DataTemplate = settings.GetDictionaryJsonValue<string?>("dataTemplate", null, options)!;
    }

    public ReportSectionSettingsModel(JsonDocument settings, JsonSerializerOptions options)
    {
        this.Label = settings.GetElementValue("label", "", options)!;
        this.UseAllContent = settings.GetElementValue("useAllContent", false, options);
        this.ShowHeadlines = settings.GetElementValue("showHeadlines", false, options);
        this.ShowFullStory = settings.GetElementValue("showFullStory", false, options);
        this.ShowImage = settings.GetElementValue("showImage", false, options);
        this.ConvertToBase64Image = settings.GetElementValue("convertToBase64Image", false, options);
        this.CacheData = settings.GetElementValue("cacheData", false, options);
        this.Direction = settings.GetElementValue("direction", "", options)!;
        this.RemoveDuplicates = settings.GetElementValue("removeDuplicates", false, options)!;
        this.OverrideExcludeHistorical = settings.GetElementValue("overrideExcludeHistorical", false, options)!;
        this.InTableOfContents = settings.GetElementValue<bool?>("inTableOfContents", null, options)!;
        this.HideEmpty = settings.GetElementValue("hideEmpty", false, options)!;
        this.GroupBy = settings.GetElementValue("groupBy", "", options)!;
        this.SortBy = settings.GetElementValue("sortBy", "", options)!;
        this.SortDirection = settings.GetElementValue("sortDirection", "", options)!;
        this.Url = settings.GetElementValue("url", "", options)!;
        this.UrlCache = settings.GetElementValue<string?>("urlCache", null, options)!;
        this.Preload = settings.GetElementValue("preload", false, options)!;
        this.DataType = settings.GetElementValue<string?>("dataType", null, options)!;
        this.DataProperty = settings.GetElementValue<string?>("dataProperty", null, options)!;
        this.DataTemplate = settings.GetElementValue<string?>("dataTemplate", null, options)!;
    }
    #endregion
}
