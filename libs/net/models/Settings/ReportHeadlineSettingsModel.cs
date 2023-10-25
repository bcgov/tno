using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportHeadlineSettingsModel
{
    #region Properties
    public bool ShowSource { get; set; }
    public bool ShowShortName { get; set; }
    public bool ShowPublishedOn { get; set; }
    public bool ShowSentiment { get; set; }
    public bool ShowByline { get; set; }
    #endregion

    #region Constructors
    public ReportHeadlineSettingsModel() { }

    public ReportHeadlineSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.ShowSource = settings.GetDictionaryJsonValue<bool>("showSource", false, options)!;
        this.ShowShortName = settings.GetDictionaryJsonValue<bool>("showShortName", false, options)!;
        this.ShowPublishedOn = settings.GetDictionaryJsonValue<bool>("showPublishedOn", false, options)!;
        this.ShowSentiment = settings.GetDictionaryJsonValue<bool>("showSentiment", false, options)!;
        this.ShowByline = settings.GetDictionaryJsonValue<bool>("showByline", false, options)!;
    }
    #endregion
}
