namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAIResultCompletionModel class, the outcome of generating a claimed AI section result.
/// </summary>
public class ReportAIResultCompletionModel
{
    #region Properties
    /// <summary>get/set - Whether generation succeeded.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>get/set - The section HTML.</summary>
    public string Output { get; set; } = "";

    /// <summary>get/set - Why generation failed.</summary>
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
    #endregion
}
