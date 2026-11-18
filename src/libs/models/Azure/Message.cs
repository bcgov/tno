using System.Text.Json.Serialization;

namespace TNO.Models.Azure;

/// <summary>
/// Message class, provides the message from Azure AI response.
/// </summary>
public class Message
{
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("refusal")]
    public object Refusal { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("annotations")]
    public List<object> Annotations { get; set; } = default!;
}
