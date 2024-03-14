using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class Token
{
    [JsonPropertyName("index")]
    public int index { get; set; }

    [JsonPropertyName("word")]
    public string word { get; set; } = "";

    [JsonPropertyName("originalText")]
    public string originalText { get; set; } = "";

    [JsonPropertyName("lemma")]
    public string lemma { get; set; } = "";

    [JsonPropertyName("characterOffsetBegin")]
    public int characterOffsetBegin { get; set; }

    [JsonPropertyName("characterOffsetEnd")]
    public int characterOffsetEnd { get; set; }

    [JsonPropertyName("pos")]
    public string pos { get; set; } = "";

    [JsonPropertyName("ner")]
    public string ner { get; set; } = "";

    [JsonPropertyName("speaker")]
    public string speaker { get; set; } = "";

    [JsonPropertyName("before")]
    public string before { get; set; } = "";

    [JsonPropertyName("after")]
    public string after { get; set; } = "";

    [JsonPropertyName("normalizedNER")]
    public object? normalizedNER { get; set; }

    [JsonPropertyName("timex")]
    public Timex? timex { get; set; }
}


