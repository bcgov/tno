using System.Text.Json.Serialization;

namespace TNO.Services.ExtractQuotes.CoreNLP.models;

public class NerConfidences
{
    [JsonPropertyName("PERSON")]
    public double PERSON { get; set; }

    [JsonPropertyName("ORGANIZATION")]
    public double? ORGANIZATION { get; set; }

    [JsonPropertyName("DATE")]
    public double? DATE { get; set; }

    [JsonPropertyName("TIME")]
    public double? TIME { get; set; }

    [JsonPropertyName("LOCATION")]
    public double? LOCATION { get; set; }

    [JsonPropertyName("NUMBER")]
    public int? NUMBER { get; set; }
}


