using System.Text.Json;
using TNO.Core.Extensions;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ChartSettingsModel
{
    #region Properties
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
    public string GroupBy { get; set; } = "contentType";

    /// <summary>
    /// get/set - Chart.JS options.
    /// </summary>
    public JsonDocument Options { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    public ChartSettingsModel() { }

    public ChartSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.AltText = settings.GetDictionaryJsonValue("altText", "", options)!;
        this.ChartType = settings.GetDictionaryJsonValue("chartType", "", options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", "", options)!;
        this.Options = settings.GetDictionaryJsonValue("options", JsonDocument.Parse("{}"), options)!;
    }

    public ChartSettingsModel(JsonDocument settings, JsonSerializerOptions options)
    {
        this.AltText = settings.GetElementValue<string>("altText", "")!;
        this.ChartType = settings.GetElementValue<string>("chartType", "")!;
        this.GroupBy = settings.GetElementValue<string>("groupBy", "")!;
        this.Options = settings.GetElementValue<JsonDocument>("options", JsonDocument.Parse("{}"), options)!;
    }
    #endregion
}
