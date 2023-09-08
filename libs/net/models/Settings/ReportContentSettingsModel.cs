using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportContentSettingsModel
{
    #region Properties
    public bool ClearFolders { get; set; }
    public bool ExcludeHistorical { get; set; }
    public IEnumerable<int> ExcludeReports { get; set; } = Array.Empty<int>();
    public bool ShowLinkToStory { get; set; }
    public bool HighlightKeywords { get; set; }
    #endregion

    #region Constructors
    public ReportContentSettingsModel() { }

    public ReportContentSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ClearFolders = settings.GetDictionaryJsonValue<bool>("clearFolders", false, options)!;
        this.ExcludeHistorical = settings.GetDictionaryJsonValue<bool>("excludeHistorical", false, options)!;
        this.ExcludeReports = settings.GetDictionaryJsonValue<int[]>("excludeReports", Array.Empty<int>(), options)!;
        this.ShowLinkToStory = settings.GetDictionaryJsonValue<bool>("showLinkToStory", false, options)!;
        this.HighlightKeywords = settings.GetDictionaryJsonValue<bool>("highlightKeywords", false, options)!;
    }
    #endregion
}
