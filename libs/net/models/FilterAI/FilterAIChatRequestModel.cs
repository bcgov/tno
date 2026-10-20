using TNO.API.Models.Settings;

namespace TNO.API.Models.FilterAI;

/// <summary>
/// FilterAIChatRequestModel class, a question about how to write a filter prompt.
/// </summary>
public class FilterAIChatRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - The user's question.
    /// </summary>
    public string Question { get; set; } = "";

    /// <summary>
    /// get/set - The draft prompt (HTML from the prompt editor).
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// get/set - The filter's current settings.
    /// </summary>
    public FilterSettingsModel? Settings { get; set; }

    /// <summary>
    /// get/set - The explanation from the last generation, when there was one.
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// get/set - The fence nonce of the conversation, returned by the first answer.
    /// </summary>
    public string? Nonce { get; set; }

    /// <summary>
    /// get/set - The conversation so far (user and assistant turns), as returned by the last answer.
    /// </summary>
    public IEnumerable<FilterAIMessageModel> Messages { get; set; } = Array.Empty<FilterAIMessageModel>();
    #endregion
}
