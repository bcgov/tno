using TNO.API.Models.Settings;

namespace TNO.API.Models.FilterAI;

/// <summary>
/// FilterAIGenerateResultModel class, the filter settings generated from a prompt.
/// </summary>
public class FilterAIGenerateResultModel
{
    #region Properties
    /// <summary>
    /// get/set - The generated settings, including any extended query for conditions the
    /// settings cannot express.
    /// </summary>
    public FilterSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The model's explanation of how the settings meet the prompt.
    /// </summary>
    public string Explanation { get; set; } = "";

    /// <summary>
    /// get/set - The tools the model used, in order.
    /// </summary>
    public IEnumerable<FilterAIStepModel> Steps { get; set; } = Array.Empty<FilterAIStepModel>();
    #endregion
}
