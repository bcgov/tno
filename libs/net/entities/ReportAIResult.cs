using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// ReportAIResult class, provides a DB model for the generated output of one AI report section,
/// keyed by a hash of its manifest (content, prompts, section settings, model, pipeline version).
/// Previews, views, sends, retries, and resends of an unchanged manifest reuse one result.
/// </summary>
[Table("report_ai_result")]
public class ReportAIResult : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - SHA-256 of the manifest, as hex.
    /// </summary>
    [Column("hash")]
    public string Hash { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to the report.
    /// </summary>
    [Column("report_id")]
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The report.
    /// </summary>
    public virtual Report? Report { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report instance the result was first generated for.
    /// </summary>
    [Column("report_instance_id")]
    public long? ReportInstanceId { get; set; }

    /// <summary>
    /// get/set - The report instance.
    /// </summary>
    public virtual ReportInstance? ReportInstance { get; set; }

    /// <summary>
    /// get/set - Foreign key to the AI report section.
    /// </summary>
    [Column("report_section_id")]
    public int ReportSectionId { get; set; }

    /// <summary>
    /// get/set - The AI report section.
    /// </summary>
    public virtual ReportSection? ReportSection { get; set; }

    /// <summary>
    /// get/set - The result status.
    /// </summary>
    [Column("status")]
    public ReportAIResultStatus Status { get; set; }

    /// <summary>
    /// get/set - The pinned manifest the hash was computed from.
    /// </summary>
    [Column("manifest")]
    public JsonDocument Manifest { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The section HTML.
    /// </summary>
    [Column("output")]
    public string Output { get; set; } = "";

    /// <summary>
    /// get/set - Why generation failed.
    /// </summary>
    [Column("error")]
    public string? Error { get; set; }

    /// <summary>
    /// get/set - The pipeline version that produced the result.
    /// </summary>
    [Column("pipeline_version")]
    public string PipelineVersion { get; set; } = "";

    /// <summary>
    /// get/set - Requests sent to the model.
    /// </summary>
    [Column("request_count")]
    public int RequestCount { get; set; }

    /// <summary>
    /// get/set - Prompt tokens reported by the provider.
    /// </summary>
    [Column("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// get/set - Completion tokens reported by the provider.
    /// </summary>
    [Column("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// get/set - Generation time in milliseconds.
    /// </summary>
    [Column("duration_ms")]
    public long DurationMs { get; set; }

    /// <summary>
    /// get/set - Stories whose text was sent to the model.
    /// </summary>
    [Column("story_count")]
    public int StoryCount { get; set; }

    /// <summary>
    /// get/set - The deepest reduction round.
    /// </summary>
    [Column("reduction_depth")]
    public int ReductionDepth { get; set; }

    /// <summary>
    /// get/set - While pending, when the generator's claim lapses and another may take over.
    /// </summary>
    [Column("claim_expires_on")]
    public DateTime? ClaimExpiresOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIResult object.
    /// </summary>
    protected ReportAIResult() { }

    /// <summary>
    /// Creates a new instance of a ReportAIResult object, initializes with specified parameters.
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="reportId"></param>
    /// <param name="reportSectionId"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="manifest"></param>
    /// <param name="pipelineVersion"></param>
    public ReportAIResult(string hash, int reportId, int reportSectionId, long? reportInstanceId, JsonDocument manifest, string pipelineVersion)
    {
        this.Hash = hash;
        this.ReportId = reportId;
        this.ReportSectionId = reportSectionId;
        this.ReportInstanceId = reportInstanceId;
        this.Manifest = manifest;
        this.PipelineVersion = pipelineVersion;
        this.Status = ReportAIResultStatus.Pending;
    }
    #endregion
}
