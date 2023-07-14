using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSectionsSettingsModel
{
    #region Properties
    public bool HideEmpty { get; set; }
    public bool UsePageBreaks { get; set; }
    #endregion

    #region Constructors
    public ReportSectionsSettingsModel() { }

    public ReportSectionsSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.HideEmpty = settings.GetDictionaryJsonValue<bool>("hideEmpty", false, options)!;
        this.UsePageBreaks = settings.GetDictionaryJsonValue<bool>("usePageBreaks", false, options)!;
    }
    #endregion
}
