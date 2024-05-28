using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts;

/// <summary>
/// ChartDataModel class, provides a model that represents Chart.js data.
/// </summary>
public class ChartDataModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("labels")]
    public string[] Labels { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("datasets")]
    public ChartDatasetModel[] Datasets { get; set; } = Array.Empty<ChartDatasetModel>();
    #endregion
}
