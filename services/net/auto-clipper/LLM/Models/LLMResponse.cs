using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

public class LLMResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<LLMChoice>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public LLMUsage? Usage { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
