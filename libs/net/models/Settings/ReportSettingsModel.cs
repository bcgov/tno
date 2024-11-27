using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

public class ReportSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - Subject configuration settings.
    /// </summary>
    public ReportSubjectSettingsModel Subject { get; set; } = new();

    /// <summary>
    /// get/set - Headline configuration settings.
    /// </summary>
    public ReportHeadlineSettingsModel Headline { get; set; } = new();

    /// <summary>
    /// get/set - Content configuration settings.
    /// </summary>
    public ReportContentSettingsModel Content { get; set; } = new();

    /// <summary>
    /// get/set - Section configuration settings.
    /// </summary>
    public ReportSectionsSettingsModel Sections { get; set; } = new();

    /// <summary>
    /// get/set - Treat the report as if it was sent, but do not email it out.
    /// </summary>
    public bool DoNotSendEmail { get; set; } = false;

    /// <summary>
    /// get/set - Omit BC Update content.
    /// </summary>
    public bool OmitBCUpdates { get; set; } = false;
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
        this.OmitBCUpdates = settings.GetDictionaryJsonValue<bool>("omitBCUpdates", false, options)!;
    }
    #endregion
}
