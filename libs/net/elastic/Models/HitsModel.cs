using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class HitsModel<T>
    where T : class
{
    #region Properties
    [JsonPropertyName("total")]
    public TotalModel Total { get; set; } = new TotalModel();

    [JsonPropertyName("max_score")]
    public float? MaxScore { get; set; }

    [JsonPropertyName("hits")]
    public IEnumerable<HitModel<T>> Hits { get; set; } = Array.Empty<HitModel<T>>();
    #endregion
}
