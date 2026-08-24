using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TNO.AI;
using TNO.API.Areas.Admin.Models.Automation;
using TNO.Elastic;
using TNO.Services.Automation.Config;
using AdminAutomationProfileModel = TNO.API.Areas.Admin.Models.Automation.AutomationProfileModel;
using AdminAutomationRunModel = TNO.API.Areas.Admin.Models.Automation.AutomationRunModel;
using ContentActionModel = TNO.API.Areas.Services.Models.Content.ContentActionModel;
using ContentModel = TNO.API.Areas.Services.Models.Content.ContentModel;
using ContentTagModel = TNO.API.Areas.Services.Models.Content.ContentTagModel;
using ContentTonePoolModel = TNO.API.Areas.Services.Models.Content.ContentTonePoolModel;
using LLMModel = TNO.API.Areas.Services.Models.LLM.LLMModel;
using LookupModel = TNO.API.Areas.Editor.Models.Lookup.LookupModel;

namespace TNO.Services.Automation.Engine;

/// <summary>
/// AutomationEngine class, executes schema-version-2 automation profiles.
/// Concepts (see docs/planning/mmi-automation/01-engine.md):
/// - a run context of named collections holding projected digests plus deltas - never full models;
/// - lifecycle phases (init → process → complete), each process step declaring its content source;
/// - the subject rule: every action applies to the item its step iterates;
/// - lazy named analyses (structured or raw) consumed by actions through value sources;
/// - declarative property conditions evaluated before any prompt is sent;
/// - exclusion that stops future work but never discards accumulated changes;
/// - explicit saving (Save Collection / Save Content Now): one fetch + one update + one index per saved item;
/// - an always-on decision log (prompts included) flushed incrementally;
/// - dry runs that compute and log everything but write nothing;
/// - comparison runs executing two definitions dry and diffing their intended changes.
/// </summary>
public class AutomationEngine
{
    #region Variables
    private const int FilterPageSize = 500;
    private const int MaxResultWindow = 10000;
    private const int DefaultSourceMax = 2000;
    private const int DefaultSearchMax = 500;
    // The summary travels whole to the browser and is capped at 10MB by the API; a headline is
    // there to identify a story, not to reproduce it.
    private const int SummaryHeadlineLength = 120;
    // How many items a single log message names inline; the rest are reported as a count (the
    // per-item entries carry the detail, and the run outcome carries the full list).
    private const int MaxLoggedItems = 25;

    private static readonly string[] DefaultDigestFields =
    {
        "id", "headline", "byline", "summary", "body", "publishedOn", "source", "otherSource",
        "section", "page", "edition", "status", "contentType", "sourceId", "licenseId", "mediaTypeId", "uid",
        "source.name", "source.code", "mediaType.name", "series.name", "contributor.name",
        "labels", "topics", "sentiment", "tags", "actions", "publishedOnUtc",
    };

    /// <summary>
    /// Compound digest fields project from nested _source objects; map each digest field to the
    /// Elasticsearch source field(s) that carry it.
    /// </summary>
    private static readonly Dictionary<string, string[]> DigestSourceFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["source"] = new[] { "source", "otherSource" },
        ["source.name"] = new[] { "source", "otherSource" },
        ["source.code"] = new[] { "source", "otherSource" },
        ["mediaType.name"] = new[] { "mediaType" },
        ["series.name"] = new[] { "series", "otherSeries" },
        ["contributor.name"] = new[] { "contributor" },
        ["sentiment"] = new[] { "tonePools" },
        ["publishedOnUtc"] = new[] { "publishedOn" },
    };

    // The body is never capped by default: content.update writes the LLM's body back, so a
    // truncated ingest would silently destroy story text on save. An explicit per-action
    // 'truncate' config can still cap it deliberately.
    private static readonly Dictionary<string, int> DefaultTruncation = new(StringComparer.OrdinalIgnoreCase)
    {
        ["headline"] = 300,
        ["summary"] = 500,
    };

    private readonly IApiService _api;
    private readonly ITNOElasticClient _elasticClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly AutomationOptions _options;
    private readonly ILogger _logger;
    private readonly LlmDirectClient _llm;
    private int _draftCounter;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AutomationEngine.
    /// </summary>
    public AutomationEngine(IApiService api, ITNOElasticClient elasticClient, ElasticOptions elasticOptions, AutomationOptions options, ILogger logger)
    {
        _api = api;
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions;
        _options = options;
        _logger = logger;
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(30, options.LLMRequestTimeoutSeconds)),
        };
        _llm = new LlmDirectClient(httpClient, logger);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Execute the specified profile for the specified run and return the outcome summary.
    /// </summary>
    public async Task<RunSummary> ExecuteAsync(AdminAutomationProfileModel profile, AdminAutomationRunModel run)
    {
        if (string.IsNullOrWhiteSpace(profile.Definition))
            throw new InvalidOperationException($"Automation profile '{profile.Name}' is schema version {profile.SchemaVersion} but has no definition document.");
        var definition = AutomationDefinition.Parse(profile.Definition!);

        if (!string.IsNullOrWhiteSpace(run.CompareDefinition))
        {
            // Comparison mode: both variants execute dry over the same trigger; the differences
            // between their intended change sets are the product. Note the same-content caveat:
            // each variant runs its own searches, so time-of-execution drift is possible.
            var candidate = AutomationDefinition.Parse(run.CompareDefinition!);
            var a = await ExecuteVariantAsync(profile, run, definition, "A", isDryRun: true);
            var b = await ExecuteVariantAsync(profile, run, candidate, "B", isDryRun: true);
            return new RunSummary
            {
                IsDryRun = true,
                IsComparison = true,
                VariantA = a,
                VariantB = b,
                Differences = BuildDifferences(a, b),
            };
        }

        var summary = await ExecuteVariantAsync(profile, run, definition, null, run.IsDryRun);
        return new RunSummary { IsDryRun = run.IsDryRun, VariantA = summary };
    }

    /// <summary>
    /// Execute one definition (one comparison variant, or the whole run).
    /// </summary>
    private async Task<VariantSummary> ExecuteVariantAsync(AdminAutomationProfileModel profile, AdminAutomationRunModel run, AutomationDefinition definition, string? variant, bool isDryRun)
    {
        var runTimer = Stopwatch.StartNew();
        var summary = new VariantSummary();
        var context = new RunContext();
        var runLogger = new RunLogger(_api, run.Id, variant, _logger);
        var lookups = await _api.GetLookupsAsync();
        var prompts = new PromptBuilder(definition, lookups, context, _jsonOptions);
        var llmCache = new Dictionary<int, LLMModel>();
        var filterCache = new Dictionary<int, (string Query, string? Settings)>();
        var idSetCache = new Dictionary<int, HashSet<long>>();
        var parallelism = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, _options.MaxParallelContentItems) };

        if (isDryRun)
            runLogger.LogDecision("run", null, null, null, Outcomes.Info, "Dry run: every decision and change is computed and logged, nothing is written.");

        // Steps execute in declared order within the init → process → complete phase order.
        var steps = definition.Steps.Where(s => s.IsEnabled)
            .OrderBy(s => Array.IndexOf(AutomationPhases.All, s.Phase))
            .ToArray();

        var environment = new RunEnvironment(profile, run, definition, context, runLogger, prompts, lookups, llmCache, filterCache, idSetCache, summary, isDryRun);

        foreach (var step in steps)
        {
            // A deleted run must stop executing, not grind on with nowhere to record anything.
            if (runLogger.IsAbandoned)
                throw new InvalidOperationException("The run record was deleted while executing; the run was stopped.");
            var stepTimer = Stopwatch.StartNew();
            var stepSummary = new StepSummary { Name = step.Name, Phase = step.Phase };
            summary.Steps.Add(stepSummary);
            var llmCallsBefore = runLogger.LlmCalls;

            try
            {
                // Process steps always iterate; a complete step iterates its required source.
                // Once-natured actions (select-top, report.run, set operations, ...) keep their
                // once-per-step semantics inside an iterating step: they are partitioned out of
                // the per-item pass and executed exactly once after iteration completes.
                if (step.Phase != AutomationPhases.Init && step.Source != null)
                {
                    var perItemActions = step.Actions
                        .Where(a => !ActionCatalog.Types.TryGetValue(a.Type, out var d) || d.Phases.Contains(AutomationPhases.Process))
                        .ToList();
                    var onceActions = step.Actions
                        .Where(a => ActionCatalog.Types.TryGetValue(a.Type, out var d) && !d.Phases.Contains(AutomationPhases.Process))
                        .ToList();
                    var itemStep = CloneWithActions(step, perItemActions);

                    var entries = await ResolveSourceAsync(step, environment, stepSummary);
                    stepSummary.Items = entries.Count;
                    await Parallel.ForEachAsync(entries, parallelism, async (entry, _) =>
                    {
                        if (environment.Log.IsAbandoned) return;
                        try
                        {
                            await ExecuteStepInstanceAsync(itemStep, new ItemScope(entry), environment, stepSummary);
                        }
                        catch (Exception ex)
                        {
                            // One failed item must not fail the whole run.
                            lock (stepSummary) stepSummary.Failures++;
                            _logger.LogError(ex, "Step '{step}' failed for item {key}; continuing with the next item.", step.Name, entry.Key);
                            runLogger.LogDecision(step.Name, null, null, entry.Kind == "existing" ? entry.Id : null, Outcomes.Failed, $"Step instance failed: {ex.Message}");
                        }
                    });

                    if (onceActions.Count > 0)
                        await ExecuteStepInstanceAsync(CloneWithActions(step, onceActions), new ItemScope(null), environment, stepSummary);
                }
                else
                {
                    try
                    {
                        await ExecuteStepInstanceAsync(step, new ItemScope(null), environment, stepSummary);
                    }
                    catch (Exception ex)
                    {
                        stepSummary.Failures++;
                        _logger.LogError(ex, "Step '{step}' failed; continuing with the next step.", step.Name);
                        runLogger.LogDecision(step.Name, null, null, null, Outcomes.Failed, $"Step failed: {ex.Message}");
                    }
                }

            }
            finally
            {
                stepTimer.Stop();
                stepSummary.DurationMs = stepTimer.ElapsedMilliseconds;
                stepSummary.LlmCalls = runLogger.LlmCalls - llmCallsBefore;
                _logger.LogInformation(
                    "Step '{step}' ({phase}) completed in {elapsed:0.0}s - items: {items}, executions: {executions}, skipped: {skipped}, excluded: {excluded}, failures: {failures}.",
                    step.Name, step.Phase, stepTimer.Elapsed.TotalSeconds, stepSummary.Items, stepSummary.Executions, stepSummary.Skipped, stepSummary.Excluded, stepSummary.Failures);
                // Persist the log incrementally so a failed run still has its trace.
                await runLogger.FlushAsync();
            }
        }

        // Nothing auto-saves: whatever is still dirty (or an unsaved draft) was never covered by
        // a Save Collection / Save Content Now action - surface it rather than silently drop it.
        ReportUnwritten(environment);
        FinalizeScoreSummaries(summary);
        ReportScoreObjectives(environment);
        await runLogger.FlushAsync();

        runTimer.Stop();
        lock (context.Sync)
        {
            summary.Collections = context.Collections.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
            summary.DraftIds = new Dictionary<string, long>(context.DraftIds);
        }
        summary.LlmCalls = runLogger.LlmCalls;
        summary.PromptTokens = runLogger.PromptTokens;
        summary.CompletionTokens = runLogger.CompletionTokens;
        summary.DurationMs = runTimer.ElapsedMilliseconds;
        return summary;
    }

    /// <summary>
    /// A shallow step copy carrying a subset of its actions (shared analyses), used to partition
    /// per-item actions from once-actions in an iterating step.
    /// </summary>
    private static StepDefinition CloneWithActions(StepDefinition step, List<ActionDefinition> actions) => new()
    {
        Name = step.Name,
        Description = step.Description,
        Phase = step.Phase,
        IsEnabled = step.IsEnabled,
        Source = step.Source,
        LlmId = step.LlmId,
        Analyses = step.Analyses,
        Actions = actions,
    };

    /// <summary>
    /// Shared per-variant services threaded through execution as one object rather than the v1
    /// engine's thirteen hand-threaded parameters.
    /// </summary>
    private sealed record RunEnvironment(
        AdminAutomationProfileModel Profile,
        AdminAutomationRunModel Run,
        AutomationDefinition Definition,
        RunContext Context,
        RunLogger Log,
        PromptBuilder Prompts,
        LookupModel? Lookups,
        Dictionary<int, LLMModel> LlmCache,
        Dictionary<int, (string Query, string? Settings)> FilterCache,
        Dictionary<int, HashSet<long>> IdSetCache,
        VariantSummary Summary,
        bool IsDryRun);

    #region Source resolution
    /// <summary>
    /// Resolve a process step's content entries from its declared source, apply include/exclude
    /// gate filters (each gate filter resolves once per run to an id set), and skip items the run
    /// has excluded.
    /// </summary>
    private async Task<List<ContentEntry>> ResolveSourceAsync(StepDefinition step, RunEnvironment env, StepSummary stepSummary)
    {
        var source = step.Source ?? throw new InvalidOperationException($"Process step '{step.Name}' has no source.");
        List<ContentEntry> entries;
        switch (source.From)
        {
            case "collection":
                {
                    lock (env.Context.Sync)
                    {
                        entries = env.Context.Collections.TryGetValue(source.Collection ?? "", out var list)
                            ? list.ToList()
                            : new List<ContentEntry>();
                    }
                    break;
                }
            case "filter":
                {
                    var (query, settings) = await GetFilterQueryAsync(source.Filter!.Value, env);
                    entries = await SearchDigestEntriesAsync(query, settings, source.Fields, source.Max ?? DefaultSourceMax, null, env, step.Name);
                    break;
                }
            default:
                {
                    env.Log.LogDecision(step.Name, null, null, null, Outcomes.Skipped, $"Unknown step source '{source.From}'; the step has nothing to iterate.");
                    return new List<ContentEntry>();
                }
        }

        // Gates: membership is a hash lookup per item; each distinct filter queried once per run.
        var includes = new List<HashSet<long>>();
        foreach (var filterId in source.Include) includes.Add(await GetFilterIdSetAsync(filterId, env));
        var excludes = new List<HashSet<long>>();
        foreach (var filterId in source.Exclude) excludes.Add(await GetFilterIdSetAsync(filterId, env));

        var eligible = new List<ContentEntry>();
        foreach (var entry in entries)
        {
            string? skipReason = null;
            lock (env.Context.Sync)
            {
                if (env.Context.Excluded.ContainsKey(entry.Key)) skipReason = "excluded earlier in the run";
            }
            if (skipReason == null && entry.Kind == "existing")
            {
                if (includes.Any(set => !set.Contains(entry.Id))) skipReason = "not matched by an include filter";
                else if (excludes.Any(set => set.Contains(entry.Id))) skipReason = "matched by an exclude filter";
            }
            if (skipReason != null)
            {
                lock (stepSummary) stepSummary.Skipped++;
                env.Log.LogDecision(step.Name, null, null, entry.Kind == "existing" ? entry.Id : null, Outcomes.Skipped, $"Item skipped: {skipReason}.");
            }
            else eligible.Add(entry);
        }
        return eligible;
    }

    private async Task<(string Query, string? Settings)> GetFilterQueryAsync(int filterId, RunEnvironment env)
    {
        if (env.FilterCache.TryGetValue(filterId, out var cached)) return cached;
        var filter = await _api.GetFilterAsync(filterId)
            ?? throw new InvalidOperationException($"Filter {filterId} does not exist.");
        var value = (filter.Query.RootElement.GetRawText(), (string?)JsonSerializer.Serialize(filter.Settings, _jsonOptions));
        env.FilterCache[filterId] = value;
        return value;
    }

    private async Task<HashSet<long>> GetFilterIdSetAsync(int filterId, RunEnvironment env)
    {
        if (env.IdSetCache.TryGetValue(filterId, out var cached)) return cached;
        var (query, settings) = await GetFilterQueryAsync(filterId, env);
        var index = SelectIndex(settings);
        var root = JsonNode.Parse(query)!.AsObject();
        root["size"] = FilterPageSize;
        root["track_total_hits"] = true;
        root["_source"] = new JsonArray("id");

        var ids = new HashSet<long>();
        var from = 0;
        while (true)
        {
            root["from"] = from;
            var result = await _elasticClient.SearchAsync<ContentModel>(index, JsonDocument.Parse(root.ToJsonString()));
            var page = result.Hits.Hits.Select(hit => hit.Source).Where(item => item != null).ToList();
            foreach (var item in page) ids.Add(item!.Id);
            if (page.Count < FilterPageSize) break;
            from += FilterPageSize;
            if (from + FilterPageSize > MaxResultWindow) break;
        }
        env.IdSetCache[filterId] = ids;
        return ids;
    }

    /// <summary>
    /// Execute a filter and build shared digest entries: the Elasticsearch _source is projected to
    /// the declared fields and long text is truncated on ingest, so the run holds digests, never
    /// bodies of full models.
    /// </summary>
    private async Task<List<ContentEntry>> SearchDigestEntriesAsync(string query, string? settings, List<string>? fields, int maxItems, Dictionary<string, int>? truncate, RunEnvironment env, string label)
    {
        var digestFields = (fields is { Count: > 0 } ? fields.ToArray() : DefaultDigestFields)
            .Union(new[] { "id" }, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var index = SelectIndex(settings);
        var root = JsonNode.Parse(query)!.AsObject();
        root["size"] = FilterPageSize;
        root["track_total_hits"] = true;
        // Project the _source to the fields the digest reads; compound digest fields map to the
        // nested objects that carry them.
        var sourceFields = digestFields
            .SelectMany(f => DigestSourceFields.TryGetValue(f, out var mapped) ? mapped : new[] { f })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        root["_source"] = new JsonArray(sourceFields.Select(f => (JsonNode?)f).ToArray());

        var entries = new List<ContentEntry>();
        var from = 0;
        var truncated = false;
        while (true)
        {
            root["from"] = from;
            var result = await _elasticClient.SearchAsync<ContentModel>(index, JsonDocument.Parse(root.ToJsonString()));
            var page = result.Hits.Hits.Select(hit => hit.Source).Where(item => item != null).ToList();
            foreach (var item in page)
            {
                if (entries.Count >= maxItems) { truncated = true; break; }
                var entry = env.Context.GetOrAddEntry(item!.Id, () => BuildEntry(item!, digestFields, truncate));
                entries.Add(entry);
            }
            if (truncated || page.Count < FilterPageSize) break;
            from += FilterPageSize;
            if (from + FilterPageSize > MaxResultWindow) { truncated = true; break; }
        }
        if (truncated)
            env.Log.LogDecision(label, null, null, null, Outcomes.Info, $"Search truncated at {entries.Count} item(s) (max {maxItems}, result window {MaxResultWindow}).");
        return entries;
    }

    private static ContentEntry BuildEntry(ContentModel content, string[] fields, Dictionary<string, int>? truncate)
    {
        var entry = new ContentEntry { Kind = "existing", Id = content.Id };
        foreach (var field in fields)
        {
            var value = field.ToLowerInvariant() switch
            {
                "id" => content.Id.ToString(),
                "headline" => content.Headline,
                "byline" => content.Byline,
                "summary" => content.Summary,
                "body" => content.Body,
                // Compared as a date, not a timestamp - two stories filed the same day must render identically.
                "publishedon" => content.PublishedOn?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "",
                // The full timestamp, kept so a copied/created item preserves the original's dates.
                "publishedonutc" => content.PublishedOn?.ToUniversalTime().ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                "source" => content.Source?.Name ?? content.OtherSource,
                "othersource" => content.OtherSource,
                "section" => content.Section,
                "page" => content.Page,
                "edition" => content.Edition,
                "status" => content.Status.ToString(),
                "contenttype" => content.ContentType.ToString(),
                "sourceid" => content.SourceId?.ToString(),
                "licenseid" => content.LicenseId.ToString(),
                "mediatypeid" => content.MediaTypeId.ToString(),
                "uid" => content.Uid,
                "tags" => JsonSerializer.Serialize(content.Tags.Select(t => t.Code)),
                // Only actions that are actually applied: Boolean actions carry value "true",
                // String actions a non-empty value.
                "actions" => JsonSerializer.Serialize(content.Actions
                    .Where(a => a.ValueType == Entities.ValueType.Boolean
                        ? string.Equals(a.Value, "true", StringComparison.OrdinalIgnoreCase)
                        : !string.IsNullOrWhiteSpace(a.Value))
                    .Select(a => a.Name)),
                "source.name" => content.Source?.Name ?? content.OtherSource,
                "source.code" => content.Source?.Code ?? content.OtherSource,
                "mediatype.name" => content.MediaType?.Name,
                "series.name" => content.Series?.Name ?? content.OtherSeries,
                "contributor.name" => content.Contributor?.Name,
                "labels" => string.Join(",", content.Labels.Select(l => string.IsNullOrWhiteSpace(l.Value) ? l.Key : l.Value)),
                "topics" => JsonSerializer.Serialize(content.Topics.Select(t => new { name = t.Name, score = t.Score })),
                "sentiment" => (content.TonePools.FirstOrDefault(t => t.Name == "Default") ?? content.TonePools.FirstOrDefault())?.Value.ToString(),
                _ => null,
            };
            if (value == null) continue;
            var caps = truncate ?? DefaultTruncation;
            if (caps.TryGetValue(field, out var cap) && value.Length > cap) value = value[..cap];
            entry.Digest[field] = value;
        }
        return entry;
    }

    private string SelectIndex(string? filterSettings)
    {
        var searchUnpublished = false;
        if (!string.IsNullOrWhiteSpace(filterSettings))
        {
            try
            {
                using var settings = JsonDocument.Parse(filterSettings);
                if (settings.RootElement.TryGetProperty("searchUnpublished", out var property))
                    searchUnpublished = property.ValueKind == JsonValueKind.True;
            }
            catch (JsonException)
            {
                // Malformed settings default to the published index.
            }
        }
        return searchUnpublished ? _elasticOptions.ContentIndex : _elasticOptions.PublishedIndex;
    }
    #endregion

    #region Step execution
    /// <summary>
    /// Execute one step instance: evaluate each action's gates (property conditions first - a
    /// failing condition costs no LLM call), run consumed analyses lazily, and dispatch confirmed
    /// actions. Action failures are isolated; 'abort'/'exclude' stop the remaining actions.
    /// </summary>
    private async Task ExecuteStepInstanceAsync(StepDefinition step, ItemScope scope, RunEnvironment env, StepSummary stepSummary)
    {
        lock (stepSummary) stepSummary.Executions++;
        var subject = scope.Subject;
        var contentId = subject is { Kind: "existing" } ? subject.Id : (long?)null;

        foreach (var action in step.Actions.Where(a => a.IsEnabled))
        {
            if (scope.Aborted || scope.Excluded) break;
            var actionName = action.Name ?? action.Type;
            try
            {
                if (!ActionCatalog.Types.TryGetValue(action.Type, out var descriptor))
                {
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"Action type '{action.Type}' is not registered.");
                    continue;
                }
                if (descriptor.RequiresSubject && subject == null)
                {
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, "The action requires an iterated item and the step has none.");
                    continue;
                }

                // Gate 1: property condition / analysis-result gate. Evaluated before any prompt
                // is sent - this is where most of the saved runtime comes from.
                if (action.When != null)
                {
                    var target = scope.ResolveTarget(action.Target) ?? subject;
                    // Result references can sit anywhere in the condition tree (not/all/any, not
                    // just the top level); every one needs its analysis triggered and the boolean
                    // resolver supplied, or the gate reads nothing and fails/passes wrongly.
                    var references = new List<string>();
                    CollectFromRefs(action.When, references);
                    foreach (var reference in references)
                        await EnsureAnalysisForReferenceAsync(step, reference, scope, env, stepSummary);
                    var result = references.Count > 0
                        ? ConditionEvaluator.Evaluate(action.When, f => target?.GetField(f), reference => ValueResolver.ResolveBool(reference, scope))
                        : ConditionEvaluator.Evaluate(action.When, f => target?.GetField(f));
                    if (!result.Passed)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.ConditionFailed, result.Detail);
                        continue;
                    }
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.ConditionPassed, result.Detail);
                }

                // Gate 2: confirmation statement against a raw analysis response ({value} capture).
                string? captured = null;
                if (!string.IsNullOrWhiteSpace(action.Confirm))
                {
                    var analysisName = action.Analysis ?? (step.Analyses.Count == 1 ? step.Analyses[0].Name : null);
                    if (analysisName == null)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, "Confirm requires a named analysis.");
                        continue;
                    }
                    var response = await EnsureAnalysisAsync(step, analysisName, scope, env, stepSummary) ?? "";
                    var matcher = new ConfirmationMatcher(action.Confirm, action.Field, action.Objective);
                    if (!matcher.IsValid)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, "Invalid confirmation statement.");
                        continue;
                    }
                    if (!matcher.TryMatch(response, out captured))
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.NotConfirmed,
                            string.IsNullOrWhiteSpace(response) ? "No confirmation; the response was empty (no criteria met)." : "The confirmation statement was not found in the response.");
                        continue;
                    }
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Confirmed, $"Confirmed by analysis '{analysisName}'{(captured != null ? $" with value '{Truncate(captured, 200)}'" : "")}.");
                }

                await ExecuteActionAsync(step, action, descriptor, scope, captured, env, stepSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Action '{action}' ({type}) in step '{step}' failed; skipping it.", actionName, action.Type, step.Name);
                env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Failed, $"Action failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Ensure the analysis a 'name.key' reference names has run (lazily) for this item.
    /// </summary>
    /// <summary>Collect every 'analysisName.key' reference anywhere in a condition tree.</summary>
    private static void CollectFromRefs(ConditionDefinition condition, List<string> references)
    {
        if (!string.IsNullOrWhiteSpace(condition.From)) references.Add(condition.From!);
        if (condition.Not != null) CollectFromRefs(condition.Not, references);
        foreach (var child in condition.All ?? Enumerable.Empty<ConditionDefinition>()) CollectFromRefs(child, references);
        foreach (var child in condition.Any ?? Enumerable.Empty<ConditionDefinition>()) CollectFromRefs(child, references);
    }

    private async Task EnsureAnalysisForReferenceAsync(StepDefinition step, string reference, ItemScope scope, RunEnvironment env, StepSummary stepSummary)
    {
        var name = reference.Split('.', 2)[0];
        if (!name.Equals("content", StringComparison.OrdinalIgnoreCase))
            await EnsureAnalysisAsync(step, name, scope, env, stepSummary);
    }

    /// <summary>
    /// Run the named analysis for this item if it has not run yet, and return its raw response.
    /// Lazy: an item whose actions never consume an analysis never pays for it.
    /// </summary>
    private async Task<string?> EnsureAnalysisAsync(StepDefinition step, string name, ItemScope scope, RunEnvironment env, StepSummary stepSummary)
    {
        if (scope.Raw.TryGetValue(name, out var existing)) return existing;
        var analysis = step.Analyses.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (analysis == null) return null;

        // A chained analysis continues the earlier exchange so the model retains that context.
        List<(string Role, string Content)> messages;
        var chainRoot = name;
        if (!string.IsNullOrWhiteSpace(analysis.Chain))
        {
            await EnsureAnalysisAsync(step, analysis.Chain!, scope, env, stepSummary);
            chainRoot = FindChainRoot(step, analysis);
            messages = scope.Conversations.TryGetValue(chainRoot, out var conversation)
                ? conversation
                : new List<(string, string)>();
        }
        else
        {
            messages = new List<(string, string)>();
        }

        var subject = scope.Subject;
        // '{target...}' reads the draft the analysis names, so a prompt can describe the copy the
        // step's actions are building instead of only the item the iteration started from. An
        // unresolved draft is logged, never silent: the tokens then render as nothing.
        ContentEntry? target = null;
        if (!string.IsNullOrWhiteSpace(analysis.Target))
        {
            target = scope.ResolveTarget(analysis.Target);
            if (target == null)
                env.Log.LogDecision(step.Name, name, "analysis", subject is { Kind: "existing" } ? subject.Id : (long?)null, Outcomes.Info,
                    $"Draft '{analysis.Target}' does not exist yet, so the analysis's {{target...}} tokens are empty. It is created by a content.create action - make sure that action runs before whatever consumes this analysis.");
        }
        var text = env.Prompts.Resolve(analysis.Prompt);
        // The first message of an exchange carries the working copy; later chained turns already
        // have it, so they substitute the tokens without re-appending the story.
        text = env.Prompts.Substitute(text, subject, target, appendSubject: messages.Count == 0);
        if (!analysis.Raw && analysis.Returns.Count > 0)
        {
            var spec = string.Join("\n", analysis.Returns.Select(kv => $"- \"{kv.Key}\": {kv.Value}"));
            text = $"{text}\n\nRespond with a single JSON object with exactly these keys:\n{spec}";
        }
        messages.Add(("user", text));

        var llm = await ResolveLlmAsync(analysis.LlmId ?? step.LlmId ?? env.Profile.LLMId, env);
        var contentId = subject is { Kind: "existing" } ? subject.Id : (long?)null;
        var timer = Stopwatch.StartNew();
        try
        {
            var result = await _llm.InvokeAsync(
                new LlmEndpoint(llm.ProjectEndpoint!, llm.ApiKey!, llm.DeploymentName!),
                messages,
                jsonMode: !analysis.Raw,
                attempts: Math.Max(1, _options.LLMRequestAttempts));
            timer.Stop();

            messages.Add(("assistant", result.Content));
            scope.Conversations[chainRoot] = messages;
            scope.Raw[name] = result.Content;

            var detail = (string?)null;
            if (!analysis.Raw)
            {
                var document = ParseJsonLenient(result.Content);
                if (document != null)
                {
                    scope.Structured[name] = document;
                    var missing = analysis.Returns.Keys
                        .Where(key => document.RootElement.ValueKind != JsonValueKind.Object
                            || !document.RootElement.EnumerateObject().Any(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();
                    if (missing.Length > 0) detail = $"{{\"missingKeys\":\"{string.Join(",", missing)}\"}}";
                }
                else detail = "{\"parse\":\"the response was not valid JSON\"}";
            }
            env.Log.LogLlm(step.Name, name, null, null, contentId,
                messages.Count > 2 ? text : messages[0].Content, result.Content,
                result.PromptTokens, result.CompletionTokens, timer.ElapsedMilliseconds,
                Outcomes.Executed, result.Attempts, detail);
            return result.Content;
        }
        catch (Exception ex)
        {
            timer.Stop();
            env.Log.LogLlm(step.Name, name, null, null, contentId, text, "", null, null, timer.ElapsedMilliseconds, Outcomes.Failed, 1, $"{{\"error\":{JsonSerializer.Serialize(ex.Message)}}}");
            // Cache the failure as an empty response so the item does not retry per consuming action.
            scope.Raw[name] = "";
            return "";
        }
    }

    private static string FindChainRoot(StepDefinition step, AnalysisDefinition analysis)
    {
        var current = analysis;
        var guard = 0;
        while (!string.IsNullOrWhiteSpace(current.Chain) && guard++ < 20)
        {
            var parent = step.Analyses.FirstOrDefault(a => a.Name.Equals(current.Chain, StringComparison.OrdinalIgnoreCase));
            if (parent == null) break;
            current = parent;
        }
        return current.Name;
    }

    private async Task<LLMModel> ResolveLlmAsync(int? llmId, RunEnvironment env)
    {
        var id = llmId ?? env.Profile.LLMId ?? throw new InvalidOperationException("No LLM is configured for this analysis (analysis, step, and profile are all unset).");
        lock (env.LlmCache)
        {
            if (env.LlmCache.TryGetValue(id, out var cached)) return cached;
        }
        var llm = await _api.GetLLMAsync(id) ?? throw new InvalidOperationException($"LLM {id} does not exist.");
        if (llm.ProjectEndpoint == null) throw new InvalidOperationException($"LLM '{llm.Name}' is missing a project endpoint.");
        if (string.IsNullOrWhiteSpace(llm.DeploymentName) || string.IsNullOrWhiteSpace(llm.ApiKey))
            throw new InvalidOperationException($"LLM '{llm.Name}' requires a deployment name and API key; the automation engine does not support agent-mode LLMs.");
        lock (env.LlmCache) env.LlmCache[id] = llm;
        return llm;
    }
    #endregion

    #region Action handlers
    /// <summary>
    /// Dispatch one gated action to its handler.
    /// </summary>
    private async Task ExecuteActionAsync(StepDefinition step, ActionDefinition action, ActionDescriptor descriptor, ItemScope scope, string? captured, RunEnvironment env, StepSummary stepSummary)
    {
        var actionName = action.Name ?? action.Type;
        var subject = scope.Subject;
        var target = scope.ResolveTarget(action.Target);
        var contentId = subject is { Kind: "existing" } ? subject.Id : (long?)null;

        // Value sources consume analyses too - run them lazily before resolving, exactly like
        // the when/confirm gates do, so action order never decides whether a result exists.
        if (!string.IsNullOrWhiteSpace(action.Value?.From))
            await EnsureAnalysisForReferenceAsync(step, action.Value!.From!, scope, env, stepSummary);
        if (action.Set != null)
            foreach (var source in action.Set.Values.Where(v => !string.IsNullOrWhiteSpace(v.From)))
                await EnsureAnalysisForReferenceAsync(step, source.From!, scope, env, stepSummary);

        // Resolve the action's value. The migrated-v1 pattern maps '<analysis>.value' to the
        // confirmation's {value} capture, which is the extracted value, not the raw response.
        string? value = null;
        if (action.Value != null)
        {
            value = captured != null && action.Value.From != null && action.Value.From.EndsWith(".value", StringComparison.OrdinalIgnoreCase)
                ? captured
                : ValueResolver.Resolve(action.Value, scope, target ?? subject, env.Prompts);
            if (value == null && captured != null) value = captured;
        }
        else if (captured != null)
        {
            value = captured;
        }

        void LogExecuted(string description, string? detail = null)
            => env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Executed, description, detail);
        void RecordChange(string type, ContentEntry entry, string? field = null, string? changeValue = null)
        {
            var change = new ChangeSummary
            {
                Type = type,
                ContentRef = entry.Kind == "draft" ? entry.TempKey ?? "" : entry.Id.ToString(),
                Field = field,
                Value = changeValue == null ? null : Truncate(changeValue, 500),
                Step = step.Name,
            };
            lock (env.Summary.Changes) env.Summary.Changes.Add(change);
        }

        switch (action.Type)
        {
            case "search":
                {
                    var (query, settings) = await GetFilterQueryAsync(action.Filter!.Value, env);
                    var entries = await SearchDigestEntriesAsync(query, settings, action.Fields, action.Max ?? DefaultSearchMax, action.Truncate, env, step.Name);
                    var collection = env.Context.GetCollection(action.Into!);
                    lock (env.Context.Sync)
                    {
                        foreach (var entry in entries)
                            if (!collection.Contains(entry)) collection.Add(entry);
                    }
                    LogExecuted($"Fetched {entries.Count} item(s) into {action.Into}.");
                    break;
                }
            case "collection.create":
                {
                    env.Context.GetCollection(action.Into!);
                    LogExecuted($"Created collection {action.Into}.");
                    break;
                }
            case "collection.add":
            case "collection.remove":
            case "collection.move":
                {
                    var item = ResolveItem(action.Item, scope);
                    if (item == null)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"Item '{action.Item ?? "$item"}' could not be resolved.");
                        break;
                    }
                    lock (env.Context.Sync)
                    {
                        if (action.Type != "collection.add" && env.Context.Collections.TryGetValue(action.FromCollection ?? "", out var from))
                            from.RemoveAll(e => e.Key == item.Key);
                        if (action.Type != "collection.remove" && !string.IsNullOrWhiteSpace(action.Into))
                        {
                            var into = env.Context.GetCollection(action.Into!);
                            if (!into.Any(e => e.Key == item.Key)) into.Add(item);
                        }
                    }
                    LogExecuted($"{action.Type} {item.Key}{(action.FromCollection != null ? $" from {action.FromCollection}" : "")}{(action.Into != null ? $" into {action.Into}" : "")}.");
                    break;
                }
            case "collection.filter":
                {
                    int removed;
                    lock (env.Context.Sync)
                    {
                        var list = env.Context.GetCollection(action.FromCollection!);
                        removed = list.RemoveAll(e => !ConditionEvaluator.Evaluate(action.Where!, e.GetField).Passed);
                    }
                    LogExecuted($"Filtered {action.FromCollection}; removed {removed} item(s).");
                    break;
                }
            case "collection.sortBy":
                {
                    lock (env.Context.Sync)
                    {
                        var list = env.Context.GetCollection(action.FromCollection!);
                        var sorted = string.Equals(action.Direction, "desc", StringComparison.OrdinalIgnoreCase)
                            ? list.OrderByDescending(e => e.GetField(action.By!) ?? "", StringComparer.OrdinalIgnoreCase).ToList()
                            : list.OrderBy(e => e.GetField(action.By!) ?? "", StringComparer.OrdinalIgnoreCase).ToList();
                        list.Clear();
                        list.AddRange(sorted);
                    }
                    LogExecuted($"Sorted {action.FromCollection} by {action.By} {(action.Direction ?? "asc")}.");
                    break;
                }
            case "collection.take":
                {
                    lock (env.Context.Sync)
                    {
                        var list = env.Context.GetCollection(action.FromCollection!);
                        if (list.Count > action.Count!.Value) list.RemoveRange(action.Count.Value, list.Count - action.Count.Value);
                    }
                    LogExecuted($"Kept the first {action.Count} item(s) of {action.FromCollection}.");
                    break;
                }
            case "collection.distinctBy":
                {
                    int removed;
                    lock (env.Context.Sync)
                    {
                        var list = env.Context.GetCollection(action.FromCollection!);
                        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        removed = list.RemoveAll(e => !seen.Add(e.GetField(action.By!) ?? e.Key));
                    }
                    LogExecuted($"Removed {removed} duplicate item(s) from {action.FromCollection} by {action.By}.");
                    break;
                }
            case "collection.union":
            case "collection.except":
            case "collection.intersect":
                {
                    lock (env.Context.Sync)
                    {
                        var from = env.Context.GetCollection(action.FromCollection!);
                        var with = env.Context.GetCollection(action.With!).Select(e => e.Key).ToHashSet();
                        var into = env.Context.GetCollection(action.Into!);
                        into.Clear();
                        IEnumerable<ContentEntry> result = action.Type switch
                        {
                            "collection.union" => from.Concat(env.Context.GetCollection(action.With!)).DistinctBy(e => e.Key),
                            "collection.except" => from.Where(e => !with.Contains(e.Key)),
                            _ => from.Where(e => with.Contains(e.Key)),
                        };
                        into.AddRange(result);
                        LogExecuted($"{action.Type} of {action.FromCollection} and {action.With} into {action.Into} ({into.Count} item(s)).");
                    }
                    break;
                }
            case "collection.copy":
                {
                    if (string.IsNullOrWhiteSpace(action.FromCollection) || string.IsNullOrWhiteSpace(action.Into)) break;
                    lock (env.Context.Sync)
                    {
                        var from = env.Context.GetCollection(action.FromCollection!);
                        var into = env.Context.GetCollection(action.Into!);
                        var have = into.Select(e => e.Key).ToHashSet();
                        var added = 0;
                        foreach (var entry in from)
                        {
                            if (!have.Add(entry.Key)) continue;
                            into.Add(entry);
                            added++;
                        }
                        LogExecuted($"Copied {added} of {from.Count} item(s) from {action.FromCollection} into {action.Into} ({from.Count - added} already present); {action.Into} now has {into.Count}.");
                    }
                    break;
                }
            case "content.update":
                {
                    if (target == null || string.IsNullOrWhiteSpace(action.Field) || string.IsNullOrWhiteSpace(value)) break;
                    lock (target.Deltas) target.Deltas.Fields[action.Field!] = value.Trim();
                    RecordChange("update-field", target, action.Field, value);
                    LogExecuted($"Set {action.Field} on {target.Key}.", $"{{\"value\":{JsonSerializer.Serialize(Truncate(value, 500))}}}");
                    break;
                }
            case "content.tags":
                {
                    if (target == null || string.IsNullOrWhiteSpace(value)) break;
                    // Only ENABLED tags that exist (matched by code or name) can be added - exactly
                    // the data the {lookup:tags} token renders, so the prompt's vocabulary and this
                    // validation cannot disagree. A disabled tag is reported, never applied.
                    var requested = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var unmatched = new List<string>();
                    var disabled = new List<string>();
                    var added = new List<string>();
                    foreach (var request in requested)
                    {
                        var match = env.Lookups?.Tags.FirstOrDefault(t =>
                            t.Code.Equals(request, StringComparison.OrdinalIgnoreCase) ||
                            t.Name.Equals(request, StringComparison.OrdinalIgnoreCase));
                        if (match == null) { unmatched.Add(request); continue; }
                        // Distinguished from 'no such tag': a disabled tag is a retired vocabulary
                        // entry, and the author needs to see that rather than a bare miss.
                        if (!match.IsEnabled) { disabled.Add(match.Code); continue; }
                        lock (target.Deltas)
                        {
                            if (!target.Deltas.Tags.Any(t => t.Id == match.Id)) target.Deltas.Tags.Add((match.Id, match.Code, match.Name));
                        }
                        added.Add(match.Code);
                    }
                    if (added.Count > 0) RecordChange("add-tags", target, null, string.Join(",", added));
                    LogExecuted(
                        $"Added {added.Count} tag(s) to {target.Key}"
                        + (added.Count > 0 ? $": {string.Join(", ", added)}" : "")
                        + (disabled.Count > 0 ? $". Skipped {disabled.Count} disabled tag(s): {string.Join(", ", disabled)}" : "")
                        + (unmatched.Count > 0 ? $". No tag matched: {string.Join(", ", unmatched)}" : "")
                        + ".",
                        unmatched.Count > 0 || disabled.Count > 0
                            ? JsonSerializer.Serialize(new { added, disabled, unmatched }, _jsonOptions)
                            : null);
                    break;
                }
            case "content.sentiment":
                {
                    if (target == null || !int.TryParse((value ?? "").Trim(), out var sentiment))
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"Sentiment value '{value}' is not a number.");
                        break;
                    }
                    lock (target.Deltas) target.Deltas.Sentiment = Math.Clamp(sentiment, -5, 5);
                    RecordChange("add-sentiment", target, null, sentiment.ToString());
                    LogExecuted($"Set sentiment {Math.Clamp(sentiment, -5, 5)} on {target.Key}.");
                    break;
                }
            case "content.contributor":
                {
                    if (target == null || string.IsNullOrWhiteSpace(value)) break;
                    var contributorName = value.Trim();
                    var contributor = FindContributor(contributorName, env.Lookups);
                    if (contributor == null && env.Context.CreatedContributors.TryGetValue(contributorName, out var runCreated))
                        contributor = runCreated;
                    if (contributor == null && action.Create == true)
                    {
                        if (env.IsDryRun)
                        {
                            RecordChange("select-columnist", target, null, contributorName);
                            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Executed, $"Dry run: contributor '{Truncate(contributorName, 100)}' would be created and selected.");
                            break;
                        }
                        try
                        {
                            var created = await _api.AddContributorAsync(contributorName);
                            if (created != null)
                            {
                                contributor = (created.Id, created.Name);
                                env.Context.CreatedContributors[contributorName] = contributor.Value;
                                env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Executed, $"Created contributor '{created.Name}' ({created.Id}).");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to create contributor '{name}'.", contributorName);
                            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Failed, $"Failed to create contributor '{Truncate(contributorName, 100)}': {ex.Message}");
                            break;
                        }
                    }
                    if (contributor == null)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"No contributor matched '{Truncate(contributorName, 100)}'.");
                        break;
                    }
                    lock (target.Deltas)
                    {
                        target.Deltas.ContributorId = contributor.Value.Id;
                        target.Deltas.ContributorName = contributor.Value.Name;
                    }
                    RecordChange("select-columnist", target, null, contributor.Value.Name);
                    LogExecuted($"Set contributor '{contributor.Value.Name}' on {target.Key}.");
                    break;
                }
            case "content.action":
                {
                    if (target == null || !action.ContentAction.HasValue) break;
                    // A content action disabled since the profile was authored must not keep being
                    // stamped on every run. Checked here, where the log can say why, rather than
                    // silently at flush.
                    if (!IsContentActionEnabled(action.ContentAction.Value, env.Lookups))
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped,
                            $"Content action '{ContentActionName(action.ContentAction.Value, env.Lookups)}' is disabled; nothing was applied to {target.Key}.");
                        break;
                    }
                    lock (target.Deltas)
                    {
                        if (!target.Deltas.ContentActionIds.Contains(action.ContentAction.Value))
                            target.Deltas.ContentActionIds.Add(action.ContentAction.Value);
                    }
                    RecordChange("add-action", target, ContentActionName(action.ContentAction.Value, env.Lookups), action.ContentAction.Value.ToString());
                    LogExecuted($"Applied content action '{ContentActionName(action.ContentAction.Value, env.Lookups)}' to {target.Key}.");
                    break;
                }
            case "content.publish":
            case "content.unpublish":
                {
                    if (target == null) break;
                    var status = action.Type == "content.publish" ? "publish" : "unpublish";
                    lock (target.Deltas) target.Deltas.Status = status;
                    RecordChange(status, target);
                    LogExecuted($"Marked {target.Key} for {status}.");
                    break;
                }
            case "content.create":
                {
                    if (subject == null && string.IsNullOrWhiteSpace(action.CopyFrom)) { }
                    var draft = new ContentEntry
                    {
                        Kind = "draft",
                        TempKey = $"{Slug(action.As ?? "draft")}-{Interlocked.Increment(ref _draftCounter)}",
                    };
                    var copySource = action.CopyFrom != null && action.CopyFrom.Equals("$item", StringComparison.OrdinalIgnoreCase) ? subject : null;
                    if (copySource != null)
                    {
                        // '*' (the UI's 'all fields' checkbox) copies every field the item carries -
                        // digest plus fields set by earlier actions; id stays out (the draft gets its
                        // own) and uid is derived below.
                        List<string> copyFields;
                        if (action.CopyFields?.Contains("*") == true)
                        {
                            lock (copySource.Deltas)
                                copyFields = copySource.Digest.Keys
                                    .Union(copySource.Deltas.Fields.Keys, StringComparer.OrdinalIgnoreCase)
                                    .Where(f => !f.Equals("id", StringComparison.OrdinalIgnoreCase) && !f.Equals("uid", StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                        }
                        else
                            copyFields = action.CopyFields is { Count: > 0 }
                                ? action.CopyFields
                                : new List<string> { "sourceId", "otherSource", "licenseId", "mediaTypeId", "publishedOn", "publishedOnUtc", "contentType" };
                        foreach (var field in copyFields)
                        {
                            var copied = copySource.GetField(field);
                            if (copied != null) draft.Digest[field] = copied;
                        }
                        // The derived-uid rule keeps a created item distinct from its original.
                        var originalUid = copySource.GetField("uid");
                        if (!string.IsNullOrWhiteSpace(originalUid))
                            draft.Digest["uid"] = $"{originalUid}-{Slug(action.As ?? "copy")}-{DateTime.UtcNow:yyyyMMdd}";
                    }
                    if (action.Set != null)
                    {
                        foreach (var (field, source) in action.Set)
                        {
                            var setValue = ValueResolver.Resolve(source, scope, subject, env.Prompts);
                            if (setValue != null) draft.Digest[field] = setValue;
                        }
                    }
                    scope.Drafts[action.As!] = draft;
                    lock (env.Context.Sync) env.Context.Drafts.Add(draft);
                    RecordChange("create-content", draft);
                    LogExecuted($"Prepared draft {draft.TempKey} as {action.As}.");
                    break;
                }
            case "collection.save":
                {
                    if (string.IsNullOrWhiteSpace(action.FromCollection)) break;
                    List<ContentEntry> items;
                    lock (env.Context.Sync) items = env.Context.GetCollection(action.FromCollection!).ToList();
                    // Only items with something to write: dirty deltas or unsaved drafts.
                    var dirtyKeys = env.Context.GetFlushables().Select(e => e.Key).ToHashSet();
                    var toSave = items.Where(e => dirtyKeys.Contains(e.Key)).ToList();
                    var index = action.Index ?? true;
                    var saved = 0;
                    var fieldTally = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var entry in toSave)
                    {
                        // The deltas are cleared by the flush, so what the save will write is
                        // described before it runs - the log and the outcome name the fields.
                        var pending = DescribeDeltas(entry, env.Lookups);
                        // Tallied only once the write succeeds: 'fields written' must never count a
                        // save that failed.
                        void TallyFields()
                        {
                            foreach (var field in pending) fieldTally[field] = fieldTally.TryGetValue(field, out var count) ? count + 1 : 1;
                        }
                        if (env.IsDryRun)
                        {
                            RecordSave(env, entry, step.Name, actionName, action.FromCollection, pending, "would-save", index, null);
                            env.Log.LogDecision(step.Name, actionName, action.Type, entry.Kind == "existing" ? entry.Id : null, Outcomes.Flushed,
                                $"Dry run: {EntryLabel(entry)} would be saved from {action.FromCollection} - {DescribeWrite(entry, pending)}.");
                            TallyFields();
                            saved++;
                            continue;
                        }
                        try
                        {
                            await FlushEntryAsync(entry, env, step.Name, index, actionName, action.Type, action.FromCollection);
                            TallyFields();
                            saved++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to save {key}.", entry.Key);
                            lock (env.Summary.FlushFailures) env.Summary.FlushFailures.Add($"{entry.Key}: {ex.Message}");
                            RecordSave(env, entry, step.Name, actionName, action.FromCollection, pending, "failed", index, ex.Message);
                            env.Log.LogDecision(step.Name, actionName, action.Type, entry.Kind == "existing" ? entry.Id : null, Outcomes.Failed,
                                $"Save failed for {EntryLabel(entry)}; {DescribeWrite(entry, pending)} was not written: {ex.Message}");
                        }
                    }
                    LogExecuted(
                        $"Saved {saved} of {items.Count} item(s) from {action.FromCollection} ({items.Count - toSave.Count} unchanged)."
                        + (fieldTally.Count > 0 ? $" Fields written: {string.Join(", ", fieldTally.OrderByDescending(f => f.Value).ThenBy(f => f.Key, StringComparer.OrdinalIgnoreCase).Select(f => $"{f.Key} ({f.Value})"))}." : ""),
                        JsonSerializer.Serialize(new { from = action.FromCollection, saved, unchanged = items.Count - toSave.Count, index, fields = fieldTally }, _jsonOptions));
                    break;
                }
            case "content.save":
                {
                    var saveTarget = target ?? subject;
                    if (saveTarget == null) break;
                    var index = action.Index ?? true;
                    var pending = DescribeDeltas(saveTarget, env.Lookups);
                    if (env.IsDryRun)
                    {
                        RecordSave(env, saveTarget, step.Name, actionName, null, pending, "would-save", index, null);
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Flushed,
                            $"Dry run: {EntryLabel(saveTarget)} would be saved now - {DescribeWrite(saveTarget, pending)}.");
                    }
                    else
                        await FlushEntryAsync(saveTarget, env, step.Name, index, actionName, action.Type, null);
                    break;
                }
            case "exclude":
                {
                    if (subject == null) break;
                    var reason = action.Reason ?? "excluded by configuration";
                    lock (env.Context.Sync) env.Context.Excluded[subject.Key] = reason;
                    lock (env.Summary.Excluded) env.Summary.Excluded.Add(new ExclusionSummary { ContentRef = subject.Kind == "draft" ? subject.TempKey ?? "" : subject.Id.ToString(), Reason = reason, Step = step.Name });
                    scope.Excluded = true;
                    lock (stepSummary) stepSummary.Excluded++;
                    // Accumulated deltas are kept - exclusion stops future work, never discards changes.
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Excluded, $"Excluded from later steps: {reason}. Accumulated changes are kept and will be written.");
                    break;
                }
            case "abort":
                {
                    scope.Aborted = true;
                    lock (stepSummary) stepSummary.Aborted++;
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Aborted, "Remaining actions on this step were stopped.");
                    break;
                }
            case "dedupe":
                {
                    if (subject == null) break;
                    await DetectDuplicateAsync(step, action, scope, env, stepSummary);
                    break;
                }
            case "score":
                {
                    if (subject == null || string.IsNullOrWhiteSpace(action.Objective)) break;
                    if (!int.TryParse((value ?? "").Trim(), out var score))
                    {
                        RecordUnscored(env, action.Objective!, step.Name);
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"Score value '{value}' is not a number; {EntryLabel(subject)} was not scored for '{action.Objective}'.");
                        break;
                    }
                    lock (env.Context.Sync)
                    {
                        if (!env.Context.Scores.TryGetValue(action.Objective!, out var scores))
                        {
                            scores = new Dictionary<string, int>();
                            env.Context.Scores[action.Objective!] = scores;
                        }
                        scores[subject.Key] = score;
                    }
                    RecordScore(env, action.Objective!, step.Name, subject, score);
                    // Name the story, the score, and the objective on every entry: filtering the log
                    // by content id must answer 'what score did this item get, and why'.
                    LogExecuted(
                        $"Scored {EntryLabel(subject)} at {score} for '{action.Objective}'.",
                        JsonSerializer.Serialize(new { objective = action.Objective, score, contentRef = EntryRef(subject) }, _jsonOptions));
                    break;
                }
            case "select-top":
                {
                    var objective = action.Objective ?? "";
                    // How many to keep: a fixed count, every item at or above a score, or both
                    // (the threshold qualifies, the count caps). Neither is a definition error the
                    // validator rejects; default to the historical top ten rather than selecting
                    // nothing if one slips through.
                    var minScore = action.MinScore;
                    var take = action.Take ?? (minScore.HasValue ? (int?)null : 10);
                    List<(string Key, int Score)> candidates;
                    lock (env.Context.Sync)
                    {
                        candidates = env.Context.Scores.TryGetValue(objective, out var scores)
                            ? scores.Select(kv => (kv.Key, kv.Value)).ToList()
                            : new List<(string, int)>();
                    }
                    // No LLM decides this: the recorded scores are ranked highest first, and the
                    // content id breaks ties. Items are scored in parallel, so the order they land
                    // in the score table is not reproducible and cannot be the tie-break.
                    var qualified = minScore.HasValue
                        ? candidates.Where(candidate => candidate.Score >= minScore.Value).ToList()
                        : candidates;
                    var ordered = qualified
                        .OrderByDescending(candidate => candidate.Score)
                        .ThenBy(candidate => RankId(candidate.Key))
                        .ThenBy(candidate => candidate.Key, StringComparer.Ordinal);
                    var ranked = (take.HasValue ? ordered.Take(take.Value) : ordered).ToList();
                    var distribution = candidates
                        .GroupBy(candidate => candidate.Score)
                        .OrderByDescending(group => group.Key)
                        .ToDictionary(group => group.Key, group => group.Count());
                    var selected = new List<(ContentEntry Entry, int Score)>();
                    var unresolved = new List<string>();
                    lock (env.Context.Sync)
                    {
                        foreach (var (key, rankedScore) in ranked)
                        {
                            var entry = env.Context.EntriesById.Values.FirstOrDefault(e => e.Key == key)
                                ?? env.Context.Drafts.FirstOrDefault(d => d.Key == key);
                            if (entry != null) selected.Add((entry, rankedScore));
                            else unresolved.Add(key);
                        }
                    }
                    // A disabled content action stamps nothing; the selection itself still runs so
                    // the collection and the outcome are unaffected.
                    var stampAction = action.ContentAction.HasValue && IsContentActionEnabled(action.ContentAction.Value, env.Lookups);
                    var contentActionName = action.ContentAction.HasValue ? ContentActionName(action.ContentAction.Value, env.Lookups) : null;
                    if (action.ContentAction.HasValue && !stampAction)
                        env.Log.LogDecision(step.Name, actionName, action.Type, null, Outcomes.Skipped,
                            $"Content action '{contentActionName}' is disabled; the selected items were not stamped with it.");
                    var rank = 0;
                    foreach (var (entry, entryScore) in selected)
                    {
                        rank++;
                        if (action.ContentAction.HasValue)
                        {
                            lock (entry.Deltas)
                            {
                                if (!entry.Deltas.ContentActionIds.Contains(action.ContentAction.Value))
                                    entry.Deltas.ContentActionIds.Add(action.ContentAction.Value);
                            }
                            RecordChange("add-action", entry, contentActionName, action.ContentAction.Value.ToString());
                        }
                        if (!string.IsNullOrWhiteSpace(action.Into))
                        {
                            lock (env.Context.Sync)
                            {
                                var into = env.Context.GetCollection(action.Into!);
                                if (!into.Any(e => e.Key == entry.Key)) into.Add(entry);
                            }
                        }
                        // One entry per selected item, carrying the content id, so the log can be
                        // filtered to 'was this story selected, at what rank, on what score'.
                        env.Log.LogDecision(step.Name, actionName, action.Type, entry.Kind == "existing" ? entry.Id : null, Outcomes.Executed,
                            $"Selected #{rank} of {selected.Count} for '{objective}': {EntryLabel(entry)} scored {entryScore}"
                            + $"{(action.Into != null ? $"; added to {action.Into}" : "")}"
                            + $"{(stampAction ? $"; stamped '{contentActionName}'" : "")}.",
                            JsonSerializer.Serialize(new { objective, rank, score = entryScore, contentRef = EntryRef(entry), into = action.Into, contentAction = stampAction ? contentActionName : null }, _jsonOptions));
                    }
                    var sortedBy = "score descending, then content id ascending (no LLM - the recorded scores are ranked)";
                    var rule = minScore.HasValue
                        ? $"every item scoring {minScore} or higher{(take.HasValue ? $", capped at {take}" : "")}"
                        : $"the top {take}";
                    lock (env.Summary.Selections)
                    {
                        env.Summary.Selections.Add(new SelectionSummary
                        {
                            Objective = objective,
                            Step = step.Name,
                            Action = actionName,
                            SortedBy = sortedBy,
                            Rule = rule,
                            Take = take,
                            MinScore = minScore,
                            Qualified = qualified.Count,
                            Candidates = candidates.Count,
                            Into = action.Into,
                            ContentAction = contentActionName,
                            Distribution = distribution,
                            Unresolved = unresolved,
                            Selected = selected.Select(s => new ScoredItemSummary
                            {
                                ContentRef = EntryRef(s.Entry),
                                Score = s.Score,
                                Headline = Truncate(s.Entry.GetField("headline") ?? "", SummaryHeadlineLength),
                                Step = step.Name,
                            }).ToList(),
                        });
                    }
                    LogExecuted(
                        $"Selected {selected.Count} of {candidates.Count} item(s) scored for '{objective}' - kept {rule}, ranked by {sortedBy}"
                        + (minScore.HasValue ? $"; {qualified.Count} item(s) met the {minScore} threshold" : "")
                        + $"{(action.Into != null ? $"; into {action.Into}" : "")}"
                        + $"{(stampAction ? $"; stamping '{contentActionName}'" : "")}."
                        + $" Score distribution: {DescribeDistribution(distribution)}."
                        + $" Selected: {DescribeSelection(selected)}."
                        + (unresolved.Count > 0 ? $" {unresolved.Count} ranked item(s) no longer resolved and were dropped: {string.Join(", ", unresolved)}." : ""),
                        JsonSerializer.Serialize(new
                        {
                            objective,
                            rule,
                            sortedBy,
                            take,
                            minScore,
                            candidates = candidates.Count,
                            qualified = qualified.Count,
                            distribution = distribution.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                            selected = selected.Select(s => new { contentRef = EntryRef(s.Entry), score = s.Score, headline = Truncate(s.Entry.GetField("headline") ?? "", SummaryHeadlineLength) }),
                            unresolved,
                        }, _jsonOptions));
                    break;
                }
            case "report.run":
                {
                    if (!action.Report.HasValue) break;
                    var usingNote = action.Using != null ? $" using {action.Using}" : "";
                    if (env.IsDryRun)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, null, Outcomes.Executed, $"Dry run: report {action.Report} would be published{usingNote}.");
                        lock (env.Summary.Changes) env.Summary.Changes.Add(new ChangeSummary { Type = "run-report", ContentRef = action.Report.Value.ToString(), Step = step.Name, Value = action.Using });
                        break;
                    }
                    await _api.PublishReportAsync(action.Report.Value);
                    lock (env.Summary.Changes) env.Summary.Changes.Add(new ChangeSummary { Type = "run-report", ContentRef = action.Report.Value.ToString(), Step = step.Name, Value = action.Using });
                    LogExecuted($"Published report {action.Report}{usingNote}.");
                    break;
                }
            case "notification.run":
                {
                    if (!action.Notification.HasValue) break;
                    var usingNote = action.Using != null ? $" using {action.Using}" : "";
                    if (env.IsDryRun)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, null, Outcomes.Executed, $"Dry run: notification {action.Notification} would be published{usingNote}.");
                        lock (env.Summary.Changes) env.Summary.Changes.Add(new ChangeSummary { Type = "run-notification", ContentRef = action.Notification.Value.ToString(), Step = step.Name, Value = action.Using });
                        break;
                    }
                    await _api.PublishNotificationAsync(action.Notification.Value);
                    lock (env.Summary.Changes) env.Summary.Changes.Add(new ChangeSummary { Type = "run-notification", ContentRef = action.Notification.Value.ToString(), Step = step.Name, Value = action.Using });
                    LogExecuted($"Published notification {action.Notification}{usingNote}.");
                    break;
                }
            default:
                env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"Action type '{action.Type}' is not implemented.");
                break;
        }
    }

    private static ContentEntry? ResolveItem(string? item, ItemScope scope)
    {
        if (string.IsNullOrWhiteSpace(item) || item.Equals("$item", StringComparison.OrdinalIgnoreCase)) return scope.Subject;
        return scope.Drafts.TryGetValue(item, out var draft) ? draft : null;
    }

    /// <summary>
    /// Resolve a content action's display name for readable change records. Deliberately
    /// unfiltered: naming an action that has since been disabled must still work, or the log
    /// would report an id where it used to report a name.
    /// </summary>
    private static string ContentActionName(int id, LookupModel? lookups)
        => lookups?.Actions.FirstOrDefault(a => a.Id == id)?.Name ?? $"action {id}";

    /// <summary>
    /// Whether a content action may still be applied. A profile keeps the id it was authored with,
    /// so an action disabled since then would otherwise keep being stamped on every run.
    /// An unavailable lookup bundle is not treated as 'disabled' - that would stop every stamp.
    /// </summary>
    private static bool IsContentActionEnabled(int id, LookupModel? lookups)
        => lookups?.Actions.FirstOrDefault(a => a.Id == id)?.IsEnabled ?? true;

    private static (int Id, string Name)? FindContributor(string name, LookupModel? lookups)
    {
        if (lookups == null) return null;
        foreach (var contributor in lookups.Contributors.Where(c => c.IsEnabled))
        {
            if (contributor.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return (contributor.Id, contributor.Name);
            var aliases = (contributor.Aliases ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (aliases.Any(alias => alias.Equals(name, StringComparison.OrdinalIgnoreCase))) return (contributor.Id, contributor.Name);
        }
        return null;
    }
    #endregion

    #region Deduplication
    /// <summary>
    /// The complete default comparison prompt - what is sent IS this text, with the tokens
    /// replaced. There is no hidden assembly: a custom prompt places {content} and
    /// {candidates} (or {candidate.*} fields in iterate mode) wherever it wants them.
    /// </summary>
    private const string DefaultDedupePrompt =
        "Compare the CURRENT story to each CANDIDATE story. Two stories are duplicates when they " +
        "have the same (or a trivially reworded) headline, the same story text (the summary, or " +
        "the body when there is no summary), and the same published date. " +
        "If a candidate is a duplicate of the current story respond with \"[DUPLICATE:{value}]\" " +
        "where {value} is the contentId of that candidate. If none are duplicates respond with nothing." +
        "\n\n## Current Story\n{content}\n\n## Candidates\n{candidates}";

    private static readonly System.Text.RegularExpressions.Regex _candidateFieldToken =
        new(@"\{candidate\.(?<field>[a-zA-Z.]+)\}", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Resolve one digest field of a comparison candidate for {candidate.*} tokens.</summary>
    private static string? ResolveCandidateField(ContentEntry entry, string field) => field.ToLowerInvariant() switch
    {
        "contentid" or "id" => entry.Kind == "draft" ? entry.TempKey : entry.Id.ToString(),
        "story" => string.IsNullOrWhiteSpace(entry.GetField("summary")) ? entry.GetField("body") : entry.GetField("summary"),
        _ => entry.GetField(field),
    };

    /// <summary>
    /// Compare the subject against a collection's candidates in iterate mode (one prompt per
    /// candidate) or batch mode (one prompt per chunk, the response naming the matched id).
    /// </summary>
    private async Task DetectDuplicateAsync(StepDefinition step, ActionDefinition action, ItemScope scope, RunEnvironment env, StepSummary stepSummary)
    {
        var subject = scope.Subject!;
        var actionName = action.Name ?? action.Type;
        var contentId = subject.Kind == "existing" ? subject.Id : (long?)null;
        var remember = action.Remember == true;

        // Dedupe memory: an item already linked as a duplicate needs no comparison at all.
        if (remember && subject.Kind == "existing")
        {
            try
            {
                var known = (await _api.FindContentLinksAsync(subject.Id, "duplicate")).FirstOrDefault();
                if (known != null)
                {
                    var matched = (known.ContentId == subject.Id ? known.LinkId : known.ContentId).ToString();
                    lock (env.Summary.Changes) env.Summary.Changes.Add(new ChangeSummary
                    {
                        Type = "duplicate",
                        ContentRef = subject.Id.ToString(),
                        Value = matched,
                        Step = step.Name,
                    });
                    StoreDedupeResult(scope, actionName, true, matched);
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Confirmed, $"Known duplicate of {matched} (content link); no comparison sent.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read content links for {id}; falling back to comparison.", subject.Id);
            }
        }

        List<ContentEntry> candidates;
        lock (env.Context.Sync)
        {
            candidates = env.Context.Collections.TryGetValue(action.Against ?? "", out var list)
                ? list.Where(e => e.Key != subject.Key).ToList()
                : new List<ContentEntry>();
        }
        if (candidates.Count == 0)
        {
            StoreDedupeResult(scope, actionName, false, null);
            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Skipped, $"No candidates in {action.Against}; recorded {actionName}.isDuplicate = false.");
            return;
        }
        if (action.MaxComparisons is > 0 && candidates.Count > action.MaxComparisons.Value)
        {
            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Info, $"Candidates capped at {action.MaxComparisons} of {candidates.Count}.");
            candidates = candidates.Take(action.MaxComparisons.Value).ToList();
        }

        var isBatch = string.Equals(action.Mode, "batch", StringComparison.OrdinalIgnoreCase);
        var batchSize = isBatch ? Math.Max(1, action.BatchSize ?? 25) : 1;
        // No prompt selected: a 'default-dedupe' library entry overrides the built-in text.
        var promptText = action.Prompt != null
            ? env.Prompts.Resolve(action.Prompt)
            : env.Prompts.TryResolveRef("default-dedupe") ?? DefaultDedupePrompt;
        var confirm = string.IsNullOrWhiteSpace(action.Confirm) ? "[DUPLICATE:{value}]" : action.Confirm!;
        var matcher = new ConfirmationMatcher(confirm, null, null);
        var llm = await ResolveLlmAsync(action.LlmId ?? step.LlmId ?? env.Profile.LLMId, env);
        var subjectJson = subject.ToWorkingJson(_jsonOptions);

        for (var offset = 0; offset < candidates.Count; offset += batchSize)
        {
            var batch = candidates.Skip(offset).Take(batchSize).ToList();
            var digest = batch.Select(e =>
            {
                var view = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["contentId"] = e.Kind == "draft" ? e.TempKey : e.Id.ToString(),
                    ["headline"] = e.GetField("headline"),
                    ["byline"] = e.GetField("byline"),
                    ["source"] = e.GetField("source"),
                    ["publishedOn"] = e.GetField("publishedOn"),
                    ["story"] = string.IsNullOrWhiteSpace(e.GetField("summary")) ? e.GetField("body") : e.GetField("summary"),
                };
                return view;
            }).ToList();
            var previousJson = isBatch
                ? JsonSerializer.Serialize(digest, _jsonOptions)
                : JsonSerializer.Serialize(digest[0], _jsonOptions);
            // The prompt IS what is sent: tokens are substituted in place and nothing is
            // appended invisibly. A prompt without data tokens sends no story data (the
            // validator warns about that at save).
            var body = env.Prompts.Substitute(promptText, subject, appendSubject: false);
            if (isBatch)
            {
                // Field-level candidate tokens are per-candidate; a batch has many.
                body = _candidateFieldToken.Replace(body, "(batch mode: use {candidates})");
            }
            else
            {
                var candidate = batch[0];
                body = _candidateFieldToken.Replace(body, match => ResolveCandidateField(candidate, match.Groups["field"].Value) ?? "");
            }
            var prompt = body.Replace("{candidates}", previousJson).Replace("{candidate}", previousJson);

            var timer = Stopwatch.StartNew();
            LlmResult result;
            try
            {
                result = await _llm.InvokeAsync(new LlmEndpoint(llm.ProjectEndpoint!, llm.ApiKey!, llm.DeploymentName!), new[] { ("user", prompt) }, false, Math.Max(1, _options.LLMRequestAttempts));
            }
            catch (Exception ex)
            {
                timer.Stop();
                env.Log.LogLlm(step.Name, null, actionName, action.Type, contentId, prompt, "", null, null, timer.ElapsedMilliseconds, Outcomes.Failed, 1, $"{{\"error\":{JsonSerializer.Serialize(ex.Message)}}}");
                continue;
            }
            timer.Stop();

            var confirmed = matcher.IsValid && matcher.TryMatch(result.Content, out var capturedValue);
            // Iterate mode compares exactly one candidate, so a plain [DUPLICATE] answer is
            // unambiguous; accept it alongside the default [DUPLICATE:{value}] marker.
            if (!confirmed && !isBatch && string.IsNullOrWhiteSpace(action.Confirm)
                && result.Content.Contains("[DUPLICATE]", StringComparison.OrdinalIgnoreCase))
                confirmed = true;
            string? matchedRef = null;
            if (confirmed)
            {
                matchedRef = isBatch
                    ? batch.FirstOrDefault(e => (e.Kind == "draft" ? e.TempKey : e.Id.ToString()) == (matcher.TryMatch(result.Content, out var v) ? (v ?? "").Trim() : ""))
                        is { } matched ? (matched.Kind == "draft" ? matched.TempKey : matched.Id.ToString()) : null
                    : batch[0].Kind == "draft" ? batch[0].TempKey : batch[0].Id.ToString();
                if (isBatch && matchedRef == null)
                {
                    env.Log.LogLlm(step.Name, null, actionName, action.Type, contentId, prompt, result.Content, result.PromptTokens, result.CompletionTokens, timer.ElapsedMilliseconds, Outcomes.NotConfirmed, result.Attempts,
                        "{\"note\":\"the response named an id that is not in the batch\"}");
                    continue;
                }
            }

            env.Log.LogLlm(step.Name, null, actionName, action.Type, contentId, prompt, result.Content, result.PromptTokens, result.CompletionTokens, timer.ElapsedMilliseconds,
                confirmed ? Outcomes.Confirmed : Outcomes.NotConfirmed, result.Attempts,
                confirmed ? $"{{\"duplicateOf\":{JsonSerializer.Serialize(matchedRef)}}}" : null);

            if (!confirmed) continue;

            lock (env.Summary.Changes) env.Summary.Changes.Add(new ChangeSummary
            {
                Type = "duplicate",
                ContentRef = subject.Kind == "draft" ? subject.TempKey ?? "" : subject.Id.ToString(),
                Value = matchedRef,
                Step = step.Name,
            });

            // Pure detector: record the answer and stop comparing. Later actions route on the
            // result with condition gates - the action itself decides nothing.
            StoreDedupeResult(scope, actionName, true, matchedRef);
            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Confirmed, $"Duplicate of {matchedRef}; recorded {actionName}.isDuplicate = true.");

            // Dedupe memory: persist the confirmed pair so later runs skip the comparison.
            // Real runs only - a dry run writes nothing.
            if (remember && !env.IsDryRun && subject.Kind == "existing" && long.TryParse(matchedRef, out var matchedContentId))
            {
                try
                {
                    await _api.AddContentLinkAsync(subject.Id, matchedContentId, "duplicate");
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.Info, $"Recorded content link {subject.Id} -> {matchedContentId} (duplicate).");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record the duplicate content link {id} -> {matched}.", subject.Id, matchedRef);
                }
            }
            return;
        }

        StoreDedupeResult(scope, actionName, false, null);
        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, Outcomes.NotConfirmed, $"No duplicate found among {candidates.Count} candidate(s); recorded {actionName}.isDuplicate = false.");
    }

    /// <summary>
    /// Publish a dedupe result into the item scope under the action's name, in the same stores
    /// analyses use - so conditions ('name.isDuplicate'), value sources ('name.matchedId'), and
    /// the lazy-analysis resolver all see it like any other analysis answer.
    /// </summary>
    private static void StoreDedupeResult(ItemScope scope, string name, bool isDuplicate, string? matchedRef)
    {
        scope.Raw[name] = isDuplicate ? $"[DUPLICATE:{matchedRef}]" : "";
        scope.Structured[name] = JsonDocument.Parse(JsonSerializer.Serialize(new { isDuplicate, matchedId = matchedRef }, _jsonOptions));
    }
    #endregion

    /// <summary>
    /// Close the log with one entry per scoring objective: how many items were scored, how many
    /// carried each score, and the highest-scoring stories. Score actions log per item, which is
    /// the detail; this is the shape of the whole objective in one place.
    /// </summary>
    private static void ReportScoreObjectives(RunEnvironment env)
    {
        foreach (var objective in env.Summary.Scores)
        {
            var top = objective.Items.Take(MaxLoggedItems)
                .Select((item, position) => $"#{position + 1} content {item.ContentRef}{(string.IsNullOrWhiteSpace(item.Headline) ? "" : $" \"{item.Headline}\"")} ({item.Score})");
            env.Log.LogDecision("end-of-run", null, "score", null, Outcomes.Info,
                $"Objective '{objective.Objective}': {objective.Items.Count} item(s) scored by {string.Join(", ", objective.Steps)}"
                + (objective.Unscored > 0 ? $", {objective.Unscored} not scored (the value was not a number)" : "")
                + $". Score distribution: {DescribeDistribution(objective.Distribution)}."
                + (objective.Items.Count > 0
                    ? $" Highest scoring: {string.Join("; ", top)}{(objective.Items.Count > MaxLoggedItems ? $"; and {objective.Items.Count - MaxLoggedItems} more" : "")}."
                    : ""),
                JsonSerializer.Serialize(new
                {
                    objective = objective.Objective,
                    steps = objective.Steps,
                    scored = objective.Items.Count,
                    unscored = objective.Unscored,
                    distribution = objective.Distribution.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                }, _jsonOptions));
        }
    }

    #region Flushing
    /// <summary>
    /// Report every entry still dirty (or an unsaved draft) at the end of the run: nothing
    /// auto-saves, so anything not covered by a Save Collection / Save Content Now action is
    /// surfaced in the log and summary instead of being silently dropped.
    /// </summary>
    private static void ReportUnwritten(RunEnvironment env)
    {
        foreach (var entry in env.Context.GetFlushables())
        {
            var reference = entry.Kind == "draft" ? entry.TempKey ?? entry.Key : entry.Id.ToString();

            // What is pending: the specific fields/flags that never reached the database.
            string pending;
            lock (entry.Deltas)
            {
                var parts = new List<string>();
                if (entry.Kind == "draft") parts.Add("draft never created");
                if (entry.Deltas.Fields.Count > 0) parts.Add(string.Join(", ", entry.Deltas.Fields.Keys));
                if (entry.Deltas.Tags.Count > 0) parts.Add($"{entry.Deltas.Tags.Count} tag(s)");
                if (entry.Deltas.Sentiment.HasValue) parts.Add("sentiment");
                if (entry.Deltas.ContributorId.HasValue) parts.Add("contributor");
                if (entry.Deltas.ContentActionIds.Count > 0) parts.Add($"{entry.Deltas.ContentActionIds.Count} content action(s)");
                if (entry.Deltas.Status != null) parts.Add(entry.Deltas.Status);
                pending = parts.Count > 0 ? string.Join("; ", parts) : "changes";
            }

            // Where it lives: the collections that would write it via a Save Collection action.
            string collections;
            lock (env.Context.Sync)
                collections = string.Join(", ", env.Context.Collections
                    .Where(kv => kv.Value.Any(e => e.Key == entry.Key))
                    .Select(kv => kv.Key));
            var hint = collections.Length > 0
                ? $"in {collections} - a Save Collection action on one of these would write it"
                : "in no collection - use Save Content Now in the step that changed it";

            lock (env.Summary.FlushFailures)
                env.Summary.FlushFailures.Add($"{reference}: unsaved ({pending}); {hint}.");
            env.Log.LogDecision("end-of-run", null, null, entry.Kind == "existing" ? entry.Id : null, Outcomes.Skipped, $"{entry.Key}: unsaved ({pending}); {hint}.");
        }
    }

    /// <summary>
    /// Persist one entry: drafts are created (obtaining a database id, recorded for the summary),
    /// existing items are fetched and updated with all deltas applied in one request with indexing.
    /// The fields the write carries are described before the deltas are cleared, so the decision
    /// log and the run outcome name what was written rather than counting it. An entry with no
    /// pending deltas returns without a save record - there was nothing to write.
    /// </summary>
    private async Task FlushEntryAsync(ContentEntry entry, RunEnvironment env, string stepName, bool index, string? actionName = null, string? actionType = null, string? collection = null)
    {
        var written = DescribeDeltas(entry, env.Lookups);
        var headline = Truncate(entry.GetField("headline") ?? "", SummaryHeadlineLength);
        if (entry.Kind == "draft")
        {
            var model = BuildDraftModel(entry);
            ApplyDeltas(entry, model, env);
            var publish = entry.Deltas.Status == "publish";
            // Create with the final status. A Draft-then-publish two-step makes the indexing
            // service request notifications for the DRAFT (alert rules fail against it) and its
            // dedupe cache then swallows the publish's own request - so alerts never send. With
            // Publish on the create, the first index pass publishes and notifies in one motion.
            model.Status = publish ? Entities.ContentStatus.Publish : Entities.ContentStatus.Draft;
            var created = await _api.AddContentAsync(model)
                ?? throw new InvalidOperationException("The API returned no content for the created draft.");
            if (!publish && index)
            {
                created = await _api.UpdateContentAsync(created, index: true) ?? created;
            }
            var tempKey = entry.TempKey ?? "";
            entry.Kind = "existing";
            entry.Id = created.Id;
            lock (entry.Deltas) ClearDeltas(entry.Deltas);
            lock (env.Context.Sync) env.Context.DraftIds[tempKey] = created.Id;
            RecordSave(env, entry, stepName, actionName, collection, written, "created", index, null, headline, created.Id.ToString());
            env.Log.LogDecision(stepName, actionName, actionType, created.Id, Outcomes.Flushed,
                $"Draft {tempKey} created as content {created.Id}{(publish ? " and published" : "")}"
                + (written.Count > 0 ? $" - wrote {string.Join(", ", written)}" : "")
                + $"{(headline.Length > 0 ? $" (\"{headline}\")" : "")}.",
                JsonSerializer.Serialize(new { tempKey, contentId = created.Id, published = publish, indexed = index, fields = written }, _jsonOptions));
            return;
        }

        bool dirty;
        lock (entry.Deltas) dirty = entry.Deltas.Dirty;
        if (!dirty) return;

        var content = await _api.FindContentByIdAsync(entry.Id)
            ?? throw new InvalidOperationException($"Content {entry.Id} could not be found to apply changes.");
        ApplyDeltas(entry, content, env);
        await _api.UpdateContentAsync(content, index);
        lock (entry.Deltas) ClearDeltas(entry.Deltas);
        RecordSave(env, entry, stepName, actionName, collection, written, "saved", index, null, headline);
        env.Log.LogDecision(stepName, actionName, actionType, entry.Id, Outcomes.Flushed,
            $"Saved {EntryLabel(entry)} in one update - wrote {(written.Count > 0 ? string.Join(", ", written) : "nothing")}."
            + (index ? " Indexed." : " Not indexed."),
            JsonSerializer.Serialize(new { contentId = entry.Id, indexed = index, fields = written }, _jsonOptions));
    }

    private ContentModel BuildDraftModel(ContentEntry entry)
    {
        var model = new ContentModel
        {
            Headline = entry.GetField("headline") ?? "",
            Byline = entry.GetField("byline") ?? "",
            Summary = entry.GetField("summary") ?? "",
            Body = entry.GetField("body") ?? "",
            Section = entry.GetField("section") ?? "",
            Page = entry.GetField("page") ?? "",
            Edition = entry.GetField("edition") ?? "",
            OtherSource = entry.GetField("otherSource") ?? entry.GetField("source") ?? "",
            Uid = entry.GetField("uid") ?? $"{entry.TempKey}-{DateTime.UtcNow:yyyyMMdd}",
        };
        if (int.TryParse(entry.GetField("sourceId"), out var sourceId)) model.SourceId = sourceId;
        if (int.TryParse(entry.GetField("licenseId"), out var licenseId)) model.LicenseId = licenseId;
        if (int.TryParse(entry.GetField("mediaTypeId"), out var mediaTypeId)) model.MediaTypeId = mediaTypeId;
        if (Enum.TryParse<Entities.ContentType>(entry.GetField("contentType"), true, out var contentType)) model.ContentType = contentType;
        // Published content without a published-on date is invisible to every date-filtered query.
        // publishedOnUtc carries the original's exact timestamp; the bare comparison date is the
        // fallback (parsed as editorial-timezone midnight).
        var publishedOnUtc = entry.GetField("publishedOnUtc");
        model.PublishedOn = ParsePublishedOn(string.IsNullOrWhiteSpace(publishedOnUtc) ? entry.GetField("publishedOn") : publishedOnUtc) ?? DateTime.UtcNow;
        return model;
    }

    /// <summary>
    /// The digest renders publishedOn as a bare date (yyyy-MM-dd). Midnight UTC would read as the
    /// previous day in the editorial timezone - date-scoped searches (US/Pacific) would exclude the
    /// item - so a bare date means local midnight in the configured timezone, converted to UTC.
    /// A value carrying a time parses as UTC (postgres rejects Kind=Unspecified for timestamptz).
    /// </summary>
    private DateTime? ParsePublishedOn(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (DateTime.TryParseExact(text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
        {
            try
            {
                var zone = TimeZoneInfo.FindSystemTimeZoneById(_options.DefaultTimeZone);
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), zone);
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
        }
        return DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var full)
            ? full
            : null;
    }

    private void ApplyDeltas(ContentEntry entry, ContentModel content, RunEnvironment env)
    {
        lock (entry.Deltas)
        {
            foreach (var (field, value) in entry.Deltas.Fields) ApplyContentField(content, field, value);
            if (entry.Deltas.Tags.Count > 0)
            {
                var tags = content.Tags.ToList();
                foreach (var (tagId, code, name) in entry.Deltas.Tags.DistinctBy(t => t.Id))
                    if (!tags.Any(tag => tag.Id == tagId)) tags.Add(new ContentTagModel(tagId, code, name));
                content.Tags = tags;
            }
            if (entry.Deltas.ContributorId.HasValue) content.ContributorId = entry.Deltas.ContributorId.Value;
            if (entry.Deltas.Sentiment.HasValue)
            {
                var tonePools = content.TonePools.Where(pool => pool.Id != _options.DefaultTonePoolId).ToList();
                tonePools.Add(new ContentTonePoolModel { Id = _options.DefaultTonePoolId, ContentId = content.Id, Value = entry.Deltas.Sentiment.Value });
                content.TonePools = tonePools;
            }
            foreach (var actionId in entry.Deltas.ContentActionIds.Distinct())
            {
                // Defence in depth: the request sites already refuse a disabled action, so a
                // delta reaching here disabled means it was disabled mid-run. Never write it.
                var definition = env.Lookups?.Actions.FirstOrDefault(a => a.Id == actionId && a.IsEnabled);
                if (definition == null) continue;
                var value = definition.ValueType == Entities.ValueType.Boolean
                    ? "true"
                    : (!string.IsNullOrWhiteSpace(definition.DefaultValue) ? definition.DefaultValue : "true");
                var actions = content.Actions.ToList();
                var existing = actions.FirstOrDefault(a => a.Id == actionId);
                if (existing != null) existing.Value = value;
                else
                {
                    actions.Add(new ContentActionModel { Id = actionId, ContentId = content.Id, Value = value });
                    content.Actions = actions;
                }
            }
            if (entry.Deltas.Status == "publish") content.Status = Entities.ContentStatus.Publish;
            else if (entry.Deltas.Status == "unpublish") content.Status = Entities.ContentStatus.Unpublish;
        }
    }

    private static void ClearDeltas(Deltas deltas)
    {
        deltas.Fields.Clear();
        deltas.Tags.Clear();
        deltas.Sentiment = null;
        deltas.ContributorId = null;
        deltas.ContributorName = null;
        deltas.ContentActionIds.Clear();
        deltas.Status = null;
    }

    /// <summary>
    /// The content fields a save can actually write to an existing item. Kept beside
    /// <see cref="ApplyContentField"/>: a delta for anything else is accepted by the working copy
    /// and then dropped at flush, so the save report names it rather than claiming it was written.
    /// </summary>
    private static readonly HashSet<string> WritableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "headline", "byline", "summary", "body", "edition", "section", "page",
    };

    private static void ApplyContentField(ContentModel content, string field, string value)
    {
        switch (field.ToLowerInvariant())
        {
            case "headline": content.Headline = value; break;
            case "byline": content.Byline = value; break;
            case "summary": content.Summary = value; break;
            case "body": content.Body = value; break;
            case "edition": content.Edition = value; break;
            case "section": content.Section = value; break;
            case "page": content.Page = value; break;
        }
    }
    #endregion

    #region Helpers
    private static List<ComparisonDifference> BuildDifferences(VariantSummary a, VariantSummary b)
    {
        static Dictionary<string, HashSet<string>> Index(VariantSummary summary) => summary.Changes
            .GroupBy(c => c.ContentRef)
            .ToDictionary(g => g.Key, g => g.Select(c => $"{c.Type}{(c.Field != null ? $":{c.Field}" : "")}{(c.Value != null ? $"={c.Value}" : "")}").ToHashSet());

        var indexA = Index(a);
        var indexB = Index(b);
        var differences = new List<ComparisonDifference>();
        foreach (var key in indexA.Keys.Union(indexB.Keys).OrderBy(k => k))
        {
            var setA = indexA.TryGetValue(key, out var va) ? va : new HashSet<string>();
            var setB = indexB.TryGetValue(key, out var vb) ? vb : new HashSet<string>();
            var onlyA = setA.Except(setB).ToList();
            var onlyB = setB.Except(setA).ToList();
            if (onlyA.Count > 0 || onlyB.Count > 0)
                differences.Add(new ComparisonDifference { ContentRef = key, OnlyA = onlyA, OnlyB = onlyB });
        }
        return differences;
    }

    private static JsonDocument? ParseJsonLenient(string response)
    {
        var text = response.Trim();
        // Strip markdown fences some models wrap JSON in.
        if (text.StartsWith("```"))
        {
            var firstBreak = text.IndexOf('\n');
            if (firstBreak >= 0) text = text[(firstBreak + 1)..];
            if (text.EndsWith("```")) text = text[..^3];
            text = text.Trim();
        }
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            return JsonDocument.Parse(text[start..(end + 1)]);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Slug(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        slug = slug.Trim('-');
        return slug.Length > 0 ? slug : "item";
    }

    private static string Truncate(string value, int length)
        => value.Length <= length ? value : value[..length];

    /// <summary>The content id, or a draft's temp key before it has one.</summary>
    private static string EntryRef(ContentEntry entry)
        => entry.Kind == "draft" ? entry.TempKey ?? "" : entry.Id.ToString();

    /// <summary>
    /// A log-friendly label for an entry: the reference plus the headline, so a decision reads as
    /// a story rather than an opaque key.
    /// </summary>
    private static string EntryLabel(ContentEntry entry)
    {
        var reference = entry.Kind == "draft" ? $"draft {entry.TempKey}" : $"content {entry.Id}";
        var headline = entry.GetField("headline");
        return string.IsNullOrWhiteSpace(headline) ? reference : $"{reference} \"{Truncate(headline.Trim(), 120)}\"";
    }

    /// <summary>
    /// The tie-break for ranked scores: the content id, so items sharing a score always order the
    /// same way. Items are scored in parallel, so the order they land in the score table is not
    /// reproducible and cannot be the tie-break. Drafts (no id yet) sort last.
    /// </summary>
    private static long RankId(string key)
        => key.StartsWith("id:", StringComparison.Ordinal) && long.TryParse(key[3..], out var id) ? id : long.MaxValue;

    /// <summary>How many items carry each score, highest score first.</summary>
    private static string DescribeDistribution(IReadOnlyDictionary<int, int> distribution)
        => distribution.Count == 0
            ? "no scores were recorded"
            : string.Join(", ", distribution.OrderByDescending(entry => entry.Key).Select(entry => $"{entry.Value} item(s) scored {entry.Key}"));

    /// <summary>
    /// The selected items in rank order, each with its score. Long selections are trimmed - the
    /// per-item log entries and the run outcome carry the whole list.
    /// </summary>
    private static string DescribeSelection(IReadOnlyList<(ContentEntry Entry, int Score)> selected)
        => selected.Count == 0
            ? "none"
            : string.Join("; ", selected.Take(MaxLoggedItems).Select((item, position) => $"#{position + 1} {EntryLabel(item.Entry)} ({item.Score})"))
                + (selected.Count > MaxLoggedItems ? $"; and {selected.Count - MaxLoggedItems} more" : "");

    /// <summary>
    /// The fields a save would write for this entry, named rather than counted. Read before the
    /// flush, which clears the deltas. A field an update cannot write (see
    /// <see cref="WritableFields"/>) is named as ignored rather than reported as written.
    /// </summary>
    private static List<string> DescribeDeltas(ContentEntry entry, LookupModel? lookups)
    {
        var written = new List<string>();
        lock (entry.Deltas)
        {
            // A draft is built from its whole digest, so every field it carries is written.
            written.AddRange(entry.Deltas.Fields.Keys
                .OrderBy(field => field, StringComparer.OrdinalIgnoreCase)
                .Select(field => entry.Kind == "draft" || WritableFields.Contains(field) ? field : $"{field} (ignored - not a writable field)"));
            if (entry.Deltas.Tags.Count > 0)
                written.Add($"tags ({string.Join(", ", entry.Deltas.Tags.Select(tag => tag.Code).Distinct(StringComparer.OrdinalIgnoreCase))})");
            if (entry.Deltas.Sentiment.HasValue)
                written.Add($"sentiment ({entry.Deltas.Sentiment.Value})");
            if (entry.Deltas.ContributorId.HasValue)
                written.Add($"contributor ({entry.Deltas.ContributorName ?? entry.Deltas.ContributorId.Value.ToString()})");
            if (entry.Deltas.ContentActionIds.Count > 0)
                written.Add($"actions ({string.Join(", ", entry.Deltas.ContentActionIds.Distinct().Select(id => ContentActionName(id, lookups)))})");
            if (entry.Deltas.Status != null)
                written.Add($"status ({entry.Deltas.Status})");
        }
        return written;
    }

    /// <summary>What a save is about to write, phrased for a decision-log sentence.</summary>
    private static string DescribeWrite(ContentEntry entry, IReadOnlyList<string> written)
        => written.Count > 0
            ? $"{(entry.Kind == "draft" ? "creating the item and writing" : "writing")} {string.Join(", ", written)}"
            : entry.Kind == "draft" ? "creating the item" : "nothing to write";

    /// <summary>
    /// Record one item written (or, on a dry run, intended) by a save action, with the fields the
    /// write carried.
    /// </summary>
    private static void RecordSave(RunEnvironment env, ContentEntry entry, string stepName, string? actionName, string? collection, List<string> fields, string outcome, bool index, string? error, string? headline = null, string? contentRef = null)
    {
        var save = new SaveSummary
        {
            ContentRef = contentRef ?? EntryRef(entry),
            Step = stepName,
            Action = actionName,
            Collection = collection,
            Headline = headline ?? Truncate(entry.GetField("headline") ?? "", SummaryHeadlineLength),
            Fields = fields.ToList(),
            Outcome = outcome,
            Indexed = index,
            Error = error,
        };
        lock (env.Summary.Saves) env.Summary.Saves.Add(save);
    }

    /// <summary>
    /// Record one score against its objective so the outcome lists which items were scored and at
    /// what. A rescored item replaces its earlier score, exactly as the run's score table does.
    /// </summary>
    private static void RecordScore(RunEnvironment env, string objective, string stepName, ContentEntry entry, int score)
    {
        var item = new ScoredItemSummary
        {
            ContentRef = EntryRef(entry),
            Score = score,
            Headline = Truncate(entry.GetField("headline") ?? "", SummaryHeadlineLength),
            Step = stepName,
        };
        lock (env.Summary.Scores)
        {
            var objectiveSummary = GetObjectiveSummary(env, objective, stepName);
            objectiveSummary.Items.RemoveAll(existing => existing.ContentRef == item.ContentRef);
            objectiveSummary.Items.Add(item);
        }
    }

    /// <summary>Count an item whose value was not an integer, so a short objective is never silent.</summary>
    private static void RecordUnscored(RunEnvironment env, string objective, string stepName)
    {
        lock (env.Summary.Scores) GetObjectiveSummary(env, objective, stepName).Unscored++;
    }

    /// <summary>Get (or add) the objective's summary. Callers hold the Scores lock.</summary>
    private static ScoreObjectiveSummary GetObjectiveSummary(RunEnvironment env, string objective, string stepName)
    {
        var objectiveSummary = env.Summary.Scores.FirstOrDefault(s => s.Objective.Equals(objective, StringComparison.OrdinalIgnoreCase));
        if (objectiveSummary == null)
        {
            objectiveSummary = new ScoreObjectiveSummary { Objective = objective };
            env.Summary.Scores.Add(objectiveSummary);
        }
        if (!objectiveSummary.Steps.Contains(stepName)) objectiveSummary.Steps.Add(stepName);
        return objectiveSummary;
    }

    /// <summary>
    /// Order every objective's scored items highest first and compute its distribution, so the
    /// outcome can show how many items carried each score without recounting in the browser.
    /// </summary>
    private static void FinalizeScoreSummaries(VariantSummary summary)
    {
        foreach (var objective in summary.Scores)
        {
            objective.Items = objective.Items
                .OrderByDescending(item => item.Score)
                .ThenBy(item => RankId(item.ContentRef.All(char.IsDigit) ? $"id:{item.ContentRef}" : item.ContentRef))
                .ThenBy(item => item.ContentRef, StringComparer.Ordinal)
                .ToList();
            objective.Distribution = objective.Items
                .GroupBy(item => item.Score)
                .OrderByDescending(group => group.Key)
                .ToDictionary(group => group.Key, group => group.Count());
        }
    }
    #endregion
    #endregion
}
