using System.Text.Json;
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
    public bool RemoveDuplicateTitles3Days { get; set; }
    public bool OverrideExcludeHistorical { get; set; }
    public bool? InTableOfContents { get; set; }
    public int? IncludePreviousReports { get; set; }
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
    public int? LLMId { get; set; }
    public string? SystemPrompt { get; set; }
    public string? UserPrompt { get; set; }
    public int? ChoiceIndex { get; set; }
    public int? ChoiceQty { get; set; }
    public float? Temperature { get; set; }
    public bool ShowErrorDetails { get; set; }

    /// <summary>
    /// get/set - Which content feeds an AI section: 'Report' (every content section, the behaviour
    /// of sections saved before scopes existed) or 'Sections' (the SourceSections).
    /// </summary>
    public string? AIScope { get; set; }

    /// <summary>
    /// get/set - The content sections (by name) that feed an AI section scoped to sections.
    /// </summary>
    public string[] SourceSections { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - What an AI section produces: 'FreeText' (default) or 'TopicSummary' (headings
    /// with bullet statements and source lists).
    /// </summary>
    public string? AIOutputMode { get; set; }

    /// <summary>
    /// get/set - Story fields sent to AI synthesis. Null preserves the default fields; an empty
    /// selection is invalid. Applies to both current stories and previous report context.
    /// </summary>
    public string[]? AIInputFields { get; set; }
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
        this.RemoveDuplicateTitles3Days = settings.GetDictionaryJsonValue("removeDuplicateTitles3Days", false, options)!;
        this.OverrideExcludeHistorical = settings.GetDictionaryJsonValue("overrideExcludeHistorical", false, options)!;
        this.InTableOfContents = settings.GetDictionaryJsonValue<bool?>("inTableOfContents", null, options)!;
        this.IncludePreviousReports = settings.GetDictionaryJsonValue<int?>("includePreviousReports", null, options)!;
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
        this.LLMId = settings.GetDictionaryJsonValue<int?>("llmId", null, options)!;
        this.SystemPrompt = settings.GetDictionaryJsonValue<string?>("systemPrompt", null, options)!;
        this.UserPrompt = settings.GetDictionaryJsonValue<string?>("userPrompt", null, options)!;
        this.ChoiceIndex = settings.GetDictionaryJsonValue<int?>("choiceIndex", null, options)!;
        this.ChoiceQty = settings.GetDictionaryJsonValue<int?>("choiceQty", null, options)!;
        this.Temperature = settings.GetDictionaryJsonValue<float?>("temperature", null, options)!;
        this.ShowErrorDetails = settings.GetDictionaryJsonValue("showErrorDetails", false, options)!;
        this.AIScope = settings.GetDictionaryJsonValue<string?>("aiScope", null, options)!;
        this.SourceSections = settings.GetDictionaryJsonValue("sourceSections", Array.Empty<string>(), options)!;
        this.AIOutputMode = settings.GetDictionaryJsonValue<string?>("aiOutputMode", null, options)!;
        this.AIInputFields = settings.GetDictionaryJsonValue<string[]?>("aiInputFields", null, options);
    }

    public ReportSectionSettingsModel(JsonDocument settings, JsonSerializerOptions options)
        : this(settings.RootElement.Deserialize<Dictionary<string, object>>(options) ?? new(), options)
    {
    }
    #endregion
}
