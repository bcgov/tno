using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ChartSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Select the chart type to generate [bar|line|pie|doughnut|bubble|radar|scatter].
    /// </summary>
    public string ChartType { get; set; } = "bar";

    /// <summary>
    /// get/set - Which property to group by.
    /// </summary>
    public string GroupBy { get; set; } = "contentType";

    /// <summary>
    /// get/set - Chart.JS options.
    /// </summary>
    public object Options { get; set; } = new();
    #endregion

    #region Constructors
    public ChartSettingsModel() { }

    public ChartSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ChartType = settings.GetDictionaryJsonValue("chartType", "", options)!;
        this.GroupBy = settings.GetDictionaryJsonValue("groupBy", "", options)!;
        this.Options = settings.GetDictionaryJsonValue<object>("options", new { }, options)!;
    }
    #endregion
}
