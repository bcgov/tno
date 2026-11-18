namespace TNO.API.Areas.Admin.Models.LLM;

/// <summary>
/// LLMUsageModel class, provides a model that lists what uses an LLM connection.
/// </summary>
public class LLMUsageModel
{
    #region Properties
    /// <summary>
    /// get/set - The LLM connection.
    /// </summary>
    public int LLMId { get; set; }

    /// <summary>
    /// get/set - Everything that uses this connection the next time it runs.
    /// </summary>
    public IEnumerable<LLMUsageItemModel> Items { get; set; } = Array.Empty<LLMUsageItemModel>();

    /// <summary>
    /// get/set - The number of past Content Analysis results this connection produced.
    /// A result records the model that produced it, so a migration never changes these.
    /// </summary>
    public long ContentAnalysisResultCount { get; set; }
    #endregion
}
