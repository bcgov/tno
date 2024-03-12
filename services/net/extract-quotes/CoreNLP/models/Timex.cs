using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class Timex
{
    [JsonPropertyName("tid")]
    public string tid { get; set; } = "";

    [JsonPropertyName("type")]
    public string type { get; set; } = "";

    [JsonPropertyName("value")]
    public object value { get; set; } = "";
}


