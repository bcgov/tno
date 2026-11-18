using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class TotalModel
{
    #region Properties
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("relation")]
    public string Relation { get; set; } = "";
    #endregion
}
