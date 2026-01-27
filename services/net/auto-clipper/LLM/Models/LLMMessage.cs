using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

public class LLMMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    [JsonConverter(typeof(LlmMessageContentConverter))]
    public string? Content { get; set; }
}
