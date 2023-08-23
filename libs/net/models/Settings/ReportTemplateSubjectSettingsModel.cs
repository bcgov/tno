using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

/// <summary>
/// ReportTemplateSubjectSettingsModel class, provides a model for report subject setting options.
/// </summary>
public class ReportTemplateSubjectSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - The subject line.
    /// </summary>
    public bool Text { get; set; }

    /// <summary>
    /// get/set - Whether to include today's date in the subject line.
    /// </summary>
    public bool ShowTodaysDate { get; set; }
    #endregion

    #region Constructors
    public ReportTemplateSubjectSettingsModel() { }

    public ReportTemplateSubjectSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Text = settings.GetDictionaryJsonValue<bool>("text", false, options)!;
        this.ShowTodaysDate = settings.GetDictionaryJsonValue<bool>("showTodaysDate", false, options)!;
    }
    #endregion
}
