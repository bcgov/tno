using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsPluginDataLabelsModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsPluginDataLabelsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("anchor")]
    public string? Anchor { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("labels")]
    public ChartOptionsLegendLabelsModel? Labels { get; set; }
    #endregion
}
