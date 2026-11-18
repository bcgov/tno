namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAISectionPromptModel class, the requests an AI section would send to its model for the
/// report's current content, described without sending any.
/// </summary>
public class ReportAISectionPromptModel
{
    #region Properties
    /// <summary>get/set - The report.</summary>
    public int ReportId { get; set; }

    /// <summary>get/set - The report instance whose content was used.</summary>
    public long? ReportInstanceId { get; set; }

    /// <summary>get/set - The AI section.</summary>
    public int SectionId { get; set; }

    /// <summary>get/set - The section heading.</summary>
    public string SectionLabel { get; set; } = "";

    /// <summary>get/set - The model the section uses.</summary>
    public string? LlmName { get; set; }

    /// <summary>get/set - True: the exchanges were planned, not sent, so they carry no responses.</summary>
    public bool IsPlanned { get; set; } = true;

    /// <summary>get/set - The exchanges in order.</summary>
    public LlmExchangeModel[] Exchanges { get; set; } = Array.Empty<LlmExchangeModel>();

    /// <summary>get/set - Why the prompt could not be described.</summary>
    public string? Error { get; set; }
    #endregion
}
