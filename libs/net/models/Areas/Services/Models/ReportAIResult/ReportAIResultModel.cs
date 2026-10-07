using TNO.Entities;

namespace TNO.API.Areas.Services.Models.ReportAIResult;

/// <summary>
/// ReportAIResultModel class, a stored AI section result.
/// </summary>
public class ReportAIResultModel
{
    #region Properties
    /// <summary>get/set - Primary key.</summary>
    public long Id { get; set; }

    /// <summary>get/set - SHA-256 of the manifest, as hex.</summary>
    public string Hash { get; set; } = "";

    /// <summary>get/set - The report.</summary>
    public int ReportId { get; set; }

    /// <summary>get/set - The report instance the result was first generated for.</summary>
    public long? ReportInstanceId { get; set; }

    /// <summary>get/set - The AI section.</summary>
    public int ReportSectionId { get; set; }

    /// <summary>get/set - The result status.</summary>
    public ReportAIResultStatus Status { get; set; }

    /// <summary>get/set - The section HTML.</summary>
    public string Output { get; set; } = "";

    /// <summary>get/set - Why generation failed.</summary>
    public string? Error { get; set; }

    /// <summary>get/set - The pipeline version that produced the result.</summary>
    public string PipelineVersion { get; set; } = "";

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

    /// <summary>get/set - While pending, when the generator's claim lapses.</summary>
    public DateTime? ClaimExpiresOn { get; set; }

    /// <summary>get/set - When the result was first requested.</summary>
    public DateTime? CreatedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIResultModel.
    /// </summary>
    public ReportAIResultModel() { }

    /// <summary>
    /// Creates a new instance of a ReportAIResultModel, initializes with specified parameters.
    /// </summary>
    /// <param name="entity"></param>
    public ReportAIResultModel(Entities.ReportAIResult entity)
    {
        this.Id = entity.Id;
        this.Hash = entity.Hash;
        this.ReportId = entity.ReportId;
        this.ReportInstanceId = entity.ReportInstanceId;
        this.ReportSectionId = entity.ReportSectionId;
        this.Status = entity.Status;
        this.Output = entity.Output;
        this.Error = entity.Error;
        this.PipelineVersion = entity.PipelineVersion;
        this.RequestCount = entity.RequestCount;
        this.PromptTokens = entity.PromptTokens;
        this.CompletionTokens = entity.CompletionTokens;
        this.DurationMs = entity.DurationMs;
        this.StoryCount = entity.StoryCount;
        this.ReductionDepth = entity.ReductionDepth;
        this.ClaimExpiresOn = entity.ClaimExpiresOn;
        this.CreatedOn = entity.CreatedOn;
    }
    #endregion
}
