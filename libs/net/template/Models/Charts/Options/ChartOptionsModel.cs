using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("scales")]
    public ChartOptionsScalesModel Scales { get; set; } = new ChartOptionsScalesModel();

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("plugins")]
    public ChartOptionsPluginModel Plugins { get; set; } = new ChartOptionsPluginModel();

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("indexAxis")]
    public string? IndexAxis { get; set; }
    #endregion
}
