using System.Text.Json.Serialization;

namespace TNO.Models.Azure;

/// <summary>
/// Usage class, provides use information.
/// </summary>
public class Usage
{
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
