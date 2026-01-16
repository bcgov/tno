using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

public class LLMChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public LLMMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}
