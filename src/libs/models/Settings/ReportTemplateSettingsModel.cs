using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportTemplateSettingsModel
{
    #region Properties
    public ReportTemplateSubjectSettingsModel Subject { get; set; } = new();
    public ReportTemplateHeadlineSettingsModel Headline { get; set; } = new();
    public ReportTemplateContentSettingsModel Content { get; set; } = new();
    public ReportTemplateSectionsSettingsModel Sections { get; set; } = new();
    public bool EnableCharts { get; set; }
    #endregion

    #region Constructors
    public ReportTemplateSettingsModel() { }

    public ReportTemplateSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Subject = settings.GetDictionaryJsonValue<ReportTemplateSubjectSettingsModel>("subject", new(), options)!;
        this.Headline = settings.GetDictionaryJsonValue<ReportTemplateHeadlineSettingsModel>("headline", new(), options)!;
        this.Content = settings.GetDictionaryJsonValue<ReportTemplateContentSettingsModel>("content", new(), options)!;
        this.Sections = settings.GetDictionaryJsonValue<ReportTemplateSectionsSettingsModel>("sections", new(), options)!;
        this.EnableCharts = settings.GetDictionaryJsonValue<bool>("enableCharts", false, options)!;
    }
    #endregion
}
