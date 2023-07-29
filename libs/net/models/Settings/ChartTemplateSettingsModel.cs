using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ChartTemplateSettingsModel
{
    #region Properties
    public IEnumerable<string> ChartTypes { get; set; } = Array.Empty<string>();
    public IEnumerable<string> GroupBy { get; set; } = Array.Empty<string>();
    public JsonDocument Options { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    public ChartTemplateSettingsModel() { }

    public ChartTemplateSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ChartTypes = settings.GetDictionaryJsonValue<string[]>("chartTypes", Array.Empty<string>(), options)!;
        this.GroupBy = settings.GetDictionaryJsonValue<string[]>("groupBy", Array.Empty<string>(), options)!;
        this.Options = settings.GetDictionaryJsonValue<JsonDocument>("options", JsonDocument.Parse("{}"), options)!;
    }
    #endregion
}
