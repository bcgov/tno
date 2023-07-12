using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSettingsModel
{
    #region Properties
    public ReportHeadlineSettingsModel Headline { get; set; } = new();
    public ReportContentSettingsModel Content { get; set; } = new();
    public ReportSectionsSettingsModel Sections { get; set; } = new();
    public ReportInstanceSettingsModel Instance { get; set; } = new();
    public bool ViewOnWebOnly { get; set; }
    #endregion

    #region Constructors
    public ReportSettingsModel() { }

    public ReportSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Headline = settings.GetDictionaryJsonValue<ReportHeadlineSettingsModel>("headline", new(), options)!;
        this.Content = settings.GetDictionaryJsonValue<ReportContentSettingsModel>("content", new(), options)!;
        this.Sections = settings.GetDictionaryJsonValue<ReportSectionsSettingsModel>("sections", new(), options)!;
        this.Instance = settings.GetDictionaryJsonValue<ReportInstanceSettingsModel>("instance", new(), options)!;
        this.ViewOnWebOnly = settings.GetDictionaryJsonValue("viewOnWebOnly", false, options);
    }
    #endregion
}
