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
    /// <param name="cancellationToken"></param>
    /// <returns>The section output, or an error when the section cannot be generated.</returns>
    public async Task<(string? Output, string? Error)> GenerateAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        long? reportInstanceId,
        API.Areas.Services.Models.Report.ReportSectionModel section,
        Dictionary<string, ReportSectionModel> sectionContent,
        IReadOnlyList<PreviousReportModel> previousReports,
        API.Areas.Services.Models.LLM.LLMModel llm,
        CancellationToken cancellationToken = default)
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

        var mode = String.Equals(settings.AIOutputMode, nameof(SynthesisOutputMode.TopicSummary), StringComparison.OrdinalIgnoreCase)
            ? SynthesisOutputMode.TopicSummary
            : SynthesisOutputMode.FreeText;
        var sources = GetSourceSections(report, section, sectionContent);
        if (sources == null)
            return (null, "Choose the content sections that feed this AI section.");

        var stories = BuildStories(sources, _options.ViewContentUrl, true);
        var history = previousReports
            .Select(p => new SynthesisHistoricalInstance(
                p.PublishedOn.HasValue ? $"Report of {p.PublishedOn:yyyy-MM-dd}" : $"Report instance {p.InstanceId}",
                BuildStories(FilterSections(settings, p.Sections), _options.ViewContentUrl, false)))
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
            settings.ChoiceIndex);

        var manifest = BuildManifest(report, section, llm, request, previousReports);
        var hash = Hash(manifest);

        var deadline = DateTime.UtcNow.AddSeconds(Math.Max(30, _options.Synthesis.ClaimLeaseSeconds) + 60);
        while (true)
        {
            var existing = await _store.FindAsync(hash, cancellationToken);
            if (existing?.Status == Entities.ReportAIResultStatus.Completed)
            {
                _logger.LogDebug("Reusing AI result {id} for report {reportId} section {sectionId}", existing.Id, report.Id, section.Id);
                return (existing.Output, null);
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

            if (claim != null) return await SynthesizeAsync(claim.Id, request, cancellationToken);

            // Another preview, view, or send is generating this manifest; wait for its result.
            if (DateTime.UtcNow > deadline)
                return (null, "Another request is still generating this AI section.");
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

        _logger.LogInformation(
            "AI section '{section}' {outcome}: stories:{stories}, requests:{requests}, promptTokens:{promptTokens}, completionTokens:{completionTokens}, depth:{depth}, largestRequest:{largest}, durationMs:{duration}",
            request.SectionLabel, result.IsSuccess ? "generated" : $"failed ({result.Error})", result.Usage.StoriesProcessed, result.Usage.Requests,
            result.Usage.PromptTokens, result.Usage.CompletionTokens, result.Usage.ReductionDepth, result.Usage.LargestRequestTokens, result.Usage.DurationMs);

        return result.IsSuccess ? (result.Output, null) : (null, result.Error);
    }

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
    /// Every story in the sections, once each, as synthesis input.
    /// </summary>
    public static IReadOnlyList<SynthesisStory> BuildStories(IEnumerable<ReportSectionModel> sections, Uri? viewContentUrl, bool includeAnchors)
    {
        var seen = new HashSet<long>();
        var stories = new List<SynthesisStory>();
        foreach (var section in sections)
        {
            foreach (var content in section.Content.Where(c => seen.Add(c.Id)))
            {
                stories.Add(new SynthesisStory(
                    content.Id,
                    content.Headline,
                    BuildMetadata(content),
                    BuildStoryText(content),
                    viewContentUrl != null ? $"{viewContentUrl}{content.Id}" : null,
                    includeAnchors && ReportEngine.IsAnchoredInReport(section, content) ? $"#{ReportEngine.ContentAnchorPrefix}{content.Id}" : null,
                    GetGroup(content),
                    content.Evidence?.AnalysisId));
            }
        }
        return stories;
    }

    /// <summary>
    /// One line of metadata the model reads with the story.
    /// </summary>
    private static string BuildMetadata(ContentModel content)
    {
        var parts = new[]
        {
            content.Source?.Name ?? content.OtherSource,
            content.MediaType?.Name,
            content.Series?.Name ?? content.OtherSeries,
            content.PublishedOn?.ToString("yyyy-MM-dd HH:mm 'UTC'"),
            String.IsNullOrWhiteSpace(content.Byline) ? null : $"By {content.Byline}",
            content.Contributor?.Name,
        };
        return String.Join(" | ", parts.Where(p => !String.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// The story's synthesis input: its analysis (summary, key facts, entities, and quotes) when it
    /// has been analyzed, otherwise its full body as plain text, or its summary when it has no body.
    /// </summary>
    private static string BuildStoryText(ContentModel content)
    {
        var evidence = content.Evidence;
        if (evidence != null && !String.IsNullOrWhiteSpace(evidence.Summary))
        {
            var text = new StringBuilder(evidence.Summary.Trim());
            var facts = evidence.Facts.Where(f => !String.IsNullOrWhiteSpace(f)).ToArray();
            if (facts.Length > 0)
            {
                text.Append("\nKey facts:");
                foreach (var fact in facts) text.Append("\n- ").Append(fact.Trim());
            }
            var entities = evidence.Entities.Where(e => !String.IsNullOrWhiteSpace(e)).Distinct().ToArray();
            if (entities.Length > 0) text.Append("\nEntities: ").Append(String.Join("; ", entities));
            var quotes = evidence.Quotes.Where(q => !String.IsNullOrWhiteSpace(q.Statement)).ToArray();
            if (quotes.Length > 0)
            {
                text.Append("\nQuotes:");
                foreach (var quote in quotes)
                    text.Append("\n- \"").Append(quote.Statement.Trim()).Append('"').Append(String.IsNullOrWhiteSpace(quote.Speaker) ? "" : $" ({quote.Speaker.Trim()})");
            }
            return text.ToString();
        }

        var body = ReportEngine.RemoveBase64Images(content.Body).HtmlToPlainText();
        return String.IsNullOrWhiteSpace(body) ? content.Summary.HtmlToPlainText() : body;
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
                settings.AIOutputMode,
                settings.Temperature,
                settings.ChoiceQty,
                settings.ChoiceIndex,
                settings.IncludePreviousReports,
            },
            stories = request.Stories.Select(s => new { s.ContentId, s.AnalysisId, s.Anchor, s.Group, input = Fingerprint(s) }),
            history = previousReports.Select((p, i) => new
            {
                p.InstanceId,
                stories = request.History[i].Stories.Select(s => new { s.ContentId, s.AnalysisId, input = Fingerprint(s) }),
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
