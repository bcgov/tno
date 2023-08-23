using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportTemplateSectionsSettingsModel
{
    #region Properties
    public bool Enable { get; set; }
    public bool UsePageBreaks { get; set; }
    #endregion

    #region Constructors
    public ReportTemplateSectionsSettingsModel() { }

    public ReportTemplateSectionsSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Enable = settings.GetDictionaryJsonValue<bool>("enable", false, options)!;
        this.UsePageBreaks = settings.GetDictionaryJsonValue<bool>("usePageBreaks", false, options)!;
    }
    #endregion
}
