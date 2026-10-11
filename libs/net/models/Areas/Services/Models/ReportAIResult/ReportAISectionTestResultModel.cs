namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAISectionTestResultModel class, the outcome of a test run of an AI section: its output,
/// what it cost, and every request sent with its response.
/// </summary>
public class ReportAISectionTestResultModel
{
    #region Properties
    /// <summary>get/set - Identifies the run.</summary>
    public Guid TestId { get; set; }

    /// <summary>get/set - The report.</summary>
    public int ReportId { get; set; }

    /// <summary>get/set - The report instance whose content was used.</summary>
    public long ReportInstanceId { get; set; }

    /// <summary>get/set - The AI section.</summary>
    public int SectionId { get; set; }

    /// <summary>get/set - The user who requested the run and is told when it completes.</summary>
    public int? RequestorId { get; set; }

    /// <summary>get/set - The state of the run.</summary>
    public ReportAISectionTestStatus Status { get; set; }

    /// <summary>get/set - The section HTML.</summary>
    public string? Output { get; set; }

    /// <summary>get/set - Why the run failed.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - Requests sent to the model.</summary>
    public int RequestCount { get; set; }

    /// <summary>get/set - Prompt tokens reported by the provider.</summary>
    public int PromptTokens { get; set; }

    /// <summary>get/set - Completion tokens reported by the provider.</summary>
    public int CompletionTokens { get; set; }

    /// <summary>get/set - Generation time in milliseconds.</summary>
    public long DurationMs { get; set; }

    /// <summary>get/set - Stories whose text was sent to the model.</summary>
    public int StoryCount { get; set; }

    /// <summary>get/set - The deepest reduction round.</summary>
    public int ReductionDepth { get; set; }

    /// <summary>get/set - Every request sent, in order, with its response.</summary>
    public LlmExchangeModel[] Transcript { get; set; } = Array.Empty<LlmExchangeModel>();

    /// <summary>get/set - When the run was requested.</summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>get/set - When the run finished.</summary>
    public DateTime? CompletedOn { get; set; }
    #endregion
}
