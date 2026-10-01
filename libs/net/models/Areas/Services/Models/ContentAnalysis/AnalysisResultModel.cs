namespace TNO.API.Areas.Services.Models.ContentAnalysis;

/// <summary>
/// AnalysisSpanModel class, a span of the analyzed (normalized) text.
/// </summary>
public class AnalysisSpanModel
{
    /// <summary>get/set - Character offset.</summary>
    public int Start { get; set; }

    /// <summary>get/set - Character length.</summary>
    public int Length { get; set; }
}

/// <summary>
/// AnalysisFactModel class, a key fact with its supporting span.
/// </summary>
public class AnalysisFactModel
{
    /// <summary>get/set - The fact.</summary>
    public string Statement { get; set; } = "";

    /// <summary>get/set - Where it is stated.</summary>
    public AnalysisSpanModel? Span { get; set; }

    /// <summary>get/set - Whether the fact is inferred rather than stated.</summary>
    public bool IsInferred { get; set; }
}

/// <summary>
/// AnalysisEntityModel class, a person or organization.
/// </summary>
public class AnalysisEntityModel
{
    /// <summary>get/set - 'person' or 'organization'.</summary>
    public string Type { get; set; } = "";

    /// <summary>get/set - The name used.</summary>
    public string Name { get; set; } = "";

    /// <summary>get/set - Other names used for the same entity in the story.</summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>get/set - Roles or titles.</summary>
    public string[] Roles { get; set; } = Array.Empty<string>();

    /// <summary>get/set - Whether the identity is ambiguous (kept separate rather than merged).</summary>
    public bool IsAmbiguous { get; set; }
}

/// <summary>
/// AnalysisPlaceModel class, a place and its role in the story.
/// </summary>
public class AnalysisPlaceModel
{
    /// <summary>get/set - The place.</summary>
    public string Name { get; set; } = "";

    /// <summary>get/set - Its role (where it happened, affected area, mentioned).</summary>
    public string Role { get; set; } = "";

    /// <summary>get/set - Latitude, only when verified.</summary>
    public double? Latitude { get; set; }

    /// <summary>get/set - Longitude, only when verified.</summary>
    public double? Longitude { get; set; }
}

/// <summary>
/// AnalysisTopicModel class, a topic the story covers.
/// </summary>
public class AnalysisTopicModel
{
    /// <summary>get/set - The topic label.</summary>
    public string Label { get; set; } = "";

    /// <summary>get/set - How central it is (0-1).</summary>
    public double Relevance { get; set; }
}

/// <summary>
/// AnalysisEventModel class, a reported event. Extraction never executes actions.
/// </summary>
public class AnalysisEventModel
{
    /// <summary>get/set - Who acted.</summary>
    public string Actor { get; set; } = "";

    /// <summary>get/set - What they did.</summary>
    public string Action { get; set; } = "";

    /// <summary>get/set - When, only when stated.</summary>
    public string? Date { get; set; }

    /// <summary>get/set - Where, only when stated.</summary>
    public string? Location { get; set; }
}

/// <summary>
/// AnalysisQuoteModel class, a verbatim quote with its speaker and offsets.
/// </summary>
public class AnalysisQuoteModel
{
    /// <summary>get/set - The exact words.</summary>
    public string Statement { get; set; } = "";

    /// <summary>get/set - Who said it; empty when not attributed.</summary>
    public string Speaker { get; set; } = "";

    /// <summary>get/set - Where it appears in the analyzed text.</summary>
    public AnalysisSpanModel? Span { get; set; }
}

/// <summary>
/// AnalysisResultModel class, a worker's analysis of claimed content.
/// </summary>
public class AnalysisResultModel : AnalysisLeaseModel
{
    /// <summary>get/set - The input fingerprint analyzed.</summary>
    public string InputHash { get; set; } = "";

    /// <summary>get/set - The content had no usable text.</summary>
    public bool IsMetadataOnly { get; set; }

    /// <summary>
    /// get/set - The processes the worker ran. Only their results are applied to the content.
    /// </summary>
    public TNO.Entities.AnalysisProcess Processes { get; set; }

    /// <summary>get/set - Text normalization version.</summary>
    public string NormalizationVersion { get; set; } = "";

    /// <summary>get/set - Output schema version.</summary>
    public string SchemaVersion { get; set; } = "";

    /// <summary>get/set - Prompt version.</summary>
    public string PromptVersion { get; set; } = "";

    /// <summary>get/set - The LLM used.</summary>
    public int? LLMId { get; set; }

    /// <summary>get/set - The model deployment used.</summary>
    public string Model { get; set; } = "";

    /// <summary>get/set - The bounded summary.</summary>
    public string Summary { get; set; } = "";

    /// <summary>get/set - Key facts.</summary>
    public IEnumerable<AnalysisFactModel> KeyFacts { get; set; } = Array.Empty<AnalysisFactModel>();

    /// <summary>get/set - People and organizations.</summary>
    public IEnumerable<AnalysisEntityModel> Entities { get; set; } = Array.Empty<AnalysisEntityModel>();

    /// <summary>get/set - Places.</summary>
    public IEnumerable<AnalysisPlaceModel> Places { get; set; } = Array.Empty<AnalysisPlaceModel>();

    /// <summary>get/set - Topics.</summary>
    public IEnumerable<AnalysisTopicModel> Topics { get; set; } = Array.Empty<AnalysisTopicModel>();

    /// <summary>get/set - The primary topic label.</summary>
    public string? PrimaryTopic { get; set; }

    /// <summary>get/set - Suggested tag codes or names.</summary>
    public IEnumerable<string> SuggestedTags { get; set; } = Array.Empty<string>();

    /// <summary>get/set - The suggested contributor (from the byline or columnist).</summary>
    public string? SuggestedContributor { get; set; }

    /// <summary>get/set - Reported events.</summary>
    public IEnumerable<AnalysisEventModel> Events { get; set; } = Array.Empty<AnalysisEventModel>();

    /// <summary>get/set - Validated verbatim quotes.</summary>
    public IEnumerable<AnalysisQuoteModel> Quotes { get; set; } = Array.Empty<AnalysisQuoteModel>();

    /// <summary>get/set - Validation results.</summary>
    public Dictionary<string, object?> Validation { get; set; } = new();

    /// <summary>get/set - Prompt tokens reported by the provider.</summary>
    public int PromptTokens { get; set; }

    /// <summary>get/set - Completion tokens reported by the provider.</summary>
    public int CompletionTokens { get; set; }
}

/// <summary>
/// AnalysisSubmitResultModel class, the outcome of submitting an analysis.
/// </summary>
public class AnalysisSubmitResultModel
{
    /// <summary>'Accepted', 'Duplicate' (already stored), or 'Stale' (rejected; nothing populated).</summary>
    public string Status { get; set; } = "";

    /// <summary>get/set - The stored analysis.</summary>
    public long? AnalysisId { get; set; }

    /// <summary>get/set - Why a submission was rejected.</summary>
    public string? Reason { get; set; }

    /// <summary>get/set - The editorial fields analysis populated.</summary>
    public IEnumerable<string> PopulatedFields { get; set; } = Array.Empty<string>();

    /// <summary>get/set - Whether the content changed, so it is re-indexed and editors are told.</summary>
    public bool ContentChanged { get; set; }

    /// <summary>get/set - The content's status, to choose the index action.</summary>
    public TNO.Entities.ContentStatus? ContentStatus { get; set; }
}
