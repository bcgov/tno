using System.Text.Json.Serialization;

namespace TNO.Models.Azure;

/// <summary>
/// Choice class, provides a response from Azure AI.
/// </summary>
public class Choice
{
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("message")]
    public Message Message { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("content_filter_results")]
    public object ContentFilterResults { get; set; } = default!;
}
