using TNO.API.Models.Settings;

namespace TNO.API.Models.FilterAI;

/// <summary>
/// FilterAIGenerateRequestModel class, a request to generate filter settings from a prompt.
/// </summary>
public class FilterAIGenerateRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The prompt describing the filter (HTML from the prompt editor).
    /// </summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// get/set - The filter's current settings. The result keeps their index and paging, and
    /// their size and order unless the prompt asks for others; the criteria come from the prompt.
    /// </summary>
    public FilterSettingsModel? Settings { get; set; }
    #endregion
}
