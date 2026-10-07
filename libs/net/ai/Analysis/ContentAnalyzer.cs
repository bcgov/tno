using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TNO.AI.Text;
using TNO.AI.Tokens;
using TNO.Core.Extensions;

namespace TNO.AI.Analysis;

/// <summary>
/// AnalyzerOptions class, configuration for content analysis.
/// </summary>
public class AnalyzerOptions
{
    /// <summary>get/set - Percentage of the context window held back for estimation error.</summary>
    public int SafetyMarginPercent { get; set; } = 10;

    /// <summary>get/set - Tokens of trailing context repeated at the start of the next chunk.</summary>
    public int OverlapTokens { get; set; } = 150;

    /// <summary>get/set - Text shorter than this is not usable; only metadata is recorded.</summary>
    public int MinTextCharacters { get; set; } = 40;

    /// <summary>get/set - Attempts per request for throttling and transient failures.</summary>
    public int RequestAttempts { get; set; } = 3;

    /// <summary>get/set - The most rounds condensing facts before the summary.</summary>
    public int MaxReductionDepth { get; set; } = 4;

    /// <summary>get/set - Return key facts, people and organizations, places, events, and topics.</summary>
    public bool ExtractMetadata { get; set; } = true;

    /// <summary>get/set - Write a summary.</summary>
    public bool Summarize { get; set; } = true;

    /// <summary>get/set - Extract verbatim quotes.</summary>
    public bool ExtractQuotes { get; set; } = true;

    /// <summary>get/set - Suggest tags from the tags provided.</summary>
    public bool SuggestTags { get; set; } = true;

    /// <summary>get/set - Suggest the contributor from the byline or a columnist named in the text.</summary>
    public bool SuggestContributor { get; set; } = true;

    /// <summary>get/set - Choose the primary topic.</summary>
    public bool ChooseTopic { get; set; } = true;

    /// <summary>get - Whether any configured process needs the model.</summary>
    public bool UsesModel => this.ExtractMetadata || this.Summarize || this.ExtractQuotes || this.SuggestTags || this.ChooseTopic;
}

/// <summary>
/// ContentAnalyzer class, extracts structured information from one content item:
/// 1. normalize the text (HTML to plain text), keeping offsets into the normalized text;
/// 2. split oversized text on paragraph/sentence boundaries within the model budget, overlapping
///    only for boundary context; never truncate;
/// 3. extract facts, entities, places, topics, events, and quotes from every chunk;
/// 4. validate: quotes must appear verbatim in the text (their offsets are recorded), fact spans are
///    located when possible, and invalid items are dropped and counted;
/// 5. merge duplicates (including overlap duplicates), keeping ambiguous identities separate;
/// 6. produce a bounded summary from the validated facts.
/// Authoritative metadata (source, dates, byline) comes from the content, not the model.
/// </summary>
public partial class ContentAnalyzer
{
    #region Variables
    /// <summary>The text normalization version.</summary>
    public const string NormalizationVersion = "1";
    /// <summary>The output schema version.</summary>
    public const string SchemaVersion = "1";
    /// <summary>The prompt version.</summary>
    public const string PromptVersion = "1";

    [GeneratedRegex(@"^\s*```(?:json)?\s*|\s*```\s*$")]
    private static partial Regex CodeFenceRegex();

    [GeneratedRegex(@"^\s*by\s+", RegexOptions.IgnoreCase)]
    private static partial Regex BylinePrefixRegex();

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex NonWordRegex();

    private readonly ILlmClient _client;
    private readonly AnalyzerOptions _options;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalyzer.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentAnalyzer(ILlmClient client, AnalyzerOptions options, ILogger logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// The text analysis reads, whose offsets quotes and facts refer to.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static string NormalizeText(string? body) => body.HtmlToPlainText();

    /// <summary>
    /// Analyze the content.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="endpoint"></param>
    /// <param name="limits"></param>
    /// <param name="tags">Tags analysis may suggest.</param>
    /// <param name="beforeRequest">Called with each request's estimated tokens before it is sent (rate limiting).</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="LlmConfigurationException">The LLM limits are not configured, or leave no room for content.</exception>
    public async Task<AnalyzerResult> AnalyzeAsync(
        AnalyzerInput input,
        LlmEndpoint endpoint,
        LlmLimits limits,
        IReadOnlyList<AnalyzerTag> tags,
        Func<int, CancellationToken, Task>? beforeRequest = null,
        CancellationToken cancellationToken = default)
    {
        var contributor = _options.SuggestContributor ? SuggestContributor(input.Byline) : null;
        var text = NormalizeText(input.Body);
        if (text.Trim().Length < _options.MinTextCharacters || !_options.UsesModel)
            return new AnalyzerResult() { IsMetadataOnly = text.Trim().Length < _options.MinTextCharacters, SuggestedContributor = contributor };
        if (!limits.IsValid)
            throw new LlmConfigurationException("The LLM has no context window or output limit configured.");

        var run = new Run(this, endpoint, limits, TokenEstimator.Create(limits.TokenEstimation), beforeRequest);
        var header = BuildHeader(input);
        var system = AnalysisPrompts.Extract;
        var chunkBudget = limits.GetInputAllowance(run.Estimator.Count(system), 2, _options.SafetyMarginPercent) - run.Estimator.Count(header) - 32;
        if (chunkBudget < 64) throw new LlmConfigurationException("The LLM's context window is too small to analyze content.");

        var chunks = TextChunker.Split(text, chunkBudget, run.Estimator, Math.Min(_options.OverlapTokens, chunkBudget / 4));
        var extractions = new List<(TextChunk Chunk, JsonElement Json)>();
        foreach (var chunk in chunks)
            extractions.AddRange(await ExtractAsync(run, header, system, text, chunk, chunks.Count, cancellationToken));

        var validation = new Dictionary<string, object?>
        {
            ["chunks"] = chunks.Count,
            ["droppedQuotes"] = 0,
            ["unlocatedFacts"] = 0,
            ["droppedItems"] = 0,
        };
        var facts = new List<ExtractedFact>();
        var entities = new List<ExtractedEntity>();
        var places = new List<ExtractedPlace>();
        var topics = new List<ExtractedTopic>();
        var events = new List<ExtractedEvent>();
        var quotes = new List<ExtractedQuote>();
        string? columnist = null;

        foreach (var (chunk, json) in extractions)
        {
            foreach (var item in _options.ExtractQuotes ? Items(json, "quotes") : Enumerable.Empty<JsonElement>())
            {
                var statement = Str(item, "text") ?? Str(item, "statement");
                if (String.IsNullOrWhiteSpace(statement)) { Count(validation, "droppedItems"); continue; }
                var span = FindVerbatim(text, statement, chunk.Start, chunk.Length) ?? FindVerbatim(text, statement, 0, text.Length);
                if (span == null) { Count(validation, "droppedQuotes"); continue; }
                quotes.Add(new ExtractedQuote(text.Substring(span.Start, span.Length), Str(item, "speaker")?.Trim() ?? "", span));
            }
            foreach (var item in Items(json, "facts"))
            {
                var statement = Str(item, "statement");
                if (String.IsNullOrWhiteSpace(statement)) { Count(validation, "droppedItems"); continue; }
                var evidence = Str(item, "evidence");
                var span = String.IsNullOrWhiteSpace(evidence) ? null : FindVerbatim(text, evidence, chunk.Start, chunk.Length) ?? FindVerbatim(text, evidence, 0, text.Length);
                var inferred = Flag(item, "inferred");
                if (span == null && !inferred) Count(validation, "unlocatedFacts");
                facts.Add(new ExtractedFact(statement.Trim(), span, inferred));
            }
            foreach (var item in Items(json, "entities"))
            {
                var name = Str(item, "name");
                if (String.IsNullOrWhiteSpace(name)) { Count(validation, "droppedItems"); continue; }
                var type = (Str(item, "type") ?? "").Trim().ToLowerInvariant();
                entities.Add(new ExtractedEntity(type == "organization" || type == "organisation" ? "organization" : "person", name.Trim(), Strs(item, "aliases"), Strs(item, "roles"), Flag(item, "ambiguous")));
            }
            foreach (var item in Items(json, "places"))
            {
                var name = Str(item, "name");
                if (!String.IsNullOrWhiteSpace(name)) places.Add(new ExtractedPlace(name.Trim(), Str(item, "role")?.Trim() ?? ""));
            }
            foreach (var item in Items(json, "topics"))
            {
                var label = Str(item, "label");
                if (!String.IsNullOrWhiteSpace(label)) topics.Add(new ExtractedTopic(label.Trim(), Math.Clamp(Num(item, "relevance") ?? 0.5, 0, 1)));
            }
            foreach (var item in Items(json, "events"))
            {
                var actor = Str(item, "actor");
                var action = Str(item, "action");
                if (String.IsNullOrWhiteSpace(actor) || String.IsNullOrWhiteSpace(action)) continue;
                // Missing dates and locations stay empty rather than being invented.
                events.Add(new ExtractedEvent(actor.Trim(), action.Trim(), Blank(Str(item, "date")), Blank(Str(item, "location"))));
            }
            columnist ??= Blank(Str(json, "columnist"));
        }

        var mergedFacts = facts.GroupBy(f => Key(f.Statement)).Select(g => g.OrderByDescending(f => f.Span != null).First()).ToList();
        var mergedTopics = topics.GroupBy(t => Key(t.Label)).Select(g => g.OrderByDescending(t => t.Relevance).First() with { Relevance = g.Max(t => t.Relevance) })
            .OrderByDescending(t => t.Relevance).ToArray();
        // The summary request also chooses the primary topic and suggests tags.
        var (summary, primaryTopic, suggestedTags) = _options.Summarize || _options.SuggestTags || _options.ChooseTopic
            ? await SummarizeAsync(run, input, mergedFacts, mergedTopics, _options.SuggestTags ? tags : Array.Empty<AnalyzerTag>(), cancellationToken)
            : ("", null, Array.Empty<string>());
        var metadata = _options.ExtractMetadata;

        return new AnalyzerResult()
        {
            Summary = _options.Summarize ? summary : "",
            KeyFacts = metadata ? mergedFacts : Array.Empty<ExtractedFact>(),
            Entities = metadata ? MergeEntities(entities) : Array.Empty<ExtractedEntity>(),
            Places = metadata ? places.GroupBy(p => Key(p.Name)).Select(g => g.First()).ToArray() : Array.Empty<ExtractedPlace>(),
            Topics = metadata ? mergedTopics : Array.Empty<ExtractedTopic>(),
            PrimaryTopic = _options.ChooseTopic ? primaryTopic ?? mergedTopics.FirstOrDefault()?.Label : null,
            SuggestedTags = _options.SuggestTags ? suggestedTags : Array.Empty<string>(),
            SuggestedContributor = _options.SuggestContributor ? contributor ?? columnist : null,
            Events = metadata ? events.GroupBy(e => (Key(e.Actor), Key(e.Action))).Select(g => g.First()).ToArray() : Array.Empty<ExtractedEvent>(),
            Quotes = quotes.GroupBy(q => Key(q.Statement)).Select(g => g.OrderBy(q => q.Span.Start).First()).OrderBy(q => q.Span.Start).ToArray(),
            Validation = validation,
            PromptTokens = run.PromptTokens,
            CompletionTokens = run.CompletionTokens,
        };
    }

    /// <summary>
    /// Extract from one chunk. A chunk the provider rejects as too long, or whose output is
    /// truncated, is split and each part extracted.
    /// </summary>
    private async Task<IEnumerable<(TextChunk, JsonElement)>> ExtractAsync(Run run, string header, string system, string text, TextChunk chunk, int chunkCount, CancellationToken cancellationToken)
    {
        var user = $"{header}\n{(chunkCount > 1 ? $"(part {chunk.Index + 1} of {chunkCount}; the beginning may repeat the end of the previous part)\n" : "")}---\n{chunk.Text}";
        string problem;
        try
        {
            var result = await run.SendAsync(new[] { ("system", system), ("user", user) }, true, cancellationToken);
            if (!result.IsTruncated)
            {
                var json = ParseJson(result.Content);
                if (json.HasValue) return new[] { (chunk, json.Value) };
                problem = "the response was not valid JSON";
            }
            else problem = "the response was truncated";
        }
        catch (LlmContextLengthException)
        {
            problem = "the request exceeded the context window";
        }

        var parts = TextChunker.Split(chunk.Text, Math.Max(32, run.Estimator.Count(chunk.Text) / 2), run.Estimator);
        if (parts.Count < 2) throw new InvalidOperationException($"Analysis failed because {problem}, and the text cannot be split further.");
        _logger.LogDebug("Splitting an analysis chunk because {problem}", problem);
        var results = new List<(TextChunk, JsonElement)>();
        foreach (var part in parts)
        {
            var sub = new TextChunk(chunk.Index, chunk.Start + part.Start, part.Length, part.Text);
            results.AddRange(await ExtractAsync(run, header, system, text, sub, chunkCount, cancellationToken));
        }
        return results;
    }

    /// <summary>
    /// A bounded summary from the validated facts, with the primary topic and suggested tags.
    /// Facts too many for one request are condensed first.
    /// </summary>
    private async Task<(string Summary, string? PrimaryTopic, IReadOnlyList<string> Tags)> SummarizeAsync(
        Run run, AnalyzerInput input, IReadOnlyList<ExtractedFact> facts, IReadOnlyList<ExtractedTopic> topics, IReadOnlyList<AnalyzerTag> tags, CancellationToken cancellationToken)
    {
        if (facts.Count == 0) return ("", topics.FirstOrDefault()?.Label, Array.Empty<string>());

        var system = AnalysisPrompts.Summarize;
        var tagList = String.Join("\n", tags.Select(t => $"{t.Code}: {t.Name}"));
        var topicList = String.Join(", ", topics.Take(10).Select(t => t.Label));
        var fixedText = $"{BuildHeader(input)}\nTopics found: {topicList}\n";
        var allowance = run.Limits.GetInputAllowance(run.Estimator.Count(system) + run.Estimator.Count(fixedText), 2, _options.SafetyMarginPercent);

        // The tag list is configuration, not content; leave it out when it would crowd out the facts.
        var tagTokens = run.Estimator.Count(tagList);
        var includeTags = tags.Count > 0 && tagTokens < allowance / 3;
        if (includeTags) allowance -= tagTokens;

        var statements = facts.Select(f => f.Statement).ToList();
        for (var depth = 0; run.Estimator.Count(String.Join("\n", statements)) > allowance; depth++)
        {
            if (depth >= _options.MaxReductionDepth) throw new InvalidOperationException("Analysis could not condense the facts for the summary.");
            statements = await CondenseAsync(run, statements, allowance, cancellationToken);
        }

        var user = new StringBuilder(fixedText)
            .AppendLine("Facts:")
            .AppendLine(String.Join("\n", statements.Select(s => $"- {s}")));
        if (includeTags) user.AppendLine("Tags you may choose from (code: name):").AppendLine(tagList);

        var result = await run.SendAsync(new[] { ("system", system), ("user", user.ToString()) }, true, cancellationToken);
        var json = ParseJson(result.Content) ?? throw new InvalidOperationException("The summary response was not valid JSON.");
        var codes = includeTags
            ? Strs(json, "tags").Where(code => tags.Any(t => String.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase))).ToArray()
            : Array.Empty<string>();
        return (Str(json, "summary")?.Trim() ?? "", Blank(Str(json, "primaryTopic")), codes);
    }

    /// <summary>
    /// Condense facts in batches that fit, halving them at most.
    /// </summary>
    private async Task<List<string>> CondenseAsync(Run run, List<string> statements, int allowance, CancellationToken cancellationToken)
    {
        var condensed = new List<string>();
        var batch = new List<string>();
        var tokens = 0;
        async Task FlushAsync()
        {
            if (batch.Count == 0) return;
            var result = await run.SendAsync(new[] { ("system", AnalysisPrompts.Condense), ("user", String.Join("\n", batch.Select(s => $"- {s}"))) }, true, cancellationToken);
            var json = ParseJson(result.Content);
            var output = json.HasValue ? Strs(json.Value, "facts") : Array.Empty<string>();
            condensed.AddRange(output.Count > 0 && output.Count < batch.Count ? output : batch.Take(Math.Max(1, batch.Count / 2)));
            batch.Clear();
            tokens = 0;
        }
        foreach (var statement in statements)
        {
            var size = run.Estimator.Count(statement) + 2;
            if (batch.Count > 0 && tokens + size > allowance) await FlushAsync();
            batch.Add(statement);
            tokens += size;
        }
        await FlushAsync();
        return condensed;
    }

    /// <summary>
    /// Merge entities that share a normalized name or alias; ambiguous identities stay separate.
    /// </summary>
    private static IReadOnlyList<ExtractedEntity> MergeEntities(IEnumerable<ExtractedEntity> entities)
    {
        var merged = new List<ExtractedEntity>();
        foreach (var entity in entities)
        {
            var keys = entity.Aliases.Append(entity.Name).Select(Key).Where(k => k.Length > 0).ToHashSet();
            var match = entity.IsAmbiguous ? null : merged.FirstOrDefault(m => !m.IsAmbiguous && m.Type == entity.Type
                && m.Aliases.Append(m.Name).Select(Key).Any(keys.Contains));
            if (match == null)
            {
                merged.Add(entity);
                continue;
            }
            var index = merged.IndexOf(match);
            merged[index] = match with
            {
                Aliases = match.Aliases.Concat(entity.Aliases).Append(entity.Name).Where(a => Key(a) != Key(match.Name)).DistinctBy(Key).ToArray(),
                Roles = match.Roles.Concat(entity.Roles).DistinctBy(Key).ToArray(),
            };
        }
        return merged;
    }

    /// <summary>
    /// Find the needle verbatim in the text (ignoring differences in whitespace and quote marks),
    /// searching the specified region.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="needle"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns>The span in the text, or null when it does not appear.</returns>
    public static TextSpan? FindVerbatim(string text, string needle, int start, int length)
    {
        var words = needle.Trim().Trim('"', '“', '”').Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return null;
        var pattern = String.Join(@"\s+", words.Select(w => Regex.Escape(w)
            .Replace("\"", "[\"“”]").Replace("“", "[\"“”]").Replace("”", "[\"“”]")
            .Replace("'", "['‘’]").Replace("’", "['‘’]").Replace("‘", "['‘’]")));
        start = Math.Clamp(start, 0, text.Length);
        length = Math.Clamp(length, 0, text.Length - start);
        var match = Regex.Match(text.Substring(start, length), pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        return match.Success ? new TextSpan(start + match.Index, match.Length) : null;
    }

    /// <summary>
    /// The contributor the byline names ("By Jane Doe" is "Jane Doe").
    /// </summary>
    /// <param name="byline"></param>
    /// <returns></returns>
    public static string? SuggestContributor(string? byline)
    {
        var value = BylinePrefixRegex().Replace(byline ?? "", "").Trim();
        return value.Length == 0 ? null : value;
    }

    private static string BuildHeader(AnalyzerInput input)
    {
        var parts = new[] { input.Source, input.MediaType, input.Series, input.PublishedOn?.ToString("yyyy-MM-dd"), String.IsNullOrWhiteSpace(input.Byline) ? null : $"By {input.Byline}" }
            .Where(p => !String.IsNullOrWhiteSpace(p));
        var header = $"Headline: {input.Headline}\n{String.Join(" | ", parts)}";
        return String.IsNullOrWhiteSpace(input.Summary) ? header : $"{header}\nEditor's summary: {input.Summary.HtmlToPlainText()}";
    }

    private static string Key(string? value) => NonWordRegex().Replace((value ?? "").ToLowerInvariant(), " ").Trim();

    private static string? Blank(string? value) => String.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Count(Dictionary<string, object?> validation, string key) => validation[key] = (int)(validation[key] ?? 0) + 1;

    private static JsonElement? ParseJson(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(CodeFenceRegex().Replace(content.Trim(), ""));
            return document.RootElement.ValueKind == JsonValueKind.Object ? document.RootElement.Clone() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IEnumerable<JsonElement> Items(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Object).ToArray()
            : System.Array.Empty<JsonElement>();

    private static string? Str(JsonElement element, string name)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static IReadOnlyList<string> Strs(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!.Trim()).Where(s => s.Length > 0).ToArray()
            : System.Array.Empty<string>();

    private static bool Flag(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    private static double? Num(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
    #endregion

    #region Classes
    /// <summary>
    /// The state of one analysis.
    /// </summary>
    private sealed class Run
    {
        private readonly ContentAnalyzer _analyzer;
        private readonly Func<int, CancellationToken, Task>? _beforeRequest;

        public Run(ContentAnalyzer analyzer, LlmEndpoint endpoint, LlmLimits limits, ITokenEstimator estimator, Func<int, CancellationToken, Task>? beforeRequest)
        {
            _analyzer = analyzer;
            this.Endpoint = endpoint;
            this.Limits = limits;
            this.Estimator = estimator;
            _beforeRequest = beforeRequest;
        }

        public LlmEndpoint Endpoint { get; }
        public LlmLimits Limits { get; }
        public ITokenEstimator Estimator { get; }
        public int PromptTokens { get; private set; }
        public int CompletionTokens { get; private set; }

        public async Task<LlmResult> SendAsync(IReadOnlyList<(string Role, string Content)> messages, bool jsonMode, CancellationToken cancellationToken)
        {
            var tokens = messages.Sum(m => this.Estimator.Count(m.Content)) + LlmLimits.RequestOverheadTokens + LlmLimits.MessageOverheadTokens * messages.Count;
            if (_beforeRequest != null) await _beforeRequest(tokens + this.Limits.MaxOutputTokens, cancellationToken);
            var result = await _analyzer._client.InvokeAsync(this.Endpoint, messages, jsonMode, _analyzer._options.RequestAttempts,
                new LlmRequestOptions(this.Limits.MaxOutputTokens), cancellationToken);
            this.PromptTokens += result.PromptTokens ?? 0;
            this.CompletionTokens += result.CompletionTokens ?? 0;
            return result;
        }
    }
    #endregion
}
