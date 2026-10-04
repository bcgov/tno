using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TNO.AI.Text;
using TNO.AI.Tokens;

namespace TNO.AI.Synthesis;

/// <summary>
/// ReportSynthesizer class, synthesizes an AI report section from every included story through
/// bounded, token-budgeted requests:
/// 1. stories, ordered by topic so related stories share a request, are sent in batches of at most
///    MaxBatchInputTokens (a story too large for one request is split into parts, never truncated),
///    and each batch returns findings citing evidence handles;
/// 2. findings are reduced recursively until they fit one request (at most MaxReductionDepth rounds);
/// 3. the output is assembled: topic summaries in application code, free text by one final request
///    whose input is itself bounded.
/// A request the provider rejects as too long, or whose output is truncated, is split and retried.
/// When synthesis cannot complete within its limits it fails rather than sending partial or
/// unbounded input.
/// </summary>
public partial class ReportSynthesizer
{
    #region Variables
    [GeneratedRegex(@"\[\s*((?:[SP][\d\-]+)(?:\s*[,;]\s*[SP][\d\-]+)*)\s*\]")]
    private static partial Regex CitationRegex();

    [GeneratedRegex(@"^\s*```(?:json)?\s*|\s*```\s*$")]
    private static partial Regex CodeFenceRegex();

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILlmClient _client;
    private readonly SynthesisOptions _options;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportSynthesizer.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ReportSynthesizer(ILlmClient client, SynthesisOptions options, ILogger logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Synthesize the section.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SynthesisResult> SynthesizeAsync(SynthesisRequest request, CancellationToken cancellationToken = default)
    {
        var run = new Run(request, TokenEstimator.Create(request.Limits.TokenEstimation), _options);
        try
        {
            if (!request.Limits.IsValid)
                throw new SynthesisException("The LLM has no context window or output limit configured, so its requests cannot be bounded.");

            // Handles for the current stories; the provenance stays out of the model context.
            for (var i = 0; i < request.Stories.Count; i++)
                run.AddSource($"S{i + 1}", request.Stories[i]);

            var instructions = ComposeInstructions(request);

            // Each previous instance is processed on its own, oldest first, and kept to its share.
            var history = new List<(string Label, IReadOnlyList<Finding> Findings)>();
            var historyShare = request.History.Count == 0 ? 0 : run.FinalAllowance(instructions) * 3 / 10 / request.History.Count;
            for (var i = 0; i < request.History.Count; i++)
            {
                var instance = request.History[i];
                var stories = instance.Stories.Select((story, n) => (Handle: $"P{i + 1}-{n + 1}", Story: story)).ToArray();
                var (previous, _) = await MapAsync(run, instructions, stories, cancellationToken);
                history.Add((instance.Label, await ReduceAsync(run, instructions, previous, historyShare, false, cancellationToken)));
            }

            // Every story is read in one pass. Stories are ordered by their analysis topic so related
            // stories share a request; mapping each topic on its own sent a request per story when
            // nearly every story had its own topic.
            var current = request.Stories
                .Select((story, i) => (Handle: $"S{i + 1}", Story: story))
                .OrderBy(s => String.IsNullOrWhiteSpace(s.Story.Group) ? 1 : 0)
                .ThenBy(s => s.Story.Group?.Trim() ?? "", StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var (mapped, batches) = await MapAsync(run, instructions, current, cancellationToken);
            // Findings from a single batch are already consolidated.
            var consolidate = request.Mode == SynthesisOutputMode.TopicSummary && batches > 1;
            var findings = await ReduceAsync(run, instructions, mapped, run.ReduceAllowance(instructions), consolidate, cancellationToken);

            string output;
            IReadOnlyList<Finding> final;
            if (request.Mode == SynthesisOutputMode.TopicSummary)
            {
                final = findings;
                output = RenderTopicSummary(run, findings);
            }
            else
            {
                (output, final) = await WriteFreeTextAsync(run, instructions, findings, history, cancellationToken);
            }

            return new SynthesisResult(true, output, null, final, run.Sources.Values.ToArray(), run.Usage());
        }
        catch (Exception ex) when (ex is SynthesisException || ex is HttpRequestException || ex is JsonException
            || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            _logger.LogWarning(ex, "Synthesis of section '{section}' failed", request.SectionLabel);
            return new SynthesisResult(false, "", ex.Message, Array.Empty<Finding>(), run.Sources.Values.ToArray(), run.Usage());
        }
    }

    /// <summary>
    /// The section's own prompts, which decide what matters.
    /// </summary>
    private static string ComposeInstructions(SynthesisRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Section: {request.SectionLabel}");
        if (!String.IsNullOrWhiteSpace(request.SystemPrompt)) builder.AppendLine(request.SystemPrompt.Trim());
        if (!String.IsNullOrWhiteSpace(request.UserPrompt)) builder.AppendLine(request.UserPrompt.Trim());
        return builder.ToString().Trim();
    }

    #region Map
    /// <summary>
    /// Send every story, in batches that fit the allowance, and collect their findings.
    /// </summary>
    private async Task<(List<Finding> Findings, int Batches)> MapAsync(Run run, string instructions, IReadOnlyList<(string Handle, SynthesisStory Story)> stories, CancellationToken cancellationToken)
    {
        if (stories.Count == 0) return (new List<Finding>(), 0);
        var system = SynthesisPrompts.Map(instructions);
        var allowance = run.BatchAllowance(system);
        if (allowance < 64) throw new SynthesisException("The section instructions leave no room for stories within the model's context window.");

        var items = new List<Item>();
        foreach (var (handle, story) in stories)
            items.AddRange(StoryItems(run, handle, story, allowance));

        var batches = Pack(items, allowance);
        var results = await RunAllAsync(run, batches, batch => ProcessAsync(run, system, batch, true, allowance, cancellationToken), cancellationToken);
        return (results.SelectMany(r => r).ToList(), batches.Count);
    }

    /// <summary>
    /// A story as one or more items, each within the allowance. Only a story too large for one
    /// request is split, on paragraph and sentence boundaries.
    /// </summary>
    private static IEnumerable<Item> StoryItems(Run run, string handle, SynthesisStory story, int allowance)
    {
        var header = $"[{handle}] {story.Headline}\n{story.Metadata}";
        var headerTokens = run.Estimator.Count(header) + 16;
        var text = String.IsNullOrWhiteSpace(story.Text) ? "(no text)" : story.Text;
        var whole = $"{header}\n{text}";
        var tokens = run.Estimator.Count(whole);
        if (tokens <= allowance) return new[] { new Item(whole, tokens, handle, new[] { handle }, text, $"[{handle}] {story.Headline}") };

        var chunks = TextChunker.Split(text, Math.Max(32, allowance - headerTokens), run.Estimator);
        return chunks.Select(chunk =>
        {
            var part = $"{header}\n(part {chunk.Index + 1} of {chunks.Count})\n{chunk.Text}";
            return new Item(part, run.Estimator.Count(part), handle, new[] { handle }, chunk.Text, $"[{handle}] {story.Headline}");
        });
    }
    #endregion

    #region Reduce
    /// <summary>
    /// Reduce findings until they fit within 'allowance' tokens. When 'consolidate' is set, the
    /// findings always pass through at least one single-request consolidation (for topic summaries,
    /// whose output is assembled from them).
    /// </summary>
    private async Task<List<Finding>> ReduceAsync(Run run, string instructions, List<Finding> findings, int allowance, bool consolidate, CancellationToken cancellationToken)
    {
        var system = SynthesisPrompts.Reduce(instructions);
        var requestAllowance = run.InputAllowance(system);
        var batchAllowance = run.BatchAllowance(system);
        var target = Math.Min(allowance, requestAllowance);
        var consolidated = !consolidate;

        for (var depth = 1; ; depth++)
        {
            if (findings.Count == 0) return findings;
            var tokens = findings.Select((f, i) => run.Estimator.Count(FormatFinding(f, $"F{i + 1}")) + 1).Sum();
            if (tokens <= target && consolidated) return findings;
            if (depth > _options.MaxReductionDepth)
                throw new SynthesisException($"Synthesis could not reduce the findings to fit within {_options.MaxReductionDepth} rounds.");
            run.RecordDepth(depth);

            // Findings are cited by compact handles; the stories behind them stay out of the prompt.
            var items = findings.Select((f, i) =>
            {
                var line = FormatFinding(f, $"F{i + 1}");
                return new Item(line, run.Estimator.Count(line), $"F{i + 1}", f.Sources, null, null);
            }).ToList();
            var batches = Pack(items, batchAllowance);
            var results = await RunAllAsync(run, batches, batch => ProcessAsync(run, system, batch, false, batchAllowance, cancellationToken), cancellationToken);
            var reduced = results.SelectMany(r => r).ToList();

            // A single batch is a consolidation of every finding.
            if (batches.Count == 1) consolidated = true;
            _logger.LogDebug("Synthesis reduce round {depth}: {findings} finding(s) in {batches} request(s) became {reduced}", depth, findings.Count, batches.Count, reduced.Count);
            findings = reduced;
        }
    }
    #endregion

    #region Requests
    /// <summary>
    /// Run the batches with bounded concurrency, preserving order.
    /// </summary>
    private async Task<List<T>> RunAllAsync<T>(Run run, IReadOnlyList<List<Item>> batches, Func<List<Item>, Task<T>> process, CancellationToken cancellationToken)
    {
        using var throttle = new SemaphoreSlim(Math.Max(1, _options.MaxConcurrentRequests));
        var tasks = batches.Select(async batch =>
        {
            await throttle.WaitAsync(cancellationToken);
            try
            {
                return await process(batch);
            }
            finally
            {
                throttle.Release();
            }
        }).ToArray();
        return (await Task.WhenAll(tasks)).ToList();
    }

    /// <summary>
    /// Send one batch and parse its findings. A batch the provider rejects as too long, or whose
    /// output is truncated or unreadable, is split in two and each half retried; a single story part
    /// is split into smaller parts.
    /// </summary>
    private async Task<List<Finding>> ProcessAsync(Run run, string system, List<Item> batch, bool isMap, int allowance, CancellationToken cancellationToken)
    {
        var handles = batch.Select(i => i.Handle).ToHashSet();
        var input = String.Join("\n\n", batch.Select(i => i.Text));
        string? problem;
        try
        {
            var result = await SendAsync(run, new[] { ("system", system), ("user", input) }, true, null, null, run.BatchOutputTokens, cancellationToken);
            if (!result.IsTruncated)
            {
                var findings = ParseFindings(result.Content, handles);
                if (findings != null)
                {
                    // Expand cited handles to the stories behind them.
                    var sources = batch.GroupBy(i => i.Handle).ToDictionary(g => g.Key, g => g.SelectMany(i => i.Sources).Distinct().ToArray());
                    findings = findings.Select(f => f with { Sources = f.Sources.SelectMany(h => sources[h]).Distinct().ToArray() }).ToList();
                    if (isMap) run.RecordProcessed(batch.Select(i => i.Handle));
                    return findings;
                }
                problem = "the response was not valid findings";
            }
            else problem = "the response was truncated at the output limit";
        }
        catch (LlmContextLengthException)
        {
            problem = "the request exceeded the context window";
        }

        _logger.LogDebug("Splitting a synthesis batch of {count} item(s) because {problem}", batch.Count, problem);
        if (batch.Count > 1)
        {
            var half = batch.Count / 2;
            var first = await ProcessAsync(run, system, batch.Take(half).ToList(), isMap, allowance, cancellationToken);
            var second = await ProcessAsync(run, system, batch.Skip(half).ToList(), isMap, allowance, cancellationToken);
            return first.Concat(second).ToList();
        }

        var item = batch[0];
        if (isMap && item.StoryText != null && item.Tokens > 64)
        {
            // Split the story part into smaller parts; each still carries the story's handle.
            var header = item.StoryHeader;
            var parts = TextChunker.Split(item.StoryText, Math.Max(32, item.Tokens / 2), run.Estimator);
            if (parts.Count > 1)
            {
                var smaller = parts.Select(p =>
                {
                    var text = $"{header}\n(continued, part {p.Index + 1} of {parts.Count})\n{p.Text}";
                    return new Item(text, run.Estimator.Count(text), item.Handle, item.Sources, p.Text, header);
                }).ToList();
                return await ProcessAsync(run, system, smaller, isMap, allowance, cancellationToken);
            }
        }
        throw new SynthesisException($"Synthesis failed because {problem}, and the input cannot be split further.");
    }

    /// <summary>
    /// Send one request within the deployment's rate limits, recording its size and usage.
    /// </summary>
    private async Task<LlmResult> SendAsync(Run run, IReadOnlyList<(string Role, string Content)> messages, bool jsonMode, float? temperature, int? choiceCount, int maxOutputTokens, CancellationToken cancellationToken)
    {
        var tokens = messages.Sum(m => run.Estimator.Count(m.Content)) + LlmLimits.RequestOverheadTokens + LlmLimits.MessageOverheadTokens * messages.Count;
        run.RecordRequestSize(tokens);
        var limits = run.Request.Limits;
        await LlmRateLimiter.WaitAsync(LlmRateLimiter.GetKey(run.Request.Endpoint), limits.RequestsPerMinute, limits.TokensPerMinute, tokens + maxOutputTokens, cancellationToken);
        var result = await _client.InvokeAsync(
            run.Request.Endpoint,
            messages,
            jsonMode,
            _options.RequestAttempts,
            new LlmRequestOptions(maxOutputTokens, temperature, choiceCount),
            cancellationToken);
        run.RecordUsage(result);
        return result;
    }
    #endregion

    #region Output
    /// <summary>
    /// Write the free-text section from the findings with one final request whose input fits.
    /// </summary>
    private async Task<(string Output, IReadOnlyList<Finding> Findings)> WriteFreeTextAsync(
        Run run,
        string instructions,
        List<Finding> findings,
        IReadOnlyList<(string Label, IReadOnlyList<Finding> Findings)> history,
        CancellationToken cancellationToken)
    {
        var request = run.Request;
        var system = String.IsNullOrWhiteSpace(request.SystemPrompt) ? $"You write the '{request.SectionLabel}' section of a media monitoring report." : request.SystemPrompt!;
        var historyMessages = history
            .Where(h => h.Findings.Count > 0)
            .Select(h => ("user", $"## {h.Label} (previous report)\n{String.Join("\n", h.Findings.Select(f => $"- {f.Statement}"))}"))
            .ToArray();

        var historyTokens = historyMessages.Sum(m => run.Estimator.Count(m.Item2));
        var instructionTokens = run.Estimator.Count(system) + run.Estimator.Count(SynthesisPrompts.FinalCitationRule) + run.Estimator.Count(request.UserPrompt);
        var allowance = request.Limits.GetInputAllowance(instructionTokens, 2 + historyMessages.Length, _options.SafetyMarginPercent) - historyTokens;
        if (allowance < 64) throw new SynthesisException("The section instructions and previous reports leave no room for findings within the model's context window.");

        for (var round = 0; ; round++)
        {
            var findingsText = String.Join("\n", findings.Select(FormatFinalFinding));
            var findingsTokens = run.Estimator.Count(findingsText);
            var messages = new List<(string Role, string Content)> { ("system", system) };
            messages.AddRange(historyMessages);
            messages.Add(("user", $"{SynthesisPrompts.FinalCitationRule}\n\n## Findings for this report\n{findingsText}\n\n{request.UserPrompt}"));

            string problem;
            if (findingsTokens <= allowance)
            {
                try
                {
                    var result = await SendAsync(run, messages, false, request.Temperature, request.ChoiceCount, request.Limits.MaxOutputTokens, cancellationToken);
                    if (!result.IsTruncated) return (Sanitize(LinkCitations(run, SelectChoice(result, request.ChoiceIndex))), findings);
                    problem = "the section output was truncated at the output limit";
                }
                catch (LlmContextLengthException)
                {
                    problem = "the final request exceeded the context window";
                }
            }
            else problem = "the findings do not fit one request";

            // Consolidate further and try again, within the depth limit.
            if (round >= _options.MaxReductionDepth)
                throw new SynthesisException($"Synthesis failed because {problem} after {_options.MaxReductionDepth} rounds.");
            var target = findingsTokens <= allowance ? findingsTokens * 2 / 3 : allowance;
            findings = await ReduceAsync(run, instructions, findings, Math.Max(64, target), true, cancellationToken);
        }
    }

    /// <summary>
    /// The choice to use; ChoiceIndex -1 joins every choice.
    /// </summary>
    private static string SelectChoice(LlmResult result, int? choiceIndex)
    {
        var choices = result.Choices.Count > 0 ? result.Choices : new[] { result.Content };
        if (choiceIndex == -1)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < choices.Count; i++)
            {
                builder.AppendLine($"## Choice {i + 1}");
                builder.AppendLine(choices[i]);
            }
            return builder.ToString();
        }
        var index = choiceIndex ?? 0;
        return index >= 0 && index < choices.Count ? choices[index] : choices[0];
    }

    /// <summary>
    /// Replace evidence handles with links to their stories. Unknown handles are removed.
    /// </summary>
    private static string LinkCitations(Run run, string text)
    {
        return CitationRegex().Replace(text, match =>
        {
            var links = match.Groups[1].Value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(run.Sources.ContainsKey)
                .Select(handle => SourceLink(run.Sources[handle]))
                // Stories with the same headline and link (e.g. wire updates) are listed once.
                .Distinct()
                .ToArray();
            return links.Length == 0 ? "" : $" ({String.Join("; ", links)})";
        });
    }

    /// <summary>
    /// Headings with bullet statements and source lists, built from recorded provenance.
    /// </summary>
    private static string RenderTopicSummary(Run run, IReadOnlyList<Finding> findings)
    {
        var sections = findings
            .Select(f => (Heading: f.Topic, Finding: f))
            .GroupBy(x => String.IsNullOrWhiteSpace(x.Heading) ? "Other" : x.Heading.Trim(), StringComparer.OrdinalIgnoreCase);

        var html = new StringBuilder("<div class=\"ai-topic-summary\">");
        foreach (var section in sections)
        {
            html.Append($"<h3>{WebUtility.HtmlEncode(section.Key)}</h3><ul>");
            foreach (var (_, finding) in section)
            {
                var sources = finding.Sources.Where(run.Sources.ContainsKey).Distinct().Select(h => SourceLink(run.Sources[h])).ToArray();
                html.Append($"<li>{WebUtility.HtmlEncode(finding.Statement)}");
                if (sources.Length > 0) html.Append($" <span class=\"ai-sources\">({String.Join("; ", sources)})</span>");
                html.Append("</li>");
            }
            html.Append("</ul>");
        }
        html.Append("</div>");
        return html.ToString();
    }

    private static string SourceLink(SynthesisSource source)
    {
        var headline = WebUtility.HtmlEncode(source.Headline);
        return String.IsNullOrWhiteSpace(source.Link) ? headline : $"<a href=\"{WebUtility.HtmlEncode(source.Link)}\">{headline}</a>";
    }

    private static readonly Lazy<Ganss.Xss.HtmlSanitizer> _sanitizer = new(() =>
    {
        var sanitizer = new Ganss.Xss.HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        return sanitizer;
    });

    /// <summary>
    /// Remove scripts, event handlers, and anything else unsafe from model-written HTML.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string Sanitize(string html) => _sanitizer.Value.Sanitize(html);
    #endregion

    #region Findings
    /// <summary>
    /// A finding as one reduce-prompt line, cited by its compact handle.
    /// </summary>
    private static string FormatFinding(Finding finding, string handle)
        => $"[{handle}] ({finding.Topic}) {finding.Statement}";

    /// <summary>
    /// The most story handles shown per finding in the final request.
    /// </summary>
    private const int MaxFinalCitations = 3;

    /// <summary>
    /// A finding as one final-prompt line, with a few of its story handles to cite.
    /// </summary>
    private static string FormatFinalFinding(Finding finding)
        => $"- {finding.Statement} {String.Concat(finding.Sources.Take(MaxFinalCitations).Select(h => $"[{h}]"))}";

    /// <summary>
    /// Parse the findings a response returns. Handles not in the request are dropped, and so are
    /// findings left without a source. Returns null when the response is not findings JSON.
    /// </summary>
    public static List<Finding>? ParseFindings(string content, IReadOnlySet<string> allowedHandles)
    {
        try
        {
            using var document = JsonDocument.Parse(CodeFenceRegex().Replace(content.Trim(), ""));
            if (!document.RootElement.TryGetProperty("findings", out var items) || items.ValueKind != JsonValueKind.Array) return null;
            var findings = new List<Finding>();
            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                var statement = item.TryGetProperty("statement", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString()?.Trim() : null;
                if (String.IsNullOrWhiteSpace(statement)) continue;
                var topic = item.TryGetProperty("topic", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString()?.Trim() ?? "" : "";
                var sources = item.TryGetProperty("sources", out var src) && src.ValueKind == JsonValueKind.Array
                    ? src.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!.Trim().Trim('[', ']')).Where(allowedHandles.Contains).Distinct().ToArray()
                    : Array.Empty<string>();
                if (sources.Length == 0) continue;
                findings.Add(new Finding(topic, statement, sources));
            }
            return findings;
        }
        catch (JsonException)
        {
            return null;
        }
    }
    #endregion

    #region Packing
    /// <summary>
    /// Pack items, in order, into batches that fit the allowance.
    /// </summary>
    private static List<List<Item>> Pack(IReadOnlyList<Item> items, int allowance)
    {
        var batches = new List<List<Item>>();
        var current = new List<Item>();
        var tokens = 0;
        foreach (var item in items)
        {
            if (current.Count > 0 && tokens + item.Tokens + 2 > allowance)
            {
                batches.Add(current);
                current = new List<Item>();
                tokens = 0;
            }
            current.Add(item);
            tokens += item.Tokens + 2;
        }
        if (current.Count > 0) batches.Add(current);
        return batches;
    }
    #endregion
    #endregion

    #region Classes
    /// <summary>
    /// One unit of request input: a story (or part of one) or a finding.
    /// </summary>
    /// <param name="Text">The prompt text.</param>
    /// <param name="Tokens">Its estimated size.</param>
    /// <param name="Handle">The handle the model cites for it: a story ("S12") or a finding ("F3").</param>
    /// <param name="Sources">The story handles it stands for; provenance the model never sees.</param>
    /// <param name="StoryText">For a story part, its text (to split it further).</param>
    /// <param name="StoryHeader">For a story part, its handle and headline.</param>
    private record Item(string Text, int Tokens, string Handle, IReadOnlyList<string> Sources, string? StoryText, string? StoryHeader);

    /// <summary>
    /// The state of one synthesis.
    /// </summary>
    private sealed class Run
    {
        private readonly object _lock = new();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly HashSet<string> _processed = new();
        private int _requests;
        private int _promptTokens;
        private int _completionTokens;
        private int _depth;
        private int _largestRequest;
        private readonly SynthesisOptions _options;

        public Run(SynthesisRequest request, ITokenEstimator estimator, SynthesisOptions options)
        {
            this.Request = request;
            this.Estimator = estimator;
            _options = options;
        }

        public SynthesisRequest Request { get; }
        public ITokenEstimator Estimator { get; }
        public Dictionary<string, SynthesisSource> Sources { get; } = new();

        public void AddSource(string handle, SynthesisStory story)
            => this.Sources[handle] = new SynthesisSource(handle, story.ContentId, story.Headline, story.Anchor ?? story.Url);

        /// <summary>
        /// Input tokens available beside the specified system prompt, in a two-message request.
        /// </summary>
        public int InputAllowance(string system) => this.Request.Limits.GetInputAllowance(this.Estimator.Count(system), 2, _options.SafetyMarginPercent);

        public int ReduceAllowance(string instructions) => InputAllowance(SynthesisPrompts.Reduce(instructions));

        /// <summary>
        /// Input tokens for one map or reduce request: the request allowance, capped by MaxBatchInputTokens.
        /// </summary>
        public int BatchAllowance(string system)
            => _options.MaxBatchInputTokens > 0 ? Math.Min(InputAllowance(system), _options.MaxBatchInputTokens) : InputAllowance(system);

        /// <summary>
        /// Output tokens for one map or reduce request.
        /// </summary>
        public int BatchOutputTokens
            => _options.MaxBatchOutputTokens > 0 ? Math.Min(this.Request.Limits.MaxOutputTokens, _options.MaxBatchOutputTokens) : this.Request.Limits.MaxOutputTokens;

        public int FinalAllowance(string instructions)
            => this.Request.Limits.GetInputAllowance(
                this.Estimator.Count(this.Request.SystemPrompt) + this.Estimator.Count(SynthesisPrompts.FinalCitationRule) + this.Estimator.Count(this.Request.UserPrompt),
                2 + this.Request.History.Count,
                _options.SafetyMarginPercent);

        public void RecordProcessed(IEnumerable<string> handles)
        {
            lock (_lock) foreach (var handle in handles.Where(h => h.StartsWith('S'))) _processed.Add(handle);
        }

        public void RecordUsage(LlmResult result)
        {
            lock (_lock)
            {
                _requests++;
                _promptTokens += result.PromptTokens ?? 0;
                _completionTokens += result.CompletionTokens ?? 0;
            }
        }

        public void RecordDepth(int depth)
        {
            lock (_lock) _depth = Math.Max(_depth, depth);
        }

        public void RecordRequestSize(int tokens)
        {
            lock (_lock) _largestRequest = Math.Max(_largestRequest, tokens);
        }

        public SynthesisUsage Usage()
        {
            lock (_lock) return new SynthesisUsage(_requests, _promptTokens, _completionTokens, _stopwatch.ElapsedMilliseconds, _processed.Count, _depth, _largestRequest);
        }
    }
    #endregion
}
