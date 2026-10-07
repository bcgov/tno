using TNO.AI.Tokens;

namespace TNO.AI.Synthesis;

/// <summary>
/// SynthesisOutputMode enum, what an AI section produces.
/// </summary>
public enum SynthesisOutputMode
{
    /// <summary>
    /// The model writes the section from the synthesized findings, following the section prompt.
    /// </summary>
    FreeText = 0,
    /// <summary>
    /// The application writes headings with bullet statements and their source lists.
    /// </summary>
    TopicSummary = 1,
}

/// <summary>
/// SynthesisStory record, one story given to synthesis.
/// </summary>
/// <param name="ContentId">The content.</param>
/// <param name="Headline">The headline.</param>
/// <param name="Metadata">One line of metadata (source, date, byline).</param>
/// <param name="Text">The input text: analysis when it exists, otherwise the full body.</param>
/// <param name="Url">A link to the story, for source lists.</param>
/// <param name="Anchor">An anchor to the story in the report body, preferred over the URL.</param>
/// <param name="Group">The group the story belongs to (a topic), or null for the whole section.</param>
/// <param name="AnalysisId">The analysis the text came from, pinned in the manifest.</param>
public record SynthesisStory(long ContentId, string Headline, string Metadata, string Text, string? Url = null, string? Anchor = null, string? Group = null, long? AnalysisId = null);

/// <summary>
/// SynthesisHistoricalInstance record, a previous report instance given as context.
/// </summary>
/// <param name="Label">How the instance is named to the model ("Report of 2026-09-28").</param>
/// <param name="Stories">The instance's stories.</param>
public record SynthesisHistoricalInstance(string Label, IReadOnlyList<SynthesisStory> Stories);

/// <summary>
/// SynthesisRequest record, one AI section to synthesize.
/// </summary>
/// <param name="SectionLabel">The section label.</param>
/// <param name="SystemPrompt">The section's system prompt.</param>
/// <param name="UserPrompt">The section's instructions.</param>
/// <param name="Mode">What the section produces.</param>
/// <param name="Stories">Every story in the section's input.</param>
/// <param name="History">Previous instances, oldest first.</param>
/// <param name="Endpoint">The model deployment.</param>
/// <param name="Limits">The deployment's limits.</param>
/// <param name="Temperature">The final request's temperature.</param>
/// <param name="ChoiceCount">The final request's number of choices.</param>
/// <param name="ChoiceIndex">The choice to use; -1 joins every choice.</param>
public record SynthesisRequest(
    string SectionLabel,
    string? SystemPrompt,
    string UserPrompt,
    SynthesisOutputMode Mode,
    IReadOnlyList<SynthesisStory> Stories,
    IReadOnlyList<SynthesisHistoricalInstance> History,
    LlmEndpoint Endpoint,
    LlmLimits Limits,
    float? Temperature = null,
    int? ChoiceCount = null,
    int? ChoiceIndex = null);

/// <summary>
/// SynthesisOptions class, configuration for bounded synthesis.
/// </summary>
public class SynthesisOptions
{
    /// <summary>
    /// get/set - Percentage of the context window held back for token estimation error.
    /// </summary>
    public int SafetyMarginPercent { get; set; } = 10;

    /// <summary>
    /// get/set - The most reduction rounds before synthesis fails.
    /// </summary>
    public int MaxReductionDepth { get; set; } = 8;

    /// <summary>
    /// get/set - The most requests in flight at once for one section.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 8;

    /// <summary>
    /// get/set - The most input tokens in one map or reduce request. Smaller requests return sooner
    /// and run in parallel; the model's context window still applies when it is smaller.
    /// </summary>
    public int MaxBatchInputTokens { get; set; } = 16000;

    /// <summary>
    /// get/set - The most output tokens one map or reduce request may return. A response that
    /// reaches it is split and retried. The final free-text request uses the model's limit.
    /// </summary>
    public int MaxBatchOutputTokens { get; set; } = 8000;

    /// <summary>
    /// get/set - Attempts per request for throttling and transient failures.
    /// </summary>
    public int RequestAttempts { get; set; } = 3;

    /// <summary>
    /// get/set - How long a generator holds its claim on a result before another may take over.
    /// </summary>
    public int ClaimLeaseSeconds { get; set; } = 900;

    /// <summary>
    /// get/set - Per-request timeout in seconds.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Finding record, a statement synthesized from stories, with the evidence handles behind it.
/// </summary>
/// <param name="Topic">A short topic label.</param>
/// <param name="Statement">The statement.</param>
/// <param name="Sources">The evidence handles of the stories that support it.</param>
public record Finding(string Topic, string Statement, IReadOnlyList<string> Sources);

/// <summary>
/// SynthesisSource record, the provenance behind an evidence handle.
/// </summary>
/// <param name="Handle">The handle used in prompts ("S12").</param>
/// <param name="ContentId">The content.</param>
/// <param name="Headline">The headline.</param>
/// <param name="Url">The subscriber story page URL.</param>
/// <param name="Anchor">The story's anchor within the current report, when rendered.</param>
public record SynthesisSource(string Handle, long ContentId, string Headline, string? Url, string? Anchor = null)
{
    /// <summary>The preferred destination for automatic citations.</summary>
    public string? Link => this.Anchor ?? this.Url;
}

/// <summary>
/// SynthesisUsage record, what a synthesis cost.
/// </summary>
/// <param name="Requests">Requests sent.</param>
/// <param name="PromptTokens">Prompt tokens reported by the provider.</param>
/// <param name="CompletionTokens">Completion tokens reported by the provider.</param>
/// <param name="DurationMs">Elapsed time.</param>
/// <param name="StoriesProcessed">Stories whose text was sent to the model.</param>
/// <param name="ReductionDepth">The deepest reduction round.</param>
/// <param name="LargestRequestTokens">The largest estimated request.</param>
public record SynthesisUsage(int Requests, int PromptTokens, int CompletionTokens, long DurationMs, int StoriesProcessed, int ReductionDepth, int LargestRequestTokens);

/// <summary>
/// SynthesisResult record, the outcome of synthesizing one AI section.
/// </summary>
/// <param name="IsSuccess">Whether synthesis completed within its limits.</param>
/// <param name="Output">The section HTML; empty on failure.</param>
/// <param name="Error">Why synthesis failed.</param>
/// <param name="Findings">The final findings.</param>
/// <param name="Sources">The provenance of every handle.</param>
/// <param name="Usage">What it cost.</param>
public record SynthesisResult(bool IsSuccess, string Output, string? Error, IReadOnlyList<Finding> Findings, IReadOnlyList<SynthesisSource> Sources, SynthesisUsage Usage);

/// <summary>
/// SynthesisException class, synthesis could not complete within its limits.
/// </summary>
public class SynthesisException : Exception
{
    /// <summary>
    /// Creates a new instance of a SynthesisException.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public SynthesisException(string message, Exception? innerException = null) : base(message, innerException) { }
}
