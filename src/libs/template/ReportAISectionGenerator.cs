using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TNO.AI;
using TNO.AI.Synthesis;
using TNO.AI.Tokens;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.Core.Extensions;
using TNO.TemplateEngine.Config;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine;

/// <summary>
/// ReportAISectionGenerator class, produces a direct-model AI section through bounded synthesis,
/// generating each result once. It pins a manifest of everything the output depends on (stories
/// and their input, prompts, section settings, model, previous instances, pipeline version),
/// reuses a stored result for an unchanged manifest, and otherwise claims the manifest so no other
/// preview, view, or send generates it at the same time.
/// </summary>
public class ReportAISectionGenerator
{
    #region Variables
    /// <summary>
    /// The scope that feeds an AI section every content section of the report.
    /// </summary>
    public const string ScopeReport = "Report";

    /// <summary>
    /// The scope that feeds an AI section the content sections it names.
    /// </summary>
    public const string ScopeSections = "Sections";

    /// <summary>The allowed story fields. Keep aligned with the subscriber field picker.</summary>
    public static IReadOnlyList<string> AllowedInputFields { get; } = Array.AsReadOnly(new[]
    {
        "headline", "source", "mediaType", "series", "publishedOn", "byline", "contributor",
        "summary", "keyFacts", "entities", "quotes", "body",
    });

    /// <summary>Article text is opt-in, but remains a fallback when no summary is available.</summary>
    public static IReadOnlyList<string> DefaultInputFields { get; } = Array.AsReadOnly(
        AllowedInputFields.Where(f => f != "body").ToArray());

    private static readonly JsonSerializerOptions _manifestOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    private readonly IReportAIResultStore _store;
    private readonly ILlmClient _client;
    private readonly TemplateOptions _options;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAISectionGenerator.
    /// </summary>
    /// <param name="store"></param>
    /// <param name="client"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ReportAISectionGenerator(IReportAIResultStore store, ILlmClient client, TemplateOptions options, ILogger logger)
    {
        _store = store;
        _client = client;
        _options = options;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate (or reuse) the output of one AI section.
    /// </summary>
    /// <param name="report">The report.</param>
    /// <param name="reportInstanceId">The instance being generated, if any.</param>
    /// <param name="section">The AI section.</param>
    /// <param name="sectionContent">Every section of the report with its content.</param>
    /// <param name="previousReports">Previous instances, oldest first (already limited to this section's count).</param>
    /// <param name="llm">The section's LLM.</param>
    /// <param name="wait">Whether to generate a missing section, and wait for another generator's.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="viewContentUrlOverride">The requesting preview's link URL; empty means no links, null uses local configuration.</param>
    /// <returns>The section output, an error when the section cannot be generated, its status, and when the claim of the generator holding it expires.</returns>
    public async Task<(string? Output, string? Error, AISectionStatus Status, DateTime? ClaimExpiresOn)> GenerateAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        long? reportInstanceId,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        Dictionary<string, ReportSectionModel> sectionContent,
        IReadOnlyList<PreviousReportModel> previousReports,
        API.Areas.Services.Models.LLM.LLMModel llm,
        AISectionWait wait = AISectionWait.Wait,
        CancellationToken cancellationToken = default,
        string? viewContentUrlOverride = null)
    {
        var (request, prepareError) = Prepare(report, section, sectionContent, previousReports, llm, viewContentUrlOverride);
        if (request == null) return (null, prepareError, AISectionStatus.Failed, null);
        request = request with { ReportInstanceId = reportInstanceId };

        var manifest = BuildManifest(report, section, llm, request, previousReports);
        var hash = Hash(manifest);

        var waitingForGenerator = false;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existing = await _store.FindAsync(hash, cancellationToken);
            if (existing?.Status == Entities.ReportAIResultStatus.Completed)
            {
                _logger.LogDebug("Reusing AI result {id} for report {reportId} section {sectionId}", existing.Id, report.Id, section.Id);
                return (existing.Output, null, AISectionStatus.Ready, null);
            }

            // A sender that joined an in-progress generation accepts its terminal failure too.
            // Do not turn that completion into another generation attempt before delivery.
            if (waitingForGenerator && existing?.Status == Entities.ReportAIResultStatus.Failed)
                return (null, existing.Error ?? "The AI section failed to generate.", AISectionStatus.Failed, null);

            // A preview never generates; it shows the section once a generator has stored it. A failure
            // is shown rather than queued again, so a failing section cannot loop; a send retries it.
            if (wait == AISectionWait.NoWait)
            {
                if (existing?.Status == Entities.ReportAIResultStatus.Failed)
                    return (null, existing.Error ?? "The AI section failed to generate.", AISectionStatus.Failed, null);
                return existing?.Status == Entities.ReportAIResultStatus.Pending && existing.ClaimExpiresOn > DateTime.UtcNow
                    ? (null, null, AISectionStatus.Generating, existing.ClaimExpiresOn)
                    : (null, null, AISectionStatus.NotStarted, null);
            }

            var claim = await _store.TryClaimAsync(new ReportAIResultClaimModel()
            {
                Hash = hash,
                ReportId = report.Id,
                ReportInstanceId = reportInstanceId,
                ReportSectionId = section.Id,
                Manifest = manifest,
                PipelineVersion = SynthesisPrompts.PipelineVersion,
                LeaseSeconds = _options.Synthesis.ClaimLeaseSeconds,
            }, cancellationToken);

            if (claim != null)
            {
                var (output, error) = await SynthesizeAsync(claim.Id, request, cancellationToken);
                return (output, error, error == null ? AISectionStatus.Ready : AISectionStatus.Failed, null);
            }

            // Background preparation leaves a section another generator holds to that generator.
            if (wait == AISectionWait.Prepare) return (null, null, AISectionStatus.Generating, null);

            // A send must wait for a real result. Time spent waiting is not an AI failure and
            // must never allow a pending section to be sent as blank/error output.
            waitingForGenerator = true;
            await Task.Delay(PollInterval, cancellationToken);
        }
    }

    /// <summary>
    /// Run synthesis for a claimed result and store its outcome.
    /// </summary>
    private async Task<(string? Output, string? Error)> SynthesizeAsync(long resultId, SynthesisRequest request, CancellationToken cancellationToken)
    {
        SynthesisResult result;
        try
        {
            var synthesizer = new ReportSynthesizer(_client, _options.Synthesis, _logger);
            result = await synthesizer.SynthesizeAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            await _store.CompleteAsync(resultId, new ReportAIResultCompletionModel() { IsSuccess = false, Error = ex.Message }, CancellationToken.None);
            throw;
        }

        await _store.CompleteAsync(resultId, new ReportAIResultCompletionModel()
        {
            IsSuccess = result.IsSuccess,
            Output = result.Output,
            Error = result.Error,
            RequestCount = result.Usage.Requests,
            PromptTokens = result.Usage.PromptTokens,
            CompletionTokens = result.Usage.CompletionTokens,
            DurationMs = result.Usage.DurationMs,
            StoryCount = result.Usage.StoriesProcessed,
            ReductionDepth = result.Usage.ReductionDepth,
        }, CancellationToken.None);

        _logger.Log(result.IsSuccess ? LogLevel.Information : LogLevel.Warning,
            "AI section '{section}' {outcome} (report:{reportId}, instance:{instanceId}, section:{sectionId}): stories:{stories}, requests:{requests}, promptTokens:{promptTokens}, completionTokens:{completionTokens}, depth:{depth}, largestRequest:{largest}, durationMs:{duration}",
            request.SectionLabel, result.IsSuccess ? "generated" : $"failed ({result.Error})", request.ReportId, request.ReportInstanceId, request.ReportSectionId,
            result.Usage.StoriesProcessed, result.Usage.Requests,
            result.Usage.PromptTokens, result.Usage.CompletionTokens, result.Usage.ReductionDepth, result.Usage.LargestRequestTokens, result.Usage.DurationMs);

        return result.IsSuccess ? (result.Output, null) : (null, result.Error);
    }

    /// <summary>
    /// Validate the section and its LLM, and build the synthesis request: the stories in scope with
    /// the selected fields, the previous instances, every section of the report, and the model.
    /// </summary>
    /// <param name="report">The report.</param>
    /// <param name="section">The AI section (its settings may be unsaved drafts).</param>
    /// <param name="sectionContent">Every section of the report with its content.</param>
    /// <param name="previousReports">Previous instances, oldest first.</param>
    /// <param name="llm">The section's LLM.</param>
    /// <param name="viewContentUrlOverride">The requesting preview's link URL; empty means no links, null uses local configuration.</param>
    /// <returns>The request, or the reason the section cannot be generated.</returns>
    public (SynthesisRequest? Request, string? Error) Prepare(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        Dictionary<string, ReportSectionModel> sectionContent,
        IReadOnlyList<PreviousReportModel> previousReports,
        API.Areas.Services.Models.LLM.LLMModel llm,
        string? viewContentUrlOverride = null)
    {
        var settings = section.Settings;
        if (llm.ProjectEndpoint == null || String.IsNullOrWhiteSpace(llm.DeploymentName))
            return (null, "The LLM configuration requires a project endpoint and a deployment name.");
        var limits = new LlmLimits(llm.ContextWindow ?? 0, llm.MaxOutputTokens ?? 0, llm.TokenEstimation, llm.RequestsPerMinute, llm.TokensPerMinute);
        if (!limits.IsValid)
            return (null, $"The LLM '{llm.Name}' has no context window or maximum output tokens configured, so AI sections cannot use it.");
        var apiKey = llm.ApiKey;
        if (String.IsNullOrWhiteSpace(apiKey))
            return (null, $"The LLM '{llm.Name}' has no API key configured.");

        // Saved TopicSummary settings are legacy: every report section now follows its prompt.
        var mode = SynthesisOutputMode.FreeText;
        var fields = (settings.AIInputFields ?? DefaultInputFields.ToArray())
            .Where(f => AllowedInputFields.Contains(f)).Distinct().OrderBy(f => f).ToArray();
        if (fields.Length == 0)
            return (null, "Choose at least one data field for this AI section.");
        var sources = GetSourceSections(report, section, sectionContent);
        if (sources == null)
            return (null, "Choose the content sections that feed this AI section.");

        // Do not mutate shared options: concurrent preview jobs may have different link settings.
        var viewContentUrl = viewContentUrlOverride == null ? _options.ViewContentUrl
            : String.IsNullOrWhiteSpace(viewContentUrlOverride) ? null : new Uri(viewContentUrlOverride, UriKind.RelativeOrAbsolute);
        var stories = BuildStories(sources, viewContentUrl, true, fields, sectionContent.Values);
        var history = previousReports
            .Select(p => new SynthesisHistoricalInstance(
                p.PublishedOn.HasValue ? $"Report of {p.PublishedOn:yyyy-MM-dd}" : $"Report instance {p.InstanceId}",
                BuildStories(FilterSections(settings, p.Sections), viewContentUrl, false, fields, p.Sections.Values)))
            .ToArray();

        var request = new SynthesisRequest(
            settings.Label,
            settings.SystemPrompt,
            settings.UserPrompt ?? "",
            mode,
            stories,
            history,
            new LlmEndpoint(llm.ProjectEndpoint, apiKey, llm.DeploymentName),
            limits,
            settings.Temperature,
            settings.ChoiceQty,
            settings.ChoiceIndex,
            BuildSections(sectionContent.Values),
            report.Id,
            section.Id);
        return (request, null);
    }

    /// <summary>
    /// Run the section once, recording every request and response, without reading or storing a
    /// result. Used to try a prompt.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="section"></param>
    /// <param name="sectionContent"></param>
    /// <param name="previousReports"></param>
    /// <param name="llm"></param>
    /// <param name="viewContentUrlOverride"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ReportAISectionTestOutcome> TestAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        Dictionary<string, ReportSectionModel> sectionContent,
        IReadOnlyList<PreviousReportModel> previousReports,
        API.Areas.Services.Models.LLM.LLMModel llm,
        string? viewContentUrlOverride = null,
        CancellationToken cancellationToken = default)
    {
        var (request, error) = Prepare(report, section, sectionContent, previousReports, llm, viewContentUrlOverride);
        if (request == null) return new ReportAISectionTestOutcome(false, null, error, null, null);

        var synthesizer = new ReportSynthesizer(_client, _options.Synthesis, _logger);
        var transcript = new SynthesisTranscript() { MaxMessageChars = _options.Synthesis.TranscriptMaxMessageChars };
        var result = await synthesizer.SynthesizeAsync(request, transcript, cancellationToken);
        _logger.LogInformation(
            "AI section '{section}' {outcome} (test run): stories:{stories}, requests:{requests}, promptTokens:{promptTokens}, completionTokens:{completionTokens}, depth:{depth}, largestRequest:{largest}, durationMs:{duration}",
            request.SectionLabel, result.IsSuccess ? "generated" : $"failed ({result.Error})", result.Usage.StoriesProcessed, result.Usage.Requests,
            result.Usage.PromptTokens, result.Usage.CompletionTokens, result.Usage.ReductionDepth, result.Usage.LargestRequestTokens, result.Usage.DurationMs);
        return new ReportAISectionTestOutcome(result.IsSuccess, result.IsSuccess ? result.Output : null, result.Error, result.Usage, result.Transcript ?? transcript.Exchanges);
    }

    /// <summary>
    /// Describe the requests the section would send, without sending any.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="section"></param>
    /// <param name="sectionContent"></param>
    /// <param name="previousReports"></param>
    /// <param name="llm"></param>
    /// <param name="viewContentUrlOverride"></param>
    /// <returns>The planned exchanges, or the reason the section cannot be generated.</returns>
    public (IReadOnlyList<LlmExchange>? Plan, string? Error) Describe(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        Dictionary<string, ReportSectionModel> sectionContent,
        IReadOnlyList<PreviousReportModel> previousReports,
        API.Areas.Services.Models.LLM.LLMModel llm,
        string? viewContentUrlOverride = null)
    {
        var (request, error) = Prepare(report, section, sectionContent, previousReports, llm, viewContentUrlOverride);
        if (request == null) return (null, error);
        try
        {
            return (new ReportSynthesizer(_client, _options.Synthesis, _logger).Plan(request), null);
        }
        catch (SynthesisException ex)
        {
            return (null, ex.Message);
        }
    }

    /// <summary>
    /// An exchange as the API returns it.
    /// </summary>
    /// <param name="exchange"></param>
    /// <returns></returns>
    public static LlmExchangeModel ToModel(LlmExchange exchange) => new()
    {
        Stage = exchange.Stage,
        Kind = exchange.Kind,
        Sequence = exchange.Sequence,
        Messages = exchange.Messages.Select(m => new LlmMessageModel(m.Role, m.Content)).ToArray(),
        Response = exchange.Response,
        PromptTokens = exchange.PromptTokens,
        CompletionTokens = exchange.CompletionTokens,
        DurationMs = exchange.DurationMs,
        JsonMode = exchange.JsonMode,
        IsTruncated = exchange.IsTruncated,
        Error = exchange.Error,
        EstimatedTokens = exchange.EstimatedTokens,
    };

    /// <summary>
    /// Every section of the report, in order, with its anchor and the content it holds. A section's
    /// anchor exists only when the report template renders its heading: the section is enabled and
    /// is not an empty section hidden by its settings (image, data and AI sections are never hidden
    /// for being empty).
    /// </summary>
    /// <param name="sections"></param>
    /// <returns></returns>
    public static IReadOnlyList<SynthesisSection> BuildSections(IEnumerable<ReportSectionModel> sections)
        => sections.OrderBy(s => s.SortOrder).ThenBy(s => s.Id).Select(s => new SynthesisSection(
            s.Id,
            SectionLabel(s),
            IsSectionRendered(s) ? $"#{ReportEngine.SectionAnchorPrefix}{s.Id}" : null,
            s.Content.OrderBy(c => c.SortOrder).ThenBy(c => c.Id).Select(c => c.Id).Distinct().ToArray())).ToArray();

    /// <summary>
    /// The heading the model knows a section by.
    /// </summary>
    private static string SectionLabel(ReportSectionModel section)
        => String.IsNullOrWhiteSpace(section.Settings.Label) ? section.Name : section.Settings.Label.Trim();

    /// <summary>
    /// Whether the report template renders the section's heading (the same rule as the template).
    /// </summary>
    private static bool IsSectionRendered(ReportSectionModel section)
        => section.IsEnabled
            && !(section.Settings.HideEmpty && !section.Content.Any()
                && section.SectionType != Entities.ReportSectionType.Image
                && section.SectionType != Entities.ReportSectionType.Data
                && section.SectionType != Entities.ReportSectionType.AI);

    /// <summary>
    /// The content sections that feed the AI section: every content section for the report scope
    /// (the behaviour of sections saved before scopes existed), otherwise the named sections.
    /// Returns null when a section-scoped AI section names none.
    /// </summary>
    public static IReadOnlyList<ReportSectionModel>? GetSourceSections(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        Dictionary<string, ReportSectionModel> sectionContent)
    {
        var settings = section.Settings;
        if (String.Equals(settings.AIScope, ScopeSections, StringComparison.OrdinalIgnoreCase) && settings.SourceSections.Length == 0) return null;
        return FilterSections(settings, sectionContent).ToArray();
    }

    /// <summary>
    /// The content sections a scope selects, in report order.
    /// </summary>
    private static IReadOnlyList<ReportSectionModel> FilterSections(API.Models.Settings.ReportSectionSettingsModel settings, Dictionary<string, ReportSectionModel> sections)
    {
        var isSectionScope = String.Equals(settings.AIScope, ScopeSections, StringComparison.OrdinalIgnoreCase);
        return sections.Values
            .Where(s => s.SectionType != Entities.ReportSectionType.AI && s.Content.Any())
            .Where(s => !isSectionScope || settings.SourceSections.Contains(s.Name, StringComparer.OrdinalIgnoreCase))
            .OrderBy(s => s.SortOrder)
            .ToArray();
    }

    /// <summary>
    /// Every story in the sections, once each, as synthesis input. Each story names every section of
    /// the report it appears in, so the model can tell which stories belong to each section.
    /// </summary>
    /// <param name="sections">The sections whose stories are read.</param>
    /// <param name="viewContentUrl">The story page URL base, or null for no links.</param>
    /// <param name="includeAnchors">Whether stories rendered in the report get anchors.</param>
    /// <param name="inputFields">The selected story fields.</param>
    /// <param name="reportSections">Every section of the report, for anchors and section labels; the sections read when null.</param>
    /// <returns></returns>
    public static IReadOnlyList<SynthesisStory> BuildStories(IEnumerable<ReportSectionModel> sections, Uri? viewContentUrl, bool includeAnchors, IEnumerable<string>? inputFields = null, IEnumerable<ReportSectionModel>? reportSections = null)
    {
        var fields = new HashSet<string>(inputFields ?? DefaultInputFields, StringComparer.Ordinal);
        var sourceSections = sections.ToArray();
        var allSections = (reportSections ?? sourceSections).OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToArray();
        // A story may first appear in a headlines-only section and be rendered later in full.
        var anchoredIds = includeAnchors
            ? allSections.Where(s => s.IsEnabled)
                .SelectMany(s => s.Content.Where(c => ReportEngine.IsAnchoredInReport(s, c)).Select(c => c.Id)).ToHashSet()
            : new HashSet<long>();
        var labels = new Dictionary<long, List<string>>();
        foreach (var section in allSections)
        {
            var label = SectionLabel(section);
            foreach (var id in section.Content.Select(c => c.Id).Distinct())
            {
                if (!labels.TryGetValue(id, out var list)) labels[id] = list = new List<string>();
                if (!list.Contains(label)) list.Add(label);
            }
        }
        var seen = new HashSet<long>();
        var stories = new List<SynthesisStory>();
        foreach (var section in sourceSections)
        {
            // EF collection order and the service response order can differ, especially for
            // historical reports. Use report order in both paths so their manifests match.
            foreach (var content in section.Content.OrderBy(c => c.SortOrder).ThenBy(c => c.Id).Where(c => seen.Add(c.Id)))
            {
                var storySections = labels.TryGetValue(content.Id, out var list) ? list.ToArray() : new[] { SectionLabel(section) };
                stories.Add(new SynthesisStory(
                    content.Id,
                    fields.Contains("headline") ? content.Headline : "",
                    BuildMetadata(content, fields, storySections),
                    BuildStoryText(content, fields),
                    viewContentUrl != null ? $"{viewContentUrl}{content.Id}" : null,
                    anchoredIds.Contains(content.Id) ? $"#{ReportEngine.ContentAnchorPrefix}{content.Id}" : null,
                    GetGroup(content),
                    content.Evidence?.AnalysisId,
                    GetMetadataFields(content, fields, storySections),
                    storySections));
            }
        }
        return stories;
    }

    /// <summary>
    /// The selected metadata fields that have a value, by name, in a fixed order. The section(s) the
    /// story belongs to are always included.
    /// </summary>
    private static IReadOnlyDictionary<string, string> GetMetadataFields(ContentModel content, ISet<string> fields, IReadOnlyList<string> sections)
    {
        var result = new Dictionary<string, string>();
        if (sections.Count > 0) result["section"] = String.Join("; ", sections);
        var parts = new (string Key, string? Value)[]
        {
            ("source", content.Source?.Name ?? content.OtherSource),
            ("mediaType", content.MediaType?.Name),
            ("series", content.Series?.Name ?? content.OtherSeries),
            ("publishedOn", content.PublishedOn?.ToString("yyyy-MM-dd HH:mm 'UTC'")),
            ("byline", content.Byline),
            ("contributor", content.Contributor?.Name),
        };
        foreach (var (key, value) in parts.Where(p => fields.Contains(p.Key) && !String.IsNullOrWhiteSpace(p.Value)))
            result[key] = value!;
        return result;
    }

    /// <summary>
    /// One line of metadata the model reads with the story, starting with its section(s).
    /// </summary>
    private static string BuildMetadata(ContentModel content, ISet<string> fields, IReadOnlyList<string> sections)
        => String.Join(" | ", GetMetadataFields(content, fields, sections).Select(p => $"{p.Key}: {p.Value}"));

    /// <summary>
    /// The selected analysis fields with either article text or a summary, never both.
    /// Article text takes priority when selected, and is a fallback when no summary is available.
    /// </summary>
    private static string BuildStoryText(ContentModel content, ISet<string> fields)
    {
        var evidence = content.Evidence;
        var summary = !String.IsNullOrWhiteSpace(evidence?.Summary)
            ? evidence.Summary.Trim() : content.Summary.HtmlToPlainText();
        var body = fields.Contains("body") || String.IsNullOrWhiteSpace(summary)
            ? ReportEngine.RemoveBase64Images(content.Body).HtmlToPlainText() : "";
        var text = new StringBuilder(String.IsNullOrWhiteSpace(body) && fields.Contains("summary") ? summary : "");
        if (evidence != null && !String.IsNullOrWhiteSpace(evidence.Summary))
        {
            var facts = evidence.Facts.Where(f => !String.IsNullOrWhiteSpace(f)).ToArray();
            if (fields.Contains("keyFacts") && facts.Length > 0)
            {
                text.Append("\nKey facts:");
                foreach (var fact in facts) text.Append("\n- ").Append(fact.Trim());
            }
            var entities = evidence.Entities.Where(e => !String.IsNullOrWhiteSpace(e)).Distinct().ToArray();
            if (fields.Contains("entities") && entities.Length > 0) text.Append("\nEntities: ").Append(String.Join("; ", entities));
            var quotes = evidence.Quotes.Where(q => !String.IsNullOrWhiteSpace(q.Statement)).ToArray();
            if (fields.Contains("quotes") && quotes.Length > 0)
            {
                text.Append("\nQuotes:");
                foreach (var quote in quotes)
                    text.Append("\n- \"").Append(quote.Statement.Trim()).Append('"').Append(String.IsNullOrWhiteSpace(quote.Speaker) ? "" : $" ({quote.Speaker.Trim()})");
            }
        }

        if (!String.IsNullOrWhiteSpace(body))
        {
            if (text.Length > 0) text.Append("\nArticle text:\n");
            text.Append(body);
        }
        return text.ToString();
    }

    /// <summary>
    /// The group a story is synthesized in: its analysis topic (the matched staff topic, otherwise
    /// the topic registry label). Stories not yet analyzed have no group and are synthesized with
    /// the rest of the section.
    /// </summary>
    private static string? GetGroup(ContentModel content)
        => String.IsNullOrWhiteSpace(content.Evidence?.Topic) ? null : content.Evidence.Topic.Trim();

    /// <summary>
    /// Pin everything the output depends on. Story input is fingerprinted rather than copied.
    /// </summary>
    private static JsonDocument BuildManifest(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        API.Areas.Services.Models.LLM.LLMModel llm,
        SynthesisRequest request,
        IReadOnlyList<PreviousReportModel> previousReports)
    {
        var settings = section.Settings;
        var manifest = new
        {
            pipeline = SynthesisPrompts.PipelineVersion,
            reportId = report.Id,
            sectionId = section.Id,
            section = section.Name,
            llm = new { llm.Id, llm.DeploymentName, endpoint = llm.ProjectEndpoint?.ToString(), llm.ContextWindow, llm.MaxOutputTokens, llm.TokenEstimation },
            prompts = new { system = settings.SystemPrompt, user = settings.UserPrompt },
            settings = new
            {
                settings.Label,
                settings.AIScope,
                settings.SourceSections,
                outputMode = request.Mode.ToString(),
                inputFields = (settings.AIInputFields ?? DefaultInputFields.ToArray())
                    .Where(f => AllowedInputFields.Contains(f)).Distinct().OrderBy(f => f).ToArray(),
                settings.Temperature,
                settings.ChoiceQty,
                settings.ChoiceIndex,
                settings.IncludePreviousReports,
            },
            sections = (request.Sections ?? Array.Empty<SynthesisSection>()).Select(s => new { s.Id, s.Label, s.Anchor }),
            stories = request.Stories.Select(s => new { s.ContentId, s.AnalysisId, s.Url, s.Anchor, s.Group, input = Fingerprint(s) }),
            history = previousReports.Select((p, i) => new
            {
                p.InstanceId,
                stories = request.History[i].Stories.Select(s => new { s.ContentId, s.AnalysisId, s.Url, input = Fingerprint(s) }),
            }),
        };
        return JsonSerializer.SerializeToDocument(manifest, _manifestOptions);
    }

    private static string Fingerprint(SynthesisStory story)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{story.Headline}\n{story.Metadata}\n{story.Text}")))[..16];

    /// <summary>
    /// SHA-256 of the manifest, as lowercase hex.
    /// </summary>
    public static string Hash(JsonDocument manifest)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manifest.RootElement.GetRawText()))).ToLowerInvariant();
    #endregion
}
