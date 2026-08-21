using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TNO.AI;
using TNO.API.Areas.Admin.Models.Automation.V2;
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

namespace TNO.Services.Automation.V2;

/// <summary>
/// V2Engine class, executes schema-version-2 automation profiles.
/// Concepts (see docs/planning/mmi-automation/09-engine-v2.md):
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
public class V2Engine
{
    #region Variables
    private const int FilterPageSize = 500;
    private const int MaxResultWindow = 10000;
    private const int DefaultSourceMax = 2000;
    private const int DefaultSearchMax = 500;

    private static readonly string[] DefaultDigestFields =
    {
        "id", "headline", "byline", "summary", "body", "publishedOn", "source", "otherSource",
        "section", "page", "edition", "status", "contentType", "sourceId", "licenseId", "mediaTypeId", "uid",
        "source.name", "source.code", "mediaType.name", "series.name", "contributor.name",
        "labels", "topics", "sentiment",
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
    /// Creates a new instance of a V2Engine.
    /// </summary>
    public V2Engine(IApiService api, ITNOElasticClient elasticClient, ElasticOptions elasticOptions, AutomationOptions options, ILogger logger)
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
    /// Execute the specified v2 profile for the specified run and return the outcome summary.
    /// </summary>
    public async Task<V2RunSummary> ExecuteAsync(AdminAutomationProfileModel profile, AdminAutomationRunModel run)
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
            return new V2RunSummary
            {
                IsDryRun = true,
                IsComparison = true,
                VariantA = a,
                VariantB = b,
                Differences = BuildDifferences(a, b),
            };
        }

        var summary = await ExecuteVariantAsync(profile, run, definition, null, run.IsDryRun);
        return new V2RunSummary { IsDryRun = run.IsDryRun, VariantA = summary };
    }

    /// <summary>
    /// Execute one definition (one comparison variant, or the whole run).
    /// </summary>
    private async Task<V2VariantSummary> ExecuteVariantAsync(AdminAutomationProfileModel profile, AdminAutomationRunModel run, AutomationDefinition definition, string? variant, bool isDryRun)
    {
        var runTimer = Stopwatch.StartNew();
        var summary = new V2VariantSummary();
        var context = new V2RunContext();
        var runLogger = new V2RunLogger(_api, run.Id, variant, _logger);
        var lookups = await _api.GetLookupsAsync();
        var prompts = new V2PromptBuilder(definition, lookups, context, _jsonOptions);
        var llmCache = new Dictionary<int, LLMModel>();
        var filterCache = new Dictionary<int, (string Query, string? Settings)>();
        var idSetCache = new Dictionary<int, HashSet<long>>();
        var parallelism = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, _options.MaxParallelContentItems) };

        if (isDryRun)
            runLogger.LogDecision("run", null, null, null, V2Outcomes.Info, "Dry run: every decision and change is computed and logged, nothing is written.");

        // Steps execute in declared order within the init → process → complete phase order.
        var steps = definition.Steps.Where(s => s.IsEnabled)
            .OrderBy(s => Array.IndexOf(V2Phases.All, s.Phase))
            .ToArray();

        var environment = new V2Environment(profile, run, definition, context, runLogger, prompts, lookups, llmCache, filterCache, idSetCache, summary, isDryRun);

        foreach (var step in steps)
        {
            // A deleted run must stop executing, not grind on with nowhere to record anything.
            if (runLogger.IsAbandoned)
                throw new InvalidOperationException("The run record was deleted while executing; the run was stopped.");
            var stepTimer = Stopwatch.StartNew();
            var stepSummary = new V2StepSummary { Name = step.Name, Phase = step.Phase };
            summary.Steps.Add(stepSummary);
            var llmCallsBefore = runLogger.LlmCalls;

            try
            {
                // Process steps always iterate; a complete step iterates its required source.
                // Once-natured actions (select-top, report.run, set operations, ...) keep their
                // once-per-step semantics inside an iterating step: they are partitioned out of
                // the per-item pass and executed exactly once after iteration completes.
                if (step.Phase != V2Phases.Init && step.Source != null)
                {
                    var perItemActions = step.Actions
                        .Where(a => !V2ActionCatalog.Types.TryGetValue(a.Type, out var d) || d.Phases.Contains(V2Phases.Process))
                        .ToList();
                    var onceActions = step.Actions
                        .Where(a => V2ActionCatalog.Types.TryGetValue(a.Type, out var d) && !d.Phases.Contains(V2Phases.Process))
                        .ToList();
                    var itemStep = CloneWithActions(step, perItemActions);

                    var entries = await ResolveSourceAsync(step, environment, stepSummary);
                    stepSummary.Items = entries.Count;
                    await Parallel.ForEachAsync(entries, parallelism, async (entry, _) =>
                    {
                        if (environment.Log.IsAbandoned) return;
                        try
                        {
                            await ExecuteStepInstanceAsync(itemStep, new V2ItemScope(entry), environment, stepSummary);
                        }
                        catch (Exception ex)
                        {
                            // One failed item must not fail the whole run.
                            lock (stepSummary) stepSummary.Failures++;
                            _logger.LogError(ex, "Step '{step}' failed for item {key}; continuing with the next item.", step.Name, entry.Key);
                            runLogger.LogDecision(step.Name, null, null, entry.Kind == "existing" ? entry.Id : null, V2Outcomes.Failed, $"Step instance failed: {ex.Message}");
                        }
                    });

                    if (onceActions.Count > 0)
                        await ExecuteStepInstanceAsync(CloneWithActions(step, onceActions), new V2ItemScope(null), environment, stepSummary);
                }
                else
                {
                    try
                    {
                        await ExecuteStepInstanceAsync(step, new V2ItemScope(null), environment, stepSummary);
                    }
                    catch (Exception ex)
                    {
                        stepSummary.Failures++;
                        _logger.LogError(ex, "Step '{step}' failed; continuing with the next step.", step.Name);
                        runLogger.LogDecision(step.Name, null, null, null, V2Outcomes.Failed, $"Step failed: {ex.Message}");
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
    private static V2StepDefinition CloneWithActions(V2StepDefinition step, List<V2ActionDefinition> actions) => new()
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
    private sealed record V2Environment(
        AdminAutomationProfileModel Profile,
        AdminAutomationRunModel Run,
        AutomationDefinition Definition,
        V2RunContext Context,
        V2RunLogger Log,
        V2PromptBuilder Prompts,
        LookupModel? Lookups,
        Dictionary<int, LLMModel> LlmCache,
        Dictionary<int, (string Query, string? Settings)> FilterCache,
        Dictionary<int, HashSet<long>> IdSetCache,
        V2VariantSummary Summary,
        bool IsDryRun);

    #region Source resolution
    /// <summary>
    /// Resolve a process step's content entries from its declared source, apply include/exclude
    /// gate filters (each gate filter resolves once per run to an id set), and skip items the run
    /// has excluded.
    /// </summary>
    private async Task<List<V2ContentEntry>> ResolveSourceAsync(V2StepDefinition step, V2Environment env, V2StepSummary stepSummary)
    {
        var source = step.Source ?? throw new InvalidOperationException($"Process step '{step.Name}' has no source.");
        List<V2ContentEntry> entries;
        switch (source.From)
        {
            case "collection":
                {
                    lock (env.Context.Sync)
                    {
                        entries = env.Context.Collections.TryGetValue(source.Collection ?? "", out var list)
                            ? list.ToList()
                            : new List<V2ContentEntry>();
                    }
                    break;
                }
            case "filter":
                {
                    var (query, settings) = await GetFilterQueryAsync(source.Filter!.Value, env);
                    entries = await SearchDigestEntriesAsync(query, settings, source.Fields, source.Max ?? DefaultSourceMax, null, env, step.Name);
                    break;
                }
            default: // profile
                {
                    if (string.IsNullOrWhiteSpace(env.Profile.FilterQuery))
                    {
                        env.Log.LogDecision(step.Name, null, null, null, V2Outcomes.Skipped, "The step sources the profile filter, but the profile has none.");
                        return new List<V2ContentEntry>();
                    }
                    entries = await SearchDigestEntriesAsync(env.Profile.FilterQuery!, env.Profile.FilterSettings, source.Fields, source.Max ?? DefaultSourceMax, null, env, step.Name);
                    break;
                }
        }

        // Gates: membership is a hash lookup per item; each distinct filter queried once per run.
        var includes = new List<HashSet<long>>();
        foreach (var filterId in source.Include) includes.Add(await GetFilterIdSetAsync(filterId, env));
        var excludes = new List<HashSet<long>>();
        foreach (var filterId in source.Exclude) excludes.Add(await GetFilterIdSetAsync(filterId, env));

        var eligible = new List<V2ContentEntry>();
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
                env.Log.LogDecision(step.Name, null, null, entry.Kind == "existing" ? entry.Id : null, V2Outcomes.Skipped, $"Item skipped: {skipReason}.");
            }
            else eligible.Add(entry);
        }
        return eligible;
    }

    private async Task<(string Query, string? Settings)> GetFilterQueryAsync(int filterId, V2Environment env)
    {
        if (env.FilterCache.TryGetValue(filterId, out var cached)) return cached;
        var filter = await _api.GetFilterAsync(filterId)
            ?? throw new InvalidOperationException($"Filter {filterId} does not exist.");
        var value = (filter.Query.RootElement.GetRawText(), (string?)JsonSerializer.Serialize(filter.Settings, _jsonOptions));
        env.FilterCache[filterId] = value;
        return value;
    }

    private async Task<HashSet<long>> GetFilterIdSetAsync(int filterId, V2Environment env)
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
    private async Task<List<V2ContentEntry>> SearchDigestEntriesAsync(string query, string? settings, List<string>? fields, int maxItems, Dictionary<string, int>? truncate, V2Environment env, string label)
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

        var entries = new List<V2ContentEntry>();
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
            env.Log.LogDecision(label, null, null, null, V2Outcomes.Info, $"Search truncated at {entries.Count} item(s) (max {maxItems}, result window {MaxResultWindow}).");
        return entries;
    }

    private static V2ContentEntry BuildEntry(ContentModel content, string[] fields, Dictionary<string, int>? truncate)
    {
        var entry = new V2ContentEntry { Kind = "existing", Id = content.Id };
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
                "tags" => string.Join(",", content.Tags.Select(t => t.Code)),
                "actions" => string.Join(",", content.Actions.Select(a => a.Name)),
                "source.name" => content.Source?.Name ?? content.OtherSource,
                "source.code" => content.Source?.Code ?? content.OtherSource,
                "mediatype.name" => content.MediaType?.Name,
                "series.name" => content.Series?.Name ?? content.OtherSeries,
                "contributor.name" => content.Contributor?.Name,
                "labels" => string.Join(",", content.Labels.Select(l => string.IsNullOrWhiteSpace(l.Value) ? l.Key : l.Value)),
                "topics" => string.Join(",", content.Topics.Select(t => t.Name)),
                "sentiment" => content.TonePools.FirstOrDefault()?.Value.ToString(),
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
    private async Task ExecuteStepInstanceAsync(V2StepDefinition step, V2ItemScope scope, V2Environment env, V2StepSummary stepSummary)
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
                if (!V2ActionCatalog.Types.TryGetValue(action.Type, out var descriptor))
                {
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"Action type '{action.Type}' is not registered.");
                    continue;
                }
                if (descriptor.RequiresSubject && subject == null)
                {
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, "The action requires an iterated item and the step has none.");
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
                        ? V2ConditionEvaluator.Evaluate(action.When, f => target?.GetField(f), reference => V2ValueResolver.ResolveBool(reference, scope))
                        : V2ConditionEvaluator.Evaluate(action.When, f => target?.GetField(f));
                    if (!result.Passed)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.ConditionFailed, result.Detail);
                        continue;
                    }
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.ConditionPassed, result.Detail);
                }

                // Gate 2: confirmation statement against a raw analysis response ({value} capture).
                string? captured = null;
                if (!string.IsNullOrWhiteSpace(action.Confirm))
                {
                    var analysisName = action.Analysis ?? (step.Analyses.Count == 1 ? step.Analyses[0].Name : null);
                    if (analysisName == null)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, "Confirm requires a named analysis.");
                        continue;
                    }
                    var response = await EnsureAnalysisAsync(step, analysisName, scope, env, stepSummary) ?? "";
                    var matcher = new ConfirmationMatcher(action.Confirm, action.Field, action.Objective);
                    if (!matcher.IsValid)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, "Invalid confirmation statement.");
                        continue;
                    }
                    if (!matcher.TryMatch(response, out captured))
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.NotConfirmed,
                            string.IsNullOrWhiteSpace(response) ? "No confirmation; the response was empty (no criteria met)." : "The confirmation statement was not found in the response.");
                        continue;
                    }
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Confirmed, $"Confirmed by analysis '{analysisName}'{(captured != null ? $" with value '{Truncate(captured, 200)}'" : "")}.");
                }

                await ExecuteActionAsync(step, action, descriptor, scope, captured, env, stepSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Action '{action}' ({type}) in step '{step}' failed; skipping it.", actionName, action.Type, step.Name);
                env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Failed, $"Action failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Ensure the analysis a 'name.key' reference names has run (lazily) for this item.
    /// </summary>
    /// <summary>Collect every 'analysisName.key' reference anywhere in a condition tree.</summary>
    private static void CollectFromRefs(V2ConditionDefinition condition, List<string> references)
    {
        if (!string.IsNullOrWhiteSpace(condition.From)) references.Add(condition.From!);
        if (condition.Not != null) CollectFromRefs(condition.Not, references);
        foreach (var child in condition.All ?? Enumerable.Empty<V2ConditionDefinition>()) CollectFromRefs(child, references);
        foreach (var child in condition.Any ?? Enumerable.Empty<V2ConditionDefinition>()) CollectFromRefs(child, references);
    }

    private async Task EnsureAnalysisForReferenceAsync(V2StepDefinition step, string reference, V2ItemScope scope, V2Environment env, V2StepSummary stepSummary)
    {
        var name = reference.Split('.', 2)[0];
        if (!name.Equals("content", StringComparison.OrdinalIgnoreCase))
            await EnsureAnalysisAsync(step, name, scope, env, stepSummary);
    }

    /// <summary>
    /// Run the named analysis for this item if it has not run yet, and return its raw response.
    /// Lazy: an item whose actions never consume an analysis never pays for it.
    /// </summary>
    private async Task<string?> EnsureAnalysisAsync(V2StepDefinition step, string name, V2ItemScope scope, V2Environment env, V2StepSummary stepSummary)
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
        var text = env.Prompts.Resolve(analysis.Prompt);
        // The first message of an exchange carries the working copy; later chained turns already have it.
        text = messages.Count == 0 ? env.Prompts.Substitute(text, subject) : env.Prompts.Substitute(text, null);
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
                V2Outcomes.Executed, result.Attempts, detail);
            return result.Content;
        }
        catch (Exception ex)
        {
            timer.Stop();
            env.Log.LogLlm(step.Name, name, null, null, contentId, text, "", null, null, timer.ElapsedMilliseconds, V2Outcomes.Failed, 1, $"{{\"error\":{JsonSerializer.Serialize(ex.Message)}}}");
            // Cache the failure as an empty response so the item does not retry per consuming action.
            scope.Raw[name] = "";
            return "";
        }
    }

    private static string FindChainRoot(V2StepDefinition step, V2AnalysisDefinition analysis)
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

    private async Task<LLMModel> ResolveLlmAsync(int? llmId, V2Environment env)
    {
        var id = llmId ?? env.Profile.LLMId ?? throw new InvalidOperationException("No LLM is configured for this analysis (analysis, step, and profile are all unset).");
        lock (env.LlmCache)
        {
            if (env.LlmCache.TryGetValue(id, out var cached)) return cached;
        }
        var llm = await _api.GetLLMAsync(id) ?? throw new InvalidOperationException($"LLM {id} does not exist.");
        if (llm.ProjectEndpoint == null) throw new InvalidOperationException($"LLM '{llm.Name}' is missing a project endpoint.");
        if (string.IsNullOrWhiteSpace(llm.DeploymentName) || string.IsNullOrWhiteSpace(llm.ApiKey))
            throw new InvalidOperationException($"LLM '{llm.Name}' requires a deployment name and API key; the v2 engine does not support agent-mode LLMs.");
        lock (env.LlmCache) env.LlmCache[id] = llm;
        return llm;
    }
    #endregion

    #region Action handlers
    /// <summary>
    /// Dispatch one gated action to its handler.
    /// </summary>
    private async Task ExecuteActionAsync(V2StepDefinition step, V2ActionDefinition action, V2ActionDescriptor descriptor, V2ItemScope scope, string? captured, V2Environment env, V2StepSummary stepSummary)
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
                : V2ValueResolver.Resolve(action.Value, scope, target ?? subject, env.Prompts);
            if (value == null && captured != null) value = captured;
        }
        else if (captured != null)
        {
            value = captured;
        }

        void LogExecuted(string description, string? detail = null)
            => env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Executed, description, detail);
        void RecordChange(string type, V2ContentEntry entry, string? field = null, string? changeValue = null)
        {
            var change = new V2ChangeSummary
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
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"Item '{action.Item ?? "$item"}' could not be resolved.");
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
                        removed = list.RemoveAll(e => !V2ConditionEvaluator.Evaluate(action.Where!, e.GetField).Passed);
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
                        IEnumerable<V2ContentEntry> result = action.Type switch
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
                    // Only tags that exist (matched by code or name) can be added - the same lookup
                    // data the {lookup:tags} token renders, so prompt and validation cannot disagree.
                    var requested = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var unmatched = new List<string>();
                    var added = new List<string>();
                    foreach (var request in requested)
                    {
                        var tag = env.Lookups?.Tags.FirstOrDefault(t =>
                            t.Code.Equals(request, StringComparison.OrdinalIgnoreCase) ||
                            t.Name.Equals(request, StringComparison.OrdinalIgnoreCase));
                        if (tag == null) { unmatched.Add(request); continue; }
                        lock (target.Deltas)
                        {
                            if (!target.Deltas.Tags.Any(t => t.Id == tag.Id)) target.Deltas.Tags.Add((tag.Id, tag.Code, tag.Name));
                        }
                        added.Add(tag.Code);
                    }
                    if (added.Count > 0) RecordChange("add-tags", target, null, string.Join(",", added));
                    LogExecuted($"Added {added.Count} tag(s) to {target.Key}.", unmatched.Count > 0 ? $"{{\"unmatched\":{JsonSerializer.Serialize(string.Join(",", unmatched))}}}" : null);
                    break;
                }
            case "content.sentiment":
                {
                    if (target == null || !int.TryParse((value ?? "").Trim(), out var sentiment))
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"Sentiment value '{value}' is not a number.");
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
                            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Executed, $"Dry run: contributor '{Truncate(contributorName, 100)}' would be created and selected.");
                            break;
                        }
                        try
                        {
                            var created = await _api.AddContributorAsync(contributorName);
                            if (created != null)
                            {
                                contributor = (created.Id, created.Name);
                                env.Context.CreatedContributors[contributorName] = contributor.Value;
                                env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Executed, $"Created contributor '{created.Name}' ({created.Id}).");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to create contributor '{name}'.", contributorName);
                            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Failed, $"Failed to create contributor '{Truncate(contributorName, 100)}': {ex.Message}");
                            break;
                        }
                    }
                    if (contributor == null)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"No contributor matched '{Truncate(contributorName, 100)}'.");
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
                    var draft = new V2ContentEntry
                    {
                        Kind = "draft",
                        TempKey = $"{Slug(action.As ?? "draft")}-{Interlocked.Increment(ref _draftCounter)}",
                    };
                    var copySource = action.CopyFrom != null && action.CopyFrom.Equals("$item", StringComparison.OrdinalIgnoreCase) ? subject : null;
                    if (copySource != null)
                    {
                        var copyFields = action.CopyFields is { Count: > 0 }
                            ? action.CopyFields
                            : new List<string> { "sourceId", "otherSource", "licenseId", "mediaTypeId", "publishedOn", "contentType" };
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
                            var setValue = V2ValueResolver.Resolve(source, scope, subject, env.Prompts);
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
                    List<V2ContentEntry> items;
                    lock (env.Context.Sync) items = env.Context.GetCollection(action.FromCollection!).ToList();
                    // Only items with something to write: dirty deltas or unsaved drafts.
                    var dirtyKeys = env.Context.GetFlushables().Select(e => e.Key).ToHashSet();
                    var toSave = items.Where(e => dirtyKeys.Contains(e.Key)).ToList();
                    var saved = 0;
                    foreach (var entry in toSave)
                    {
                        if (env.IsDryRun)
                        {
                            env.Log.LogDecision(step.Name, actionName, action.Type, entry.Kind == "existing" ? entry.Id : null, V2Outcomes.Flushed, $"Dry run: {entry.Key} would be saved.");
                            saved++;
                            continue;
                        }
                        try
                        {
                            await FlushEntryAsync(entry, env, step.Name, index: true);
                            saved++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to save {key}.", entry.Key);
                            lock (env.Summary.FlushFailures) env.Summary.FlushFailures.Add($"{entry.Key}: {ex.Message}");
                            env.Log.LogDecision(step.Name, actionName, action.Type, entry.Kind == "existing" ? entry.Id : null, V2Outcomes.Failed, $"Save failed; changes were not written: {ex.Message}");
                        }
                    }
                    LogExecuted($"Saved {saved} of {items.Count} item(s) from {action.FromCollection} ({items.Count - toSave.Count} unchanged).");
                    break;
                }
            case "content.save":
                {
                    var saveTarget = target ?? subject;
                    if (saveTarget == null) break;
                    if (env.IsDryRun)
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Flushed, $"Dry run: {saveTarget.Key} would be saved now.");
                    else
                        await FlushEntryAsync(saveTarget, env, step.Name, action.Index ?? true);
                    break;
                }
            case "exclude":
                {
                    if (subject == null) break;
                    var reason = action.Reason ?? "excluded by configuration";
                    lock (env.Context.Sync) env.Context.Excluded[subject.Key] = reason;
                    lock (env.Summary.Excluded) env.Summary.Excluded.Add(new V2ExclusionSummary { ContentRef = subject.Kind == "draft" ? subject.TempKey ?? "" : subject.Id.ToString(), Reason = reason, Step = step.Name });
                    scope.Excluded = true;
                    lock (stepSummary) stepSummary.Excluded++;
                    // Accumulated deltas are kept - exclusion stops future work, never discards changes.
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Excluded, $"Excluded from later steps: {reason}. Accumulated changes are kept and will be written.");
                    break;
                }
            case "abort":
                {
                    scope.Aborted = true;
                    lock (stepSummary) stepSummary.Aborted++;
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Aborted, "Remaining actions on this step were stopped.");
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
                        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"Score value '{value}' is not a number.");
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
                    LogExecuted($"Scored {subject.Key} at {score} for '{action.Objective}'.");
                    break;
                }
            case "select-top":
                {
                    List<(string Key, int Score)> ranked;
                    lock (env.Context.Sync)
                    {
                        ranked = env.Context.Scores.TryGetValue(action.Objective ?? "", out var scores)
                            ? scores.OrderByDescending(kv => kv.Value).Take(action.Take ?? 10).Select(kv => (kv.Key, kv.Value)).ToList()
                            : new List<(string, int)>();
                    }
                    var selected = new List<V2ContentEntry>();
                    lock (env.Context.Sync)
                    {
                        foreach (var (key, _) in ranked)
                        {
                            var entry = env.Context.EntriesById.Values.FirstOrDefault(e => e.Key == key)
                                ?? env.Context.Drafts.FirstOrDefault(d => d.Key == key);
                            if (entry != null) selected.Add(entry);
                        }
                    }
                    foreach (var entry in selected)
                    {
                        if (action.ContentAction.HasValue)
                        {
                            lock (entry.Deltas)
                            {
                                if (!entry.Deltas.ContentActionIds.Contains(action.ContentAction.Value))
                                    entry.Deltas.ContentActionIds.Add(action.ContentAction.Value);
                            }
                            RecordChange("add-action", entry, ContentActionName(action.ContentAction.Value, env.Lookups), action.ContentAction.Value.ToString());
                        }
                        if (!string.IsNullOrWhiteSpace(action.Into))
                        {
                            lock (env.Context.Sync)
                            {
                                var into = env.Context.GetCollection(action.Into!);
                                if (!into.Any(e => e.Key == entry.Key)) into.Add(entry);
                            }
                        }
                    }
                    LogExecuted($"Selected top {selected.Count} of '{action.Objective}'{(action.Into != null ? $" into {action.Into}" : "")}.");
                    break;
                }
            case "report.run":
                {
                    if (!action.Report.HasValue) break;
                    var usingNote = action.Using != null ? $" using {action.Using}" : "";
                    if (env.IsDryRun)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, null, V2Outcomes.Executed, $"Dry run: report {action.Report} would be published{usingNote}.");
                        lock (env.Summary.Changes) env.Summary.Changes.Add(new V2ChangeSummary { Type = "run-report", ContentRef = action.Report.Value.ToString(), Step = step.Name, Value = action.Using });
                        break;
                    }
                    await _api.PublishReportAsync(action.Report.Value);
                    lock (env.Summary.Changes) env.Summary.Changes.Add(new V2ChangeSummary { Type = "run-report", ContentRef = action.Report.Value.ToString(), Step = step.Name, Value = action.Using });
                    LogExecuted($"Published report {action.Report}{usingNote}.");
                    break;
                }
            case "notification.run":
                {
                    if (!action.Notification.HasValue) break;
                    var usingNote = action.Using != null ? $" using {action.Using}" : "";
                    if (env.IsDryRun)
                    {
                        env.Log.LogDecision(step.Name, actionName, action.Type, null, V2Outcomes.Executed, $"Dry run: notification {action.Notification} would be published{usingNote}.");
                        lock (env.Summary.Changes) env.Summary.Changes.Add(new V2ChangeSummary { Type = "run-notification", ContentRef = action.Notification.Value.ToString(), Step = step.Name, Value = action.Using });
                        break;
                    }
                    await _api.PublishNotificationAsync(action.Notification.Value);
                    lock (env.Summary.Changes) env.Summary.Changes.Add(new V2ChangeSummary { Type = "run-notification", ContentRef = action.Notification.Value.ToString(), Step = step.Name, Value = action.Using });
                    LogExecuted($"Published notification {action.Notification}{usingNote}.");
                    break;
                }
            default:
                env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"Action type '{action.Type}' is not implemented.");
                break;
        }
    }

    private static V2ContentEntry? ResolveItem(string? item, V2ItemScope scope)
    {
        if (string.IsNullOrWhiteSpace(item) || item.Equals("$item", StringComparison.OrdinalIgnoreCase)) return scope.Subject;
        return scope.Drafts.TryGetValue(item, out var draft) ? draft : null;
    }

    /// <summary>Resolve a content action's display name for readable change records.</summary>
    private static string ContentActionName(int id, LookupModel? lookups)
        => lookups?.Actions.FirstOrDefault(a => a.Id == id)?.Name ?? $"action {id}";

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
    private static string? ResolveCandidateField(V2ContentEntry entry, string field) => field.ToLowerInvariant() switch
    {
        "contentid" or "id" => entry.Kind == "draft" ? entry.TempKey : entry.Id.ToString(),
        "story" => string.IsNullOrWhiteSpace(entry.GetField("summary")) ? entry.GetField("body") : entry.GetField("summary"),
        _ => entry.GetField(field),
    };

    /// <summary>
    /// Compare the subject against a collection's candidates in iterate mode (one prompt per
    /// candidate) or batch mode (one prompt per chunk, the response naming the matched id).
    /// </summary>
    private async Task DetectDuplicateAsync(V2StepDefinition step, V2ActionDefinition action, V2ItemScope scope, V2Environment env, V2StepSummary stepSummary)
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
                    lock (env.Summary.Changes) env.Summary.Changes.Add(new V2ChangeSummary
                    {
                        Type = "duplicate",
                        ContentRef = subject.Id.ToString(),
                        Value = matched,
                        Step = step.Name,
                    });
                    StoreDedupeResult(scope, actionName, true, matched);
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Confirmed, $"Known duplicate of {matched} (content link); no comparison sent.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read content links for {id}; falling back to comparison.", subject.Id);
            }
        }

        List<V2ContentEntry> candidates;
        lock (env.Context.Sync)
        {
            candidates = env.Context.Collections.TryGetValue(action.Against ?? "", out var list)
                ? list.Where(e => e.Key != subject.Key).ToList()
                : new List<V2ContentEntry>();
        }
        if (candidates.Count == 0)
        {
            StoreDedupeResult(scope, actionName, false, null);
            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Skipped, $"No candidates in {action.Against}; recorded {actionName}.isDuplicate = false.");
            return;
        }
        if (action.MaxComparisons is > 0 && candidates.Count > action.MaxComparisons.Value)
        {
            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Info, $"Candidates capped at {action.MaxComparisons} of {candidates.Count}.");
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
                env.Log.LogLlm(step.Name, null, actionName, action.Type, contentId, prompt, "", null, null, timer.ElapsedMilliseconds, V2Outcomes.Failed, 1, $"{{\"error\":{JsonSerializer.Serialize(ex.Message)}}}");
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
                    env.Log.LogLlm(step.Name, null, actionName, action.Type, contentId, prompt, result.Content, result.PromptTokens, result.CompletionTokens, timer.ElapsedMilliseconds, V2Outcomes.NotConfirmed, result.Attempts,
                        "{\"note\":\"the response named an id that is not in the batch\"}");
                    continue;
                }
            }

            env.Log.LogLlm(step.Name, null, actionName, action.Type, contentId, prompt, result.Content, result.PromptTokens, result.CompletionTokens, timer.ElapsedMilliseconds,
                confirmed ? V2Outcomes.Confirmed : V2Outcomes.NotConfirmed, result.Attempts,
                confirmed ? $"{{\"duplicateOf\":{JsonSerializer.Serialize(matchedRef)}}}" : null);

            if (!confirmed) continue;

            lock (env.Summary.Changes) env.Summary.Changes.Add(new V2ChangeSummary
            {
                Type = "duplicate",
                ContentRef = subject.Kind == "draft" ? subject.TempKey ?? "" : subject.Id.ToString(),
                Value = matchedRef,
                Step = step.Name,
            });

            // Pure detector: record the answer and stop comparing. Later actions route on the
            // result with condition gates - the action itself decides nothing.
            StoreDedupeResult(scope, actionName, true, matchedRef);
            env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Confirmed, $"Duplicate of {matchedRef}; recorded {actionName}.isDuplicate = true.");

            // Dedupe memory: persist the confirmed pair so later runs skip the comparison.
            // Real runs only - a dry run writes nothing.
            if (remember && !env.IsDryRun && subject.Kind == "existing" && long.TryParse(matchedRef, out var matchedContentId))
            {
                try
                {
                    await _api.AddContentLinkAsync(subject.Id, matchedContentId, "duplicate");
                    env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.Info, $"Recorded content link {subject.Id} -> {matchedContentId} (duplicate).");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record the duplicate content link {id} -> {matched}.", subject.Id, matchedRef);
                }
            }
            return;
        }

        StoreDedupeResult(scope, actionName, false, null);
        env.Log.LogDecision(step.Name, actionName, action.Type, contentId, V2Outcomes.NotConfirmed, $"No duplicate found among {candidates.Count} candidate(s); recorded {actionName}.isDuplicate = false.");
    }

    /// <summary>
    /// Publish a dedupe result into the item scope under the action's name, in the same stores
    /// analyses use - so conditions ('name.isDuplicate'), value sources ('name.matchedId'), and
    /// the lazy-analysis resolver all see it like any other analysis answer.
    /// </summary>
    private static void StoreDedupeResult(V2ItemScope scope, string name, bool isDuplicate, string? matchedRef)
    {
        scope.Raw[name] = isDuplicate ? $"[DUPLICATE:{matchedRef}]" : "";
        scope.Structured[name] = JsonDocument.Parse(JsonSerializer.Serialize(new { isDuplicate, matchedId = matchedRef }, _jsonOptions));
    }
    #endregion

    #region Flushing
    /// <summary>
    /// Report every entry still dirty (or an unsaved draft) at the end of the run: nothing
    /// auto-saves, so anything not covered by a Save Collection / Save Content Now action is
    /// surfaced in the log and summary instead of being silently dropped.
    /// </summary>
    private static void ReportUnwritten(V2Environment env)
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
            env.Log.LogDecision("end-of-run", null, null, entry.Kind == "existing" ? entry.Id : null, V2Outcomes.Skipped, $"{entry.Key}: unsaved ({pending}); {hint}.");
        }
    }

    /// <summary>
    /// Persist one entry: drafts are created (obtaining a database id, recorded for the summary),
    /// existing items are fetched and updated with all deltas applied in one request with indexing.
    /// </summary>
    private async Task FlushEntryAsync(V2ContentEntry entry, V2Environment env, string stepName, bool index)
    {
        if (entry.Kind == "draft")
        {
            var model = BuildDraftModel(entry);
            ApplyDeltas(entry, model, env);
            var publish = entry.Deltas.Status == "publish";
            model.Status = Entities.ContentStatus.Draft;
            var created = await _api.AddContentAsync(model)
                ?? throw new InvalidOperationException("The API returned no content for the created draft.");
            if (publish)
            {
                created.Status = Entities.ContentStatus.Publish;
                created = await _api.UpdateContentAsync(created, index: true) ?? created;
            }
            else if (index)
            {
                created = await _api.UpdateContentAsync(created, index: true) ?? created;
            }
            var tempKey = entry.TempKey ?? "";
            entry.Kind = "existing";
            entry.Id = created.Id;
            lock (entry.Deltas) ClearDeltas(entry.Deltas);
            lock (env.Context.Sync) env.Context.DraftIds[tempKey] = created.Id;
            env.Log.LogDecision(stepName, null, null, created.Id, V2Outcomes.Flushed, $"Draft {tempKey} created as content {created.Id}{(publish ? " and published" : "")}.");
            return;
        }

        bool dirty;
        lock (entry.Deltas) dirty = entry.Deltas.Dirty;
        if (!dirty) return;

        var content = await _api.FindContentByIdAsync(entry.Id)
            ?? throw new InvalidOperationException($"Content {entry.Id} could not be found to apply changes.");
        ApplyDeltas(entry, content, env);
        string summary;
        lock (entry.Deltas)
        {
            summary = $"Applied {entry.Deltas.Fields.Count} field(s), {entry.Deltas.Tags.Count} tag(s){(entry.Deltas.Sentiment.HasValue ? ", sentiment" : "")}{(entry.Deltas.ContributorId.HasValue ? ", contributor" : "")}{(entry.Deltas.ContentActionIds.Count > 0 ? $", {entry.Deltas.ContentActionIds.Count} action(s)" : "")}{(entry.Deltas.Status != null ? $", {entry.Deltas.Status}" : "")} in one update.";
        }
        await _api.UpdateContentAsync(content, index);
        lock (entry.Deltas) ClearDeltas(entry.Deltas);
        env.Log.LogDecision(stepName, null, null, entry.Id, V2Outcomes.Flushed, summary);
    }

    private ContentModel BuildDraftModel(V2ContentEntry entry)
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
        model.PublishedOn = DateTime.TryParse(entry.GetField("publishedOn"), out var publishedOn) ? publishedOn : DateTime.UtcNow;
        return model;
    }

    private void ApplyDeltas(V2ContentEntry entry, ContentModel content, V2Environment env)
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
                var definition = env.Lookups?.Actions.FirstOrDefault(a => a.Id == actionId);
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

    private static void ClearDeltas(V2Deltas deltas)
    {
        deltas.Fields.Clear();
        deltas.Tags.Clear();
        deltas.Sentiment = null;
        deltas.ContributorId = null;
        deltas.ContributorName = null;
        deltas.ContentActionIds.Clear();
        deltas.Status = null;
    }

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
    private static List<V2ComparisonDifference> BuildDifferences(V2VariantSummary a, V2VariantSummary b)
    {
        static Dictionary<string, HashSet<string>> Index(V2VariantSummary summary) => summary.Changes
            .GroupBy(c => c.ContentRef)
            .ToDictionary(g => g.Key, g => g.Select(c => $"{c.Type}{(c.Field != null ? $":{c.Field}" : "")}{(c.Value != null ? $"={c.Value}" : "")}").ToHashSet());

        var indexA = Index(a);
        var indexB = Index(b);
        var differences = new List<V2ComparisonDifference>();
        foreach (var key in indexA.Keys.Union(indexB.Keys).OrderBy(k => k))
        {
            var setA = indexA.TryGetValue(key, out var va) ? va : new HashSet<string>();
            var setB = indexB.TryGetValue(key, out var vb) ? vb : new HashSet<string>();
            var onlyA = setA.Except(setB).ToList();
            var onlyB = setB.Except(setA).ToList();
            if (onlyA.Count > 0 || onlyB.Count > 0)
                differences.Add(new V2ComparisonDifference { ContentRef = key, OnlyA = onlyA, OnlyB = onlyB });
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
    #endregion
    #endregion
}
