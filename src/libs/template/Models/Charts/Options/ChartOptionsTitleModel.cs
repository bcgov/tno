using System.Text.Json.Serialization;

namespace TNO.TemplateEngine.Models.Charts.Options;

/// <summary>
/// ChartOptionsTitleModel class, provides a model that represents Chart.js options.
/// </summary>
public class ChartOptionsTitleModel
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
    [JsonPropertyName("display")]
    public bool? Display { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    #endregion
}
