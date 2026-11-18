using TNO.API.Models.Settings;

namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAISectionTestRequestModel class, a request to run one AI section once with draft settings
/// against the report instance's content, returning the output and the full transcript. Nothing is
/// stored.
/// </summary>
public class ReportAISectionTestRequestModel
{
    #region Properties
    /// <summary>get/set - Identifies the run to the requestor.</summary>
    public Guid TestId { get; set; }

    /// <summary>get/set - The saved AI section the draft settings apply to.</summary>
    public int SectionId { get; set; }

    /// <summary>get/set - The draft section settings (prompt, model, fields, scope).</summary>
    public ReportSectionSettingsModel Settings { get; set; } = new();
    #endregion
}
