using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class AnnotationResponse
{
    [JsonPropertyName("sentences")]
    public List<Sentence> Sentences { get; set; } = new List<Sentence>();

    [JsonPropertyName("quotes")]
    public List<Quote> Quotes { get; set; } = new List<Quote>();
}
