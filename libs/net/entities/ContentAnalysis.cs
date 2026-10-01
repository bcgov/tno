using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// ContentAnalysis class, provides a DB model for the structured analysis of one content item for
/// one input fingerprint. Records, including superseded ones, last as long as their content.
/// Inferred values are marked as inferred; missing dates, locations, or attribution are left empty.
/// </summary>
[Table("content_analysis")]
public class ContentAnalysis : AuditColumns
{
    #region Properties
    /// <summary>get/set - Primary key.</summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>get/set - Foreign key to the content.</summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>get/set - The content.</summary>
    public virtual Content? Content { get; set; }

    /// <summary>get/set - Fingerprint of the analysis inputs; the input itself is not copied.</summary>
    [Column("input_hash")]
    public string InputHash { get; set; } = "";

    /// <summary>get/set - Whether this is the analysis of the content's current input.</summary>
    [Column("is_current")]
    public bool IsCurrent { get; set; }

    /// <summary>get/set - Whether the content had no usable text, so only metadata was recorded.</summary>
    [Column("is_metadata_only")]
    public bool IsMetadataOnly { get; set; }

    /// <summary>get/set - Text normalization version.</summary>
    [Column("normalization_version")]
    public string NormalizationVersion { get; set; } = "";

    /// <summary>get/set - Output schema version.</summary>
    [Column("schema_version")]
    public string SchemaVersion { get; set; } = "";

    /// <summary>get/set - Prompt version.</summary>
    [Column("prompt_version")]
    public string PromptVersion { get; set; } = "";

    /// <summary>get/set - Foreign key to the LLM used.</summary>
    [Column("llm_id")]
    public int? LLMId { get; set; }

    /// <summary>get/set - The model deployment used.</summary>
    [Column("model")]
    public string Model { get; set; } = "";

    /// <summary>get/set - The bounded summary, produced from validated facts.</summary>
    [Column("summary")]
    public string Summary { get; set; } = "";

    /// <summary>get/set - The facts supporting the summary, with source spans.</summary>
    [Column("key_facts")]
    public JsonDocument KeyFacts { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - People and organizations, with aliases and roles.</summary>
    [Column("entities")]
    public JsonDocument Entities { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - Places and their roles in the story.</summary>
    [Column("places")]
    public JsonDocument Places { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - Topics the story covers.</summary>
    [Column("topics")]
    public JsonDocument Topics { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - The primary topic label.</summary>
    [Column("primary_topic")]
    public string? PrimaryTopic { get; set; }

    /// <summary>get/set - Foreign key to the analysis topic registry entry for the primary topic.</summary>
    [Column("analysis_topic_id")]
    public int? AnalysisTopicId { get; set; }

    /// <summary>get/set - The analysis topic registry entry for the primary topic.</summary>
    public virtual AnalysisTopic? AnalysisTopic { get; set; }

    /// <summary>get/set - Suggested tag codes (matched and unmatched).</summary>
    [Column("suggested_tags")]
    public JsonDocument SuggestedTags { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - The suggested contributor name.</summary>
    [Column("suggested_contributor")]
    public string? SuggestedContributor { get; set; }

    /// <summary>get/set - Reported events (actor, action, date, location); never executed.</summary>
    [Column("events")]
    public JsonDocument Events { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - Verbatim quotes with speaker and source offsets.</summary>
    [Column("quotes")]
    public JsonDocument Quotes { get; set; } = JsonDocument.Parse("[]");

    /// <summary>get/set - Validation results (dropped quotes, invalid spans, warnings).</summary>
    [Column("validation")]
    public JsonDocument Validation { get; set; } = JsonDocument.Parse("{}");

    /// <summary>get/set - Prompt tokens reported by the provider.</summary>
    [Column("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>get/set - Completion tokens reported by the provider.</summary>
    [Column("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>get/set - When the analysis was produced.</summary>
    [Column("analyzed_on")]
    public DateTime AnalyzedOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysis object.
    /// </summary>
    protected ContentAnalysis() { }

    /// <summary>
    /// Creates a new instance of a ContentAnalysis object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="inputHash"></param>
    public ContentAnalysis(long contentId, string inputHash)
    {
        this.ContentId = contentId;
        this.InputHash = inputHash;
        this.AnalyzedOn = DateTime.UtcNow;
    }
    #endregion
}
