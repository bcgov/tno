using TNO.Entities;

namespace TNO.DAL.Analysis;

/// <summary>
/// ContentAnalysisSettings record, the runtime Content-Analysis settings from the setting table.
/// </summary>
/// <param name="LLMId">The LLM used.</param>
/// <param name="ExcludedMediaTypeIds">Media types not analyzed.</param>
/// <param name="ExcludedSourceIds">Sources not analyzed.</param>
/// <param name="TopicPopulationMode">Existing active topics only, or allow creating topics.</param>
public record ContentAnalysisSettings(
    int? LLMId,
    IReadOnlySet<int> ExcludedMediaTypeIds,
    IReadOnlySet<int> ExcludedSourceIds,
    TopicPopulationMode TopicPopulationMode)
{
    /// <summary>
    /// Read the settings. Missing settings use the defaults: no LLM, no exclusions, existing topics only.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static ContentAnalysisSettings Read(TNOContext context)
    {
        var names = new[]
        {
            AdminConfigurableSettingNames.ContentAnalysisLLMId.ToString(),
            AdminConfigurableSettingNames.ContentAnalysisExcludedMediaTypeIds.ToString(),
            AdminConfigurableSettingNames.ContentAnalysisExcludedSourceIds.ToString(),
            AdminConfigurableSettingNames.TopicPopulationMode.ToString(),
        };
        var values = context.Settings
            .Where(s => names.Contains(s.Name) && s.IsEnabled)
            .ToDictionary(s => s.Name, s => s.Value);
        string? Get(AdminConfigurableSettingNames name) => values.TryGetValue(name.ToString(), out var value) ? value : null;

        return new ContentAnalysisSettings(
            int.TryParse(Get(AdminConfigurableSettingNames.ContentAnalysisLLMId), out var llmId) ? llmId : null,
            ParseIds(Get(AdminConfigurableSettingNames.ContentAnalysisExcludedMediaTypeIds)),
            ParseIds(Get(AdminConfigurableSettingNames.ContentAnalysisExcludedSourceIds)),
            Enum.TryParse<TopicPopulationMode>(Get(AdminConfigurableSettingNames.TopicPopulationMode), true, out var topicMode) ? topicMode : TopicPopulationMode.ExistingOnly);
    }

    /// <summary>
    /// Parse a comma-separated list of IDs.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IReadOnlySet<int> ParseIds(string? value)
        => (value ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => int.TryParse(v, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

    /// <summary>
    /// Why content is not analyzed, or null when it is.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public string? GetIneligibleReason(Content content)
    {
        if (this.ExcludedMediaTypeIds.Contains(content.MediaTypeId)) return "The media type is excluded from analysis.";
        if (content.SourceId.HasValue && this.ExcludedSourceIds.Contains(content.SourceId.Value)) return "The source is excluded from analysis.";
        if (content.ContentType == ContentType.AudioVideo && !content.IsApproved && !String.IsNullOrWhiteSpace(content.Body))
            return "The transcript is awaiting approval.";
        return null;
    }
}
