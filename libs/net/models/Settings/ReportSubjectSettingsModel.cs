using System.Text.Json;
using TNO.Models.Extensions;

namespace TNO.API.Models.Settings;

/// <summary>
/// ReportSubjectSettingsModel class, provides a model for report subject setting options.
/// </summary>
public class ReportSubjectSettingsModel
{
    #region Properties
    /// <summary>
    /// get/set - The subject line.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// get/set - Whether to include today's date in the subject line.
    /// </summary>
    public bool ShowTodaysDate { get; set; }
    #endregion

    #region Constructors
    public ReportSubjectSettingsModel() { }

    public ReportSubjectSettingsModel(Dictionary<string, object> settings, JsonSerializerOptions options)
    {
        this.Text = settings.GetDictionaryJsonValue<string>("text", "", options)!;
        this.ShowTodaysDate = settings.GetDictionaryJsonValue<bool>("showTodaysDate", false, options)!;
    }
    #endregion
}
