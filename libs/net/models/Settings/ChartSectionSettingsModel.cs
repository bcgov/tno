using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// get/set - Automatically resize chart based on data.
    /// </summary>
    public bool? AutoResize { get; set; }

    /// <summary>
    /// get/set - Whether width and height should force aspect ratio.
    /// </summary>
    public bool? MaintainAspectRatio { get; set; }

    /// <summary>
    /// get/set - The chart aspect ratio.
    /// </summary>
    public int? AspectRatio { get; set; }

    /// <summary>
    /// get/set - Alternate text to display in the img element.
    /// </summary>
    public string AltText { get; set; } = "Image";

    /// <summary>
    /// get/set - Select the chart type to generate [bar|line|pie|doughnut|bubble|radar|scatter].
    /// </summary>
    public string ChartType { get; set; } = "bar";

    /// <summary>
    /// get/set - Which property to group by [contentType|otherSource|mediaType|series|byline|sentiment|sentimentSimple].
    /// </summary>
    public string GroupBy { get; set; } = "otherSource";

    /// <summary>
    /// get/set - The sort order of the group by.
    /// </summary>
    public string GroupByOrder { get; set; } = "asc";

    /// <summary>
    /// get/set - Which property to separate datasets by [contentType|otherSource|mediaType|series|byline|section].
    /// </summary>
    public string Dataset { get; set; } = "";

    /// <summary>
    /// get/set - The sort order of the dataset.
    /// </summary>
    public string DatasetOrder { get; set; } = "asc";

    /// <summary>
    /// get/set - The value used for the dataset [count|sentiment|sentimentSimple].
    /// </summary>
    public string DatasetValue { get; set; } = "count";

    /// <summary>
    /// get/set - Whether to exclude empty values in the returned dataset.  This only works on a few values that can be null or empty.
    /// </summary>
    public bool ExcludeEmptyValues { get; set; }

    /// <summary>
    /// get/set - Whether to apply the dataset colour to each value instead of a dataset
    /// </summary>
    public bool ApplyColorToValue { get; set; }

    /// <summary>
    /// get/set - An array of colour to use for this chart datasets.
    /// </summary>
    public string[]? DatasetColors { get; set; }

    /// <summary>
    /// get/set - An array of colour to use for this chart datasets borders.
    /// </summary>
    public string[]? DatasetBorderColors { get; set; }

    /// <summary>
    /// get/set - An array of colour to use for this chart data labels.
    /// </summary>
    public string[]? DataLabelColors { get; set; }

    /// <summary>
    /// get/set - Override whether the chart is horizontal.
    /// </summary>
    public bool? IsHorizontal { get; set; }

    /// <summary>
    /// get/set - Chart title
    /// </summary>
    public string? Title { get; set; } = "";

    /// <summary>
    /// get/set - Chart title font size
    /// </summary>
    public int? TitleFontSize { get; set; }

    /// <summary>
    /// get/set - Chart subtitle
    /// </summary>
    public string? Subtitle { get; set; } = "";

    /// <summary>
    /// get/set - Chart title font size
    /// </summary>
    public int? SubtitleFontSize { get; set; }

    /// <summary>
    /// get/set - Whether to display the legend
    /// </summary>
    public bool? ShowLegend { get; set; }

    /// <summary>
    /// get/set - Legend title
    /// </summary>
    public string? LegendTitle { get; set; }

    /// <summary>
    /// get/set - Chart title font size
    /// </summary>
    public int? LegendTitleFontSize { get; set; }

    /// <summary>
    /// get/set - Legend position ['top' | 'left' | 'bottom' | 'right' | 'chartArea']
    /// </summary>
    public string? LegendPosition { get; set; }

    /// <summary>
    /// get/set - Legend align ['start' | 'center' | 'end']
    /// </summary>
    public string? LegendAlign { get; set; }

    /// <summary>
    /// get/set - Size of the dataset colour box legend
    /// </summary>
    public int? LegendLabelBoxWidth { get; set; }

    /// <summary>
    /// get/set - Legend label font size
    /// </summary>
    public int? LegendLabelFontSize { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? XShowAxisLabels { get; set; }

    /// <summary>
    /// get/set - X axis legend title
    /// </summary>
    [JsonPropertyName("xLegend")]
    public string? XLegend { get; set; }

    /// <summary>
    /// get/set - Legend X axis title font size
    /// </summary>
    public int? XLegendFontSize { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? YShowAxisLabels { get; set; }

    /// <summary>
    /// get/set - Y axis legend title
    /// </summary>
    [JsonPropertyName("yLegend")]
    public string? YLegend { get; set; }

    /// <summary>
    /// get/set - Legend Y axis title font size
    /// </summary>
    public int? YLegendFontSize { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? ShowDataLabels { get; set; }

    /// <summary>
    /// get/set - Chart data label font size
    /// </summary>
    public int? DataLabelFontSize { get; set; }

    /// <summary>
    /// get/set - Whether to stack datasets (only works on some charts).
    /// </summary>
    public bool? Stacked { get; set; }

    /// <summary>
    /// get/set - Scale suggested minimum value.
    /// </summary>
    public int? ScaleSuggestedMin { get; set; }

    /// <summary>
    /// get/set - Scale suggested maximum value.
    /// </summary>
    public int? ScaleSuggestedMax { get; set; }

    /// <summary>
    /// get/set - Scale tick step size.
    /// </summary>
    public int? ScaleTicksStepSize { get; set; }

    /// <summary>
    /// get/set - Scale add to max dataset size.
    /// </summary>
    public int? ScaleCalcMax { get; set; }

    /// <summary>
    /// get/set - Minimum width in pixels a bar must be.
    /// </summary>
    public int? MinBarLength { get; set; }

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
