using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSettingsModel
{
    #region Properties
    public ReportSubjectSettingsModel Subject { get; set; } = new();
    public ReportHeadlineSettingsModel Headline { get; set; } = new();
    public ReportContentSettingsModel Content { get; set; } = new();
    public ReportSectionsSettingsModel Sections { get; set; } = new();
    public bool DoNotSendEmail { get; set; } = false;
    #endregion

    #region Constructors
    public ReportSettingsModel() { }

    public ReportSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Subject = settings.GetDictionaryJsonValue<ReportSubjectSettingsModel>("subject", new(), options)!;
        this.Headline = settings.GetDictionaryJsonValue<ReportHeadlineSettingsModel>("headline", new(), options)!;
        this.Content = settings.GetDictionaryJsonValue<ReportContentSettingsModel>("content", new(), options)!;
        this.Sections = settings.GetDictionaryJsonValue<ReportSectionsSettingsModel>("sections", new(), options)!;
        this.DoNotSendEmail = settings.GetDictionaryJsonValue<bool>("doNotSendEmail", false, options)!;
    }
    #endregion
}
