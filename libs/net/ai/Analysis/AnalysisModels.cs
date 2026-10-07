namespace TNO.AI.Analysis;

/// <summary>
/// AnalyzerInput record, the content to analyze.
/// </summary>
/// <param name="ContentId">The content.</param>
/// <param name="Headline">The headline.</param>
/// <param name="Body">The body (HTML or text).</param>
/// <param name="Byline">The byline.</param>
/// <param name="Summary">A summary a person wrote, used as context; empty otherwise.</param>
/// <param name="Source">The source name.</param>
/// <param name="MediaType">The media type name.</param>
/// <param name="Series">The series name.</param>
/// <param name="PublishedOn">The publication date.</param>
public record AnalyzerInput(long ContentId, string Headline, string Body, string Byline, string Summary, string Source, string MediaType, string Series, DateTime? PublishedOn);

/// <summary>
/// AnalyzerTag record, a tag analysis may suggest.
/// </summary>
/// <param name="Code">The tag code.</param>
/// <param name="Name">The tag name.</param>
public record AnalyzerTag(string Code, string Name);

/// <summary>A span of the normalized text.</summary>
/// <param name="Start">Character offset.</param>
/// <param name="Length">Character length.</param>
public record TextSpan(int Start, int Length);

/// <summary>A key fact.</summary>
/// <param name="Statement">The fact.</param>
/// <param name="Span">Where it is stated, when it could be located.</param>
/// <param name="IsInferred">Whether it is inferred rather than stated.</param>
public record ExtractedFact(string Statement, TextSpan? Span, bool IsInferred);

/// <summary>A person or organization.</summary>
/// <param name="Type">'person' or 'organization'.</param>
/// <param name="Name">The name used.</param>
/// <param name="Aliases">Other names for the same entity.</param>
/// <param name="Roles">Roles or titles.</param>
/// <param name="IsAmbiguous">The identity is ambiguous; kept separate rather than merged.</param>
public record ExtractedEntity(string Type, string Name, IReadOnlyList<string> Aliases, IReadOnlyList<string> Roles, bool IsAmbiguous);

/// <summary>A place and its role.</summary>
/// <param name="Name">The place.</param>
/// <param name="Role">Its role in the story.</param>
public record ExtractedPlace(string Name, string Role);

/// <summary>A topic.</summary>
/// <param name="Label">The label.</param>
/// <param name="Relevance">How central it is (0-1).</param>
public record ExtractedTopic(string Label, double Relevance);

/// <summary>A reported event; never executed.</summary>
/// <param name="Actor">Who acted.</param>
/// <param name="Action">What they did.</param>
/// <param name="Date">When, only when stated.</param>
/// <param name="Location">Where, only when stated.</param>
public record ExtractedEvent(string Actor, string Action, string? Date, string? Location);

/// <summary>A verbatim quote.</summary>
/// <param name="Statement">The exact words, as they appear in the text.</param>
/// <param name="Speaker">Who said it; empty when not attributed.</param>
/// <param name="Span">Where it appears in the normalized text.</param>
public record ExtractedQuote(string Statement, string Speaker, TextSpan Span);

/// <summary>
/// AnalyzerResult record, the analysis of one content item.
/// </summary>
public record AnalyzerResult
{
    /// <summary>The content had no usable text; only metadata was recorded.</summary>
    public bool IsMetadataOnly { get; init; }
    /// <summary>The bounded summary.</summary>
    public string Summary { get; init; } = "";
    /// <summary>Key facts.</summary>
    public IReadOnlyList<ExtractedFact> KeyFacts { get; init; } = Array.Empty<ExtractedFact>();
    /// <summary>People and organizations.</summary>
    public IReadOnlyList<ExtractedEntity> Entities { get; init; } = Array.Empty<ExtractedEntity>();
    /// <summary>Places.</summary>
    public IReadOnlyList<ExtractedPlace> Places { get; init; } = Array.Empty<ExtractedPlace>();
    /// <summary>Topics, most relevant first.</summary>
    public IReadOnlyList<ExtractedTopic> Topics { get; init; } = Array.Empty<ExtractedTopic>();
    /// <summary>The primary topic label.</summary>
    public string? PrimaryTopic { get; init; }
    /// <summary>Suggested tag codes.</summary>
    public IReadOnlyList<string> SuggestedTags { get; init; } = Array.Empty<string>();
    /// <summary>The suggested contributor (the byline, or the columnist named in the text).</summary>
    public string? SuggestedContributor { get; init; }
    /// <summary>Reported events.</summary>
    public IReadOnlyList<ExtractedEvent> Events { get; init; } = Array.Empty<ExtractedEvent>();
    /// <summary>Validated verbatim quotes.</summary>
    public IReadOnlyList<ExtractedQuote> Quotes { get; init; } = Array.Empty<ExtractedQuote>();
    /// <summary>Validation results: counts of dropped items and warnings.</summary>
    public Dictionary<string, object?> Validation { get; init; } = new();
    /// <summary>Prompt tokens reported by the provider.</summary>
    public int PromptTokens { get; init; }
    /// <summary>Completion tokens reported by the provider.</summary>
    public int CompletionTokens { get; init; }
}
