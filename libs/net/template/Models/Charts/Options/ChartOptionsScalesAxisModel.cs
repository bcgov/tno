using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsScalesAxisModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsScalesAxisModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("ticks")]
    public ChartOptionsScalesAxisTicksModel Ticks { get; set; } = new ChartOptionsScalesAxisTicksModel();

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("title")]
    public ChartOptionsTitleModel? Title { get; set; } = new ChartOptionsTitleModel();

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("stacked")]
    public bool? Stacked { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("reverse")]
    public bool? Reverse { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("display")]
    public bool? Display { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("min")]
    public double? Min { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("max")]
    public double? Max { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("suggestedMin")]
    public double? SuggestedMin { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("suggestedMax")]
    public double? SuggestedMax { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("weight")]
    public int? Weight { get; set; }
    #endregion
}
