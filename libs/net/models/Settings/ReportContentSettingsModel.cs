using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportContentSettingsModel
{
    #region Properties
    public bool IncludeStory { get; set; }
    public bool ShowImage { get; set; }
    public bool UseThumbnail { get; set; }
    public bool HighlightKeywords { get; set; }
    #endregion

    #region Constructors
    public ReportContentSettingsModel() { }

    public ReportContentSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.IncludeStory = settings.GetDictionaryJsonValue<bool>("includeStory", false, options)!;
        this.ShowImage = settings.GetDictionaryJsonValue<bool>("showImage", false, options)!;
        this.UseThumbnail = settings.GetDictionaryJsonValue<bool>("useThumbnail", false, options)!;
        this.HighlightKeywords = settings.GetDictionaryJsonValue<bool>("highlightKeywords", false, options)!;
    }
    #endregion
}
