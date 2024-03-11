using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class EntityMention
{
    [JsonPropertyName("docTokenBegin")]
    public int docTokenBegin { get; set; }

    [JsonPropertyName("docTokenEnd")]
    public int docTokenEnd { get; set; }

    [JsonPropertyName("tokenBegin")]
    public int tokenBegin { get; set; }

    [JsonPropertyName("tokenEnd")]
    public int tokenEnd { get; set; }

    [JsonPropertyName("text")]
    public string text { get; set; } = "";

    [JsonPropertyName("characterOffsetBegin")]
    public int characterOffsetBegin { get; set; }

    [JsonPropertyName("characterOffsetEnd")]
    public int characterOffsetEnd { get; set; }

    [JsonPropertyName("ner")]
    public string ner { get; set; } = "";

    [JsonPropertyName("nerConfidences")]
    public NerConfidences? nerConfidences { get; set; }

    [JsonPropertyName("normalizedNER")]
    public object normalizedNER { get; set; } = "";

    [JsonPropertyName("timex")]
    public Timex? timex { get; set; }
}


