using System.Text.Json.Serialization;

namespace TNO.Services.AutoClipper.LLM.Models;

public class TranscriptBoundaries
{
    [JsonPropertyName("boundaries")]
    public TranscriptBoundary[] Boundaries { get; set; } = [];
}
