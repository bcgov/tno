using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsPluginLegendModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsPluginLegendModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("title")]
    public ChartOptionsTitleModel? Title { get; set; }
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("labels")]
    public ChartOptionsLegendLabelsModel? Labels { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("display")]
    public bool? Display { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("align")]
    public string? Align { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("position")]
    public string? Position { get; set; }
    #endregion
}
