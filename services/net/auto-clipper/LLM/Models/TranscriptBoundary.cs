using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

public class TranscriptBoundary
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = "";

    [JsonPropertyName("category")]
    public string? Category { get; set; } = "";

    [JsonPropertyName("score")]
    public float Score { get; set; }
}
