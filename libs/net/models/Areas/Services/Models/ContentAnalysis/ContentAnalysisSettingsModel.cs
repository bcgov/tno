using TNO.Entities;

namespace TNO.API.Areas.Services.Models.ContentAnalysis;

/// <summary>
/// ContentAnalysisSettingsModel class, the runtime Content-Analysis settings.
/// </summary>
public class ContentAnalysisSettingsModel
{
    #region Properties

    /// <summary>get/set - The LLM used.</summary>
    public int? LLMId { get; set; }

    /// <summary>get/set - Media types not analyzed.</summary>
    public IEnumerable<int> ExcludedMediaTypeIds { get; set; } = Array.Empty<int>();

    /// <summary>get/set - Sources not analyzed.</summary>
    public IEnumerable<int> ExcludedSourceIds { get; set; } = Array.Empty<int>();
    #endregion
}
