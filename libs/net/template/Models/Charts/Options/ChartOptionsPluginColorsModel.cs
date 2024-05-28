using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsPluginColorsModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsPluginColorsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("forceOverride")]
    public bool? ForceOverride { get; set; }
    #endregion
}
