using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class ValidateFailure
{
    #region Properties
    [JsonPropertyName("index")]
    public string? Index { get; set; }

    [JsonPropertyName("node")]
    public string? Node { get; set; }

    [JsonPropertyName("shard")]
    public int Shard { get; set; } = 0;

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("reason")]
    public ValidateReason Reason { get; set; } = new ValidateReason();
    #endregion
}
