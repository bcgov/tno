using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportInstanceSettingsModel
{
    #region Properties
    public bool ExcludeHistorical { get; set; }
    public IEnumerable<int> ExcludeReports { get; set; } = Array.Empty<int>();
    #endregion

    #region Constructors
    public ReportInstanceSettingsModel() { }

    public ReportInstanceSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ExcludeHistorical = settings.GetDictionaryJsonValue<bool>("excludeHistorical", false, options)!;
        this.ExcludeReports = settings.GetDictionaryJsonValue<int[]>("excludeReports", Array.Empty<int>(), options)!;
    }
    #endregion
}
