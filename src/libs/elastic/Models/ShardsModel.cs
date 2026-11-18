using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class ShardsModel
{
    #region Properties
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("successful")]
    public int Successful { get; set; }

    [JsonPropertyName("skipped")]
    public int Skipped { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }
    #endregion
}
