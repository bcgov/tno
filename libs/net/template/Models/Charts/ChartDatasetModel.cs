using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts;

/// <summary>
/// ChartDatasetModel class, provides a model that represents Chart.js data.
/// </summary>
public class ChartDatasetModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("labels")]
    public string? Label { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("data")]
    public double?[] Data { get; set; } = Array.Empty<double?>();

    /// <summary>
    /// get/set - Either an array of colours, or a function.
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    public object? BackgroundColor { get; set; }

    /// <summary>
    /// get/set - Either an array of colours, or a function.
    /// </summary>
    [JsonPropertyName("borderColor")]
    public object? BorderColor { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("spanGaps")]
    public bool? SpanGaps { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("borderWidth")]
    public int? BorderWidth { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("minBarLength")]
    public int? MinBarLength { get; set; }
    #endregion
}
