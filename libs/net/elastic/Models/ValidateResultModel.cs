using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class ValidateResultModel
{
    #region Properties
    [JsonPropertyName("_shards")]
    public ValidateShardsModel? Shards { get; set; }

    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("explanations")]
    public ValidateExplanation[]? Explanations { get; set; }

    #endregion
}
