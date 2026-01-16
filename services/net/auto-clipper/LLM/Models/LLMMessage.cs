using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

public class LLMMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
