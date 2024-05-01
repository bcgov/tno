using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportContentSettingsModel
{
    #region Properties
    public bool ClearFolders { get; set; }
    public bool OnlyNewContent { get; set; }
    public bool ExcludeHistorical { get; set; }
    public IEnumerable<int> ExcludeReports { get; set; } = Array.Empty<int>();
    public bool ShowLinkToStory { get; set; }
    public bool HighlightKeywords { get; set; }
    public bool CopyPriorInstance { get; set; }
    public bool ClearOnStartNewReport { get; set; }
    #endregion

    #region Constructors
    public ReportContentSettingsModel() { }

    public ReportContentSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ClearFolders = settings.GetDictionaryJsonValue("clearFolders", false, options)!;
        this.OnlyNewContent = settings.GetDictionaryJsonValue("onlyNewContent", false, options)!;
        this.ExcludeHistorical = settings.GetDictionaryJsonValue("excludeHistorical", false, options)!;
        this.ExcludeReports = settings.GetDictionaryJsonValue("excludeReports", Array.Empty<int>(), options)!;
        this.ShowLinkToStory = settings.GetDictionaryJsonValue("showLinkToStory", false, options)!;
        this.HighlightKeywords = settings.GetDictionaryJsonValue("highlightKeywords", false, options)!;
        this.CopyPriorInstance = settings.GetDictionaryJsonValue("copyPriorInstance", false, options)!;
        this.ClearOnStartNewReport = settings.GetDictionaryJsonValue("clearOnStartNewReport", false, options)!;
    }
    #endregion
}
