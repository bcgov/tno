using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class Sentence
{
    [JsonPropertyName("index")]
    public int index { get; set; }

    [JsonPropertyName("paragraph")]
    public int paragraph { get; set; }

    [JsonPropertyName("basicDependencies")]
    public List<BasicDependency> basicDependencies { get; set; } = new List<BasicDependency>();

    [JsonPropertyName("enhancedDependencies")]
    public List<EnhancedDependency> enhancedDependencies { get; set; } = new List<EnhancedDependency>();

    [JsonPropertyName("enhancedPlusPlusDependencies")]
    public List<EnhancedPlusPlusDependency> enhancedPlusPlusDependencies { get; set; } = new List<EnhancedPlusPlusDependency>();

    [JsonPropertyName("entitymentions")]
    public List<EntityMention> entityMentions { get; set; } = new List<EntityMention>();

    [JsonPropertyName("tokens")]
    public List<Token> tokens { get; set; } = new List<Token>();
}


