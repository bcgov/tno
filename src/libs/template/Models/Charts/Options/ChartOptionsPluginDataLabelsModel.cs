using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsPluginDataLabelsModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsPluginDataLabelsModel
{
    #region Properties
    /// <summary>
    /// get/set - Data label position [center|start|end].
    /// </summary>
    [JsonPropertyName("anchor")]
    public string? Anchor { get; set; }

    /// <summary>
    /// get/set - The background color of the data label.
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// get/set - Align the label [center|start|end|right|bottom|left|top].
    /// </summary>
    [JsonPropertyName("align")]
    public string? Align { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("labels")]
    public ChartOptionsLegendLabelsModel? Labels { get; set; }
    #endregion
}
