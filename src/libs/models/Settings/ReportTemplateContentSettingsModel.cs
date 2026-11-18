using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportTemplateContentSettingsModel
{
    #region Properties
    public bool ExcludeHistorical { get; set; }
    public bool ExcludeReports { get; set; }
    public bool ShowLinkToStory { get; set; }
    public bool HighlightKeywords { get; set; }
    #endregion

    #region Constructors
    public ReportTemplateContentSettingsModel() { }

    public ReportTemplateContentSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ExcludeHistorical = settings.GetDictionaryJsonValue<bool>("excludeHistorical", false, options)!;
        this.ExcludeReports = settings.GetDictionaryJsonValue<bool>("excludeReports", false, options)!;
        this.ShowLinkToStory = settings.GetDictionaryJsonValue<bool>("showLinkToStory", false, options)!;
        this.HighlightKeywords = settings.GetDictionaryJsonValue<bool>("highlightKeywords", false, options)!;
    }
    #endregion
}
