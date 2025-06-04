using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class ValidateExplanation
{
    #region Properties
    [JsonPropertyName("index")]
    public string Index { get; set; } = "";

    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }
    #endregion
}
