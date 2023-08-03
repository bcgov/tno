using System.Text.Json;
using TNO.Core.Extensions;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ChartSettingsModel
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
    /// get/set - Which property to group by [contentType|otherSource|product|series|byline].
    /// </summary>
    public string GroupBy { get; set; } = "product";

    /// <summary>
    /// get/set - An override title to include with the chart.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// get/set - Override whether the chart is horizontal.
    /// </summary>
    public bool IsHorizontal { get; set; }

    /// <summary>
    /// get/set - Override whether the chart will display data value labels.
    /// </summary>
    public bool ShowDataValues { get; set; }

    /// <summary>
    /// get/set - Chart.JS options.
    /// </summary>
    public JsonDocument Options { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartSettingsModel object.
    /// </summary>
    public ChartSettingsModel() { }

    /// <summary>
    /// Creates a new instance of a ChartSettingsModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="options"></param>
    public ChartSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Width = settings.GetDictionaryJsonValue("width", 500, options)!;
        this.Height = settings.GetDictionaryJsonValue("height", 500, options)!;
        this.AltText = settings.GetDictionaryJsonValue("altText", "", options)!;
        this.ChartType = settings.GetDictionaryJsonValue("chartType", "", options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", "", options)!;
        this.Title = settings.GetDictionaryJsonValue("title", "", options)!;
        this.IsHorizontal = settings.GetDictionaryJsonValue("horizontal", false, options)!;
        this.ShowDataValues = settings.GetDictionaryJsonValue("showDataValues", false, options)!;
        this.Options = settings.GetDictionaryJsonValue("options", JsonDocument.Parse("{}"), options)!;
    }

    /// <summary>
    /// Creates a new instance of a ChartSettingsModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="options"></param>
    public ChartSettingsModel(JsonDocument settings, JsonSerializerOptions options)
    {
        this.Width = settings.GetElementValue("width", 500, options)!;
        this.Height = settings.GetElementValue("height", 500, options)!;
        this.AltText = settings.GetElementValue("altText", "", options)!;
        this.ChartType = settings.GetElementValue("chartType", "", options)!;
        this.GroupBy = settings.GetElementValue("groupBy", "", options)!;
        this.Title = settings.GetElementValue("title", "", options)!;
        this.IsHorizontal = settings.GetElementValue("horizontal", false, options)!;
        this.ShowDataValues = settings.GetElementValue("showDataValues", false, options)!;
        this.Options = settings.GetElementValue("options", JsonDocument.Parse("{}"), options)!;
    }
    #endregion
}
