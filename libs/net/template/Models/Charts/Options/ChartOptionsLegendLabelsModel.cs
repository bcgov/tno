using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsLegendLabelsModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsLegendLabelsModel : ChartOptionsTitleModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("boxWidth")]
    public int? BoxWidth { get; set; }
    #endregion
}
