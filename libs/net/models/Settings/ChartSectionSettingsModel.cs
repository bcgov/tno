using System.Text.Json;
namespace TNO.API.Models.Settings;

public class ChartSectionSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Width of generated chart.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// get/set - Height of generated chart.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// get/set - Alternate text to display in the img element.
    /// </summary>
    public string AltText { get; set; } = "Image";

    /// <summary>
    /// get/set - Select the chart type to generate [bar|line|pie|doughnut|bubble|radar|scatter].
    /// </summary>
    public string ChartType { get; set; } = "bar";

    /// <summary>
    /// get/set - Which property to group by [contentType|otherSource|mediaType|series|byline].
    /// </summary>
    public string GroupBy { get; set; } = "mediaType";

    /// <summary>
    /// get/set - Override whether the chart is horizontal.
    /// </summary>
    public bool? IsHorizontal { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? ShowLegend { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? ShowLegendTitle { get; set; }

    /// <summary>
    /// get/set - An override title to include with the chart.
    /// </summary>
    public string? Title { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? ShowDataLabels { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? ShowAxis { get; set; }

    /// <summary>
    /// get/set - Chart.JS options.
    /// </summary>
    public JsonDocument Options { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartSectionSettingsModel object.
    /// </summary>
    public ChartSectionSettingsModel() { }
    #endregion
}
