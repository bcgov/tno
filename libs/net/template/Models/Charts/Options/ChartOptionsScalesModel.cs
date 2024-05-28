using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsScalesModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsScalesModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("x")]
    public ChartOptionsScalesAxisModel X { get; set; } = new ChartOptionsScalesAxisModel();

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("y")]
    public ChartOptionsScalesAxisModel Y { get; set; } = new ChartOptionsScalesAxisModel();
    #endregion
}
