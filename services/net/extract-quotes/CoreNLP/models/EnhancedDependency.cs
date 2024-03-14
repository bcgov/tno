using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class EnhancedDependency
{
    [JsonPropertyName("dep")]
    public string dep { get; set; } = "";

    [JsonPropertyName("governor")]
    public int governor { get; set; }

    [JsonPropertyName("governorGloss")]
    public string governorGloss { get; set; } = "";

    [JsonPropertyName("dependent")]
    public int dependent { get; set; }

    [JsonPropertyName("dependentGloss")]
    public string dependentGloss { get; set; } = "";
}


