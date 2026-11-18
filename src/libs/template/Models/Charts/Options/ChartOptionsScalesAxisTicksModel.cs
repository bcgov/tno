using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsScalesAxisTicksModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsScalesAxisTicksModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("font")]
    public ChartOptionsFontModel? Font { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("stepSize")]
    public int? StepSize { get; set; }
    #endregion
}
