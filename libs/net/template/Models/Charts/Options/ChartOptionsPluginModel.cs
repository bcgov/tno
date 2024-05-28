using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsPluginModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsPluginModel
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
    [JsonPropertyName("subtitle")]
    public ChartOptionsTitleModel? Subtitle { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("colors")]
    public ChartOptionsPluginColorsModel? Colors { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("legend")]
    public ChartOptionsPluginLegendModel? Legend { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("datalabels")]
    public ChartOptionsPluginDataLabelsModel? Datalabels { get; set; }
    #endregion
}
