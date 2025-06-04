using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class ValidateShardsModel
{
    #region Properties
    [JsonPropertyName("total")]
    public int Total { get; set; } = 0;

    [JsonPropertyName("failed")]
    public int Failed { get; set; } = 0;

    [JsonPropertyName("successful")]
    public int Successful { get; set; } = 0;

    [JsonPropertyName("skipped")]
    public int? Skipped { get; set; }

    [JsonPropertyName("failures")]
    public ValidateFailure[]? Failures { get; set; }
    #endregion
}
