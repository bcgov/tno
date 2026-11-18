using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsFontModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsFontModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("size")]
    public int? size { get; set; }
    #endregion
}
