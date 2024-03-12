using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class Quote
{
    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("text")]
    public string text { get; set; } = "";

    [JsonPropertyName("beginIndex")]
    public int beginIndex { get; set; }

    [JsonPropertyName("endIndex")]
    public int endIndex { get; set; }

    [JsonPropertyName("beginToken")]
    public int beginToken { get; set; }

    [JsonPropertyName("endToken")]
    public int endToken { get; set; }

    [JsonPropertyName("beginSentence")]
    public int beginSentence { get; set; }

    [JsonPropertyName("endSentence")]
    public int endSentence { get; set; }

    [JsonPropertyName("mention")]
    public string mention { get; set; } = "";

    [JsonPropertyName("mentionBegin")]
    public int mentionBegin { get; set; }

    [JsonPropertyName("mentionEnd")]
    public int mentionEnd { get; set; }

    [JsonPropertyName("mentionType")]
    public string mentionType { get; set; } = "";

    [JsonPropertyName("mentionSieve")]
    public string mentionSieve { get; set; } = "";

    [JsonPropertyName("speaker")]
    public string speaker { get; set; } = "";

    [JsonPropertyName("speakerSieve")]
    public string speakerSieve { get; set; } = "";

    [JsonPropertyName("canonicalSpeaker")]
    public string canonicalSpeaker { get; set; } = "";

    [JsonPropertyName("canonicalMentionBegin")]
    public int canonicalMentionBegin { get; set; }

    [JsonPropertyName("canonicalMentionEnd")]
    public int canonicalMentionEnd { get; set; }
}


