using System.Text.Json.Serialization;

namespace TNO.Models.Azure;

/// <summary>
/// ChatCompletionResponse class, provides a simple model for Azure AI chat complete response model.
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = default!;

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = default!;
}
