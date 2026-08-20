using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AdminAutomationProfileModel = TNO.API.Areas.Admin.Models.Automation.AutomationProfileModel;
using AdminAutomationActionModel = TNO.API.Areas.Admin.Models.Automation.AutomationActionModel;
using AdminAutomationStepModel = TNO.API.Areas.Admin.Models.Automation.AutomationStepModel;
using AdminAutomationRunModel = TNO.API.Areas.Admin.Models.Automation.AutomationRunModel;
using AdminAutomationRunResponseModel = TNO.API.Areas.Admin.Models.Automation.AutomationRunResponseModel;
using AdminAutomationRunStatus = TNO.API.Areas.Admin.Models.Automation.AutomationRunStatus;
using ContentModel = TNO.API.Areas.Services.Models.Content.ContentModel;
using ContentActionModel = TNO.API.Areas.Services.Models.Content.ContentActionModel;
using ContentTagModel = TNO.API.Areas.Services.Models.Content.ContentTagModel;
using ContentTonePoolModel = TNO.API.Areas.Services.Models.Content.ContentTonePoolModel;
using LLMModel = TNO.API.Areas.Services.Models.LLM.LLMModel;
using TNO.AI;
using TNO.AI.Models;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Elastic;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Automation.Config;
using TNO.Services.Managers;

namespace TNO.Services.Automation;

/// <summary>
/// AutomationManager class, coordinates scheduled automation runs and executes queued runs.
/// A run iterates the profile filter content, composes a single prompt per step per content item,
/// parses the LLM response for each action's confirmation statement, tracks confirmation and
/// execution counts (enforcing maxCalls), and applies content updates once per step.
/// </summary>
public class AutomationManager : ServiceManager<AutomationOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly DateTime _instanceStartedOn = DateTime.UtcNow;
    private bool _startupSweepDone;
    private int _retries = 0;
    private string? _lastPruneDate;
    private API.Areas.Editor.Models.Lookup.LookupModel? _lookups;
    private readonly ITNOElasticClient _elasticClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly IAIAgentClient _aiClient;
    private readonly AzureAIOptions _azureAIOptions;
    private readonly IKafkaAdmin _kafkaAdmin;
    private readonly IKafkaListener<string, AutomationRequestModel> _listener;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationManager object.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="elasticClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="aiClient"></param>
    /// <param name="azureAIOptions"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public AutomationManager(
        IApiService api,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IAIAgentClient aiClient,
        IOptions<AzureAIOptions> azureAIOptions,
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, AutomationRequestModel> consumer,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<AutomationOptions> options,
        ILogger<AutomationManager> logger)
        : base(api, chesService, chesOptions, options, logger)
    {
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions.Value;
        _aiClient = aiClient;
        _azureAIOptions = azureAIOptions.Value;
        _kafkaAdmin = kafkaAdmin;
        // LLM prompts can be large (candidate digests, full story bodies); the default
        // HttpClient timeout of 100 seconds is not enough for slow completions.
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(30, this.Options.LLMRequestTimeoutSeconds)),
        };
        _listener = consumer;
        // Automation runs can take longer than the Kafka max poll interval (default 5 minutes).
        // Long-running-job mode pauses the partition while a run executes (polling continues to
        // keep the consumer in the group), and the handler commits + resumes when done.
        _listener.IsLongRunningJob = true;
        _listener.OnError += ListenerErrorHandler;
        _listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run service loop.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                _listener.Stop();
            }

            // Automatically recover from a critical failure after a configured delay so the
            // service does not remain stuck in the 'Failed' state indefinitely.
            if (this.State.Status == ServiceStatus.Failed && this.Options.AutoRestartAfterCriticalFailure)
            {
                this.Logger.LogInformation(
                    "Automation service will attempt to restart after a critical failure in {delay} ms",
                    this.Options.RetryAfterCriticalFailureDelayMS);
                await Task.Delay(this.Options.RetryAfterCriticalFailureDelayMS);
                this.State.Resume();
            }

            if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    await PruneRunHistoryAsync();
                    await ReconcileStalePendingRunsAsync();

                    // Only subscribe to topics that exist.
                    var topics = this.Options.GetTopics();
                    var kafkaTopics = _kafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics)).ToArray();

                    if (topics.Length > 0)
                    {
                        _listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else
                    {
                        _listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Automation service had an unexpected failure.");
                    this.State.RecordFailure();
                    await this.SendErrorEmailAsync("Automation service had an unexpected failure", ex);
                }
            }

            this.Logger.LogDebug("Service sleeping for {delay} ms", delay);
            await Task.Delay(delay);
        }
    }

    #region Run Execution
    /// <summary>
    /// Creates a new cancellation token.
    /// Create a new thread if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            // Make sure the prior task is cancelled before creating a new one.
            if (_cancelToken?.IsCancellationRequested == false)
                _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();
            _consumer = Task.Run(ListenerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ListenerHandlerAsync()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await _listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        _listener.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
                _listener.Stop();
        }
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to stop the consumer task.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerStopHandler(object sender, EventArgs e)
    {
        if (_consumer != null &&
            !_notRunning.Contains(_consumer.Status) &&
            _cancelToken?.IsCancellationRequested == false)
        {
            _cancelToken?.Cancel();
        }
    }

    /// <summary>
    /// Handle an automation request message: execute the queued run it references.
    /// Skips runs that are no longer queued (idempotency guard for at-least-once delivery).
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, AutomationRequestModel> result)
    {
        try
        {
            this.Logger.LogInformation(
                "Automation request received: {topic}:{partition}:{offset} run {runId}",
                result.Topic, result.Partition, result.Offset, result.Message.Value.RunId);
            _retries = 0;

            var run = await this.Api.GetAutomationRunAsync(result.Message.Value.RunId);
            if (run == null)
            {
                this.Logger.LogWarning("Automation run {runId} does not exist; message ignored.", result.Message.Value.RunId);
                return;
            }
            if (run.Status != AdminAutomationRunStatus.Draft || run.CompletedOn != null)
            {
                this.Logger.LogInformation("Automation run {runId} has already been processed; message ignored.", run.Id);
                return;
            }

            await ExecuteQueuedRunAsync(run);
        }
        finally
        {
            // Long-running-job mode: inform Kafka the message is completed and resume
            // consuming. This is done regardless of whether the run failed or succeeded
            // (a failed run is recorded with a Failed status, not retried by redelivery).
            _listener.Commit(result);
            _listener.Resume();
        }
    }

    /// <summary>
    /// Reconcile queued runs whose Kafka message was lost. Only Draft runs older than the
    /// configured stale threshold are executed here (the claim makes this race-free). Runs
    /// stuck in Running (a crashed instance) are marked Failed after the abandoned threshold
    /// rather than re-executed - a slow run is indistinguishable from a crashed one.
    /// </summary>
    /// <returns></returns>
    private async Task ReconcileStalePendingRunsAsync()
    {
        if (this.Options.StalePendingRunMinutes <= 0) return;

        var cutoff = DateTime.UtcNow.AddMinutes(-this.Options.StalePendingRunMinutes);
        var runs = await this.Api.GetAutomationRunsAsync() ?? Array.Empty<AdminAutomationRunModel>();
        var stale = runs
            .Where(run =>
                run.Status == AdminAutomationRunStatus.Draft &&
                run.CompletedOn == null &&
                run.StartedOn <= cutoff)
            .OrderBy(run => run.StartedOn)
            .ToArray();

        foreach (var run in stale)
        {
            this.Logger.LogWarning("Reconciling stale queued automation run {runId} (queued {startedOn:O}).", run.Id, run.StartedOn);
            await ExecuteQueuedRunAsync(run);
        }

        // Runs executing when this instance started cannot still be executing here - a restart
        // killed them. Fail them immediately with a clear note instead of waiting for the
        // inactivity watchdog (activity newer than our start belongs to another instance).
        if (!_startupSweepDone)
        {
            _startupSweepDone = true;
            var dead = runs
                .Where(run => run.Status == AdminAutomationRunStatus.Running &&
                    run.CompletedOn == null &&
                    (run.LastResponseOn ?? run.StartedOn) < _instanceStartedOn)
                .ToArray();
            foreach (var run in dead)
            {
                this.Logger.LogWarning("Automation run {runId} was executing when the service restarted; marking it Failed.", run.Id);
                run.Status = AdminAutomationRunStatus.Failed;
                run.CompletedOn = DateTime.UtcNow;
                run.Note = "The automation service restarted while this run was executing; the run was stopped. Re-run it to start over.";
                await this.Api.UpdateAutomationRunAsync(run);
            }
        }

        if (this.Options.AbandonedRunInactivityMinutes > 0)
        {
            // A healthy run posts a response record as it completes each step, so a Running run that
            // has posted nothing for the inactivity window has stalled (the executing instance likely
            // crashed). Use the last response time as the activity heartbeat, falling back to the run's
            // start time for runs that have not posted any responses yet.
            var inactivityCutoff = DateTime.UtcNow.AddMinutes(-this.Options.AbandonedRunInactivityMinutes);
            var abandoned = runs
                .Where(run =>
                    run.Status == AdminAutomationRunStatus.Running &&
                    run.CompletedOn == null &&
                    (run.LastResponseOn ?? run.StartedOn) <= inactivityCutoff)
                .ToArray();

            foreach (var run in abandoned)
            {
                var lastActivity = run.LastResponseOn ?? run.StartedOn;
                this.Logger.LogWarning("Automation run {runId} has posted no responses since {lastActivity:O}; marking it Failed as abandoned.", run.Id, lastActivity);
                run.Status = AdminAutomationRunStatus.Failed;
                run.CompletedOn = DateTime.UtcNow;
                run.Note = $"Run abandoned: no activity for over {this.Options.AbandonedRunInactivityMinutes} minutes (the executing instance likely crashed).";
                await this.Api.UpdateAutomationRunAsync(run);
            }
        }
    }

    /// <summary>
    /// Execute the specified queued run and report the outcome to the API.
    /// </summary>
    /// <param name="run"></param>
    /// <returns></returns>
    private async Task ExecuteQueuedRunAsync(AdminAutomationRunModel run)
    {
        // Atomically claim the run (Draft -> Running); only one service instance can win,
        // so redeliveries, reconciliation sweeps, and scaled-out instances never execute
        // the same run twice.
        if (!await this.Api.ClaimAutomationRunAsync(run.Id))
        {
            this.Logger.LogInformation("Automation run {runId} was already claimed by another instance; skipped.", run.Id);
            return;
        }

        this.Logger.LogInformation("Executing automation run {runId} for profile {profileId}.", run.Id, run.ProfileId);
        run.Status = AdminAutomationRunStatus.Running;

        try
        {
            var profile = await this.Api.GetAutomationProfileAsync(run.ProfileId)
                ?? throw new InvalidOperationException($"Automation profile {run.ProfileId} does not exist.");
            if (!profile.IsEnabled) throw new InvalidOperationException($"Automation profile '{profile.Name}' is disabled.");

            if (profile.SchemaVersion >= 2 && !string.IsNullOrWhiteSpace(profile.Definition))
            {
                // Schema version 2: the definition-document engine (run context, collections,
                // phases, analyses, always-on decision log, dry runs). v1 profiles keep executing
                // on the code path below until they are migrated.
                // Drafts persist with validation errors; runs must not execute one.
                var parsed = TNO.API.Areas.Admin.Models.Automation.V2.AutomationDefinition.Parse(profile.Definition!);
                var invalid = TNO.API.Areas.Admin.Models.Automation.V2.AutomationDefinitionValidator.Validate(parsed)
                    .Where(e => e.Severity == "error")
                    .ToArray();
                if (invalid.Length > 0)
                    throw new InvalidOperationException(
                        $"The definition is invalid and cannot run ({invalid.Length} error(s)): " +
                        string.Join("; ", invalid.Take(5).Select(e => $"{e.Path}: {e.Message}")));
                var engine = new V2.V2Engine(this.Api, _elasticClient, _elasticOptions, this.Options, this.Logger);
                var v2Summary = await engine.ExecuteAsync(profile, run);
                run.Status = AdminAutomationRunStatus.Completed;
                run.CompletedOn = DateTime.UtcNow;
                run.Summary = JsonSerializer.Serialize(v2Summary, _jsonOptions);
                run.Note = BuildV2RunNote(run.Note, v2Summary);
            }
            else if (run.IsDryRun)
            {
                throw new InvalidOperationException("Dry runs require a schema version 2 profile.");
            }
            else
            {
                var summary = await ExecuteRunAsync(profile, run.Id);
                run.Status = AdminAutomationRunStatus.Completed;
                run.CompletedOn = DateTime.UtcNow;
                run.Summary = JsonSerializer.Serialize(summary, _jsonOptions);
                run.Note = BuildRunNote(run.Note, summary);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Automation run {runId} failed.", run.Id);
            run.Status = AdminAutomationRunStatus.Failed;
            run.CompletedOn = DateTime.UtcNow;
            run.Note = $"Run failed: {ex.Message}";
        }

        // Persist status/note/completion first, then the (potentially very large) summary through
        // its own raw-body endpoint. Keeping the summary out of the run PUT avoids re-escaping it as
        // a JSON string property, which allocates one huge buffer and throws OutOfMemoryException.
        await this.Api.UpdateAutomationRunAsync(run);
        if (!string.IsNullOrEmpty(run.Summary))
            await this.Api.UpdateAutomationRunSummaryAsync(run.Id, run.Summary);
    }

    /// <summary>
    /// Execute the specified automation profile and return a summary of the outcome.
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    private async Task<RunSummary> ExecuteRunAsync(AdminAutomationProfileModel profile, long runId)
    {
        var llm = profile.LLMId.HasValue ? await this.Api.GetLLMAsync(profile.LLMId.Value) : null;
        if (llm == null) throw new InvalidOperationException($"Automation profile '{profile.Name}' requires an LLM.");
        if (llm.ProjectEndpoint == null)
            throw new InvalidOperationException($"LLM '{llm.Name}' is missing a project endpoint.");

        var summary = new RunSummary();
        // Prompt/response records are captured per step and flushed to their own table (not kept in
        // the summary), so the large prompt/response text is never held in memory for the whole run.
        var stepResponses = new List<ResponseSummary>();
        var hasProfileFilter = !IsEmptyQuery(profile.FilterQuery);

        // Load the content items to iterate when the profile has a filter.
        // Run-scoped gate-filter cache: each gate filter query executes at most once per run and
        // only its content ids are kept - gates never need the content models, and caching full
        // models here previously duplicated tens of MB of bodies for nothing.
        var filterCache = new Dictionary<string, List<long>>();
        _lookups = null; // Refresh lookup caches (actions, tags, contributors) once per run.

        // Load lookup caches up front so parallel item processing never races the lazy fetch.
        await GetLookupsAsync();

        var contentItems = new List<ContentModel>();
        if (hasProfileFilter)
        {
            contentItems = await SearchFilterAsync(profile.FilterQuery!, profile.FilterSettings, FilterCacheKey(profile.FilterId, profile.FilterQuery!));
            this.Logger.LogInformation("Profile '{name}' filter returned {count} content item(s) to iterate.", profile.Name, contentItems.Count);
        }

        var parallelism = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, this.Options.MaxParallelContentItems) };

        var steps = profile.Steps.Where(step => step.IsEnabled).OrderBy(step => step.Priority).ToArray();
        var executionCounts = new Dictionary<string, int>();

        // Run-scoped scoring state: objective -> (content id -> score). Populated by 'score-content'
        // actions and consumed by 'select-top' actions via the {candidates:objective} token.
        var scores = new Dictionary<string, Dictionary<long, int>>(StringComparer.OrdinalIgnoreCase);
        var contentById = contentItems.ToDictionary(item => item.Id);

        // Run-scoped record of which content items each action successfully processed
        // (keyed by automation action id). Consumed by 'deduplicate' actions to compare
        // the current item against a prior action's processed items.
        var executedContentByAction = new Dictionary<int, List<long>>();

        // Run-scoped collections produced by 'fetch-content' actions (keyed by automation action
        // id). The Task is stored rather than the list so the filter executes exactly once per run
        // even when the owning step processes items in parallel; the list is read-only once built
        // and shared by every consumer, so a collection is only ever held in memory once.
        var collectionsByAction = new Dictionary<int, Task<IReadOnlyList<ContentModel>>>();

        // Steps may override the profile LLM; fetch each override once per run.
        var llmCache = new Dictionary<int, LLMModel>();

        foreach (var step in steps)
        {
            // Resolve the step's LLM: its own override when configured, otherwise the profile's.
            var stepLlm = await ResolveLlmAsync(step.LLMId, llm, llmCache);
            if (step.LLMId.HasValue && step.LLMId.Value != profile.LLMId)
                this.Logger.LogInformation("Step '{step}' uses LLM override '{llm}'.", step.Name, stepLlm.Name);

            var stepSummary = new StepSummary
            {
                Id = step.Id,
                Name = step.Name,
                Target = step.Target,
                Actions = step.Actions.Where(action => action.IsEnabled)
                    .Select(action => new ActionSummary
                    {
                        Name = action.Name,
                        ActionType = action.ActionType,
                        MaxCalls = action.MaxCalls,
                    }).ToList(),
            };
            summary.Steps.Add(stepSummary);

            var stepTimer = System.Diagnostics.Stopwatch.StartNew();
            this.Logger.LogInformation("Step '{step}' ({target}) started.", step.Name, step.Target);

            // Step filter behaviors. Each behavior fetches only what it consumes: a gate needs
            // content ids (cached run-wide, id-only Elasticsearch _source), iteration needs the
            // full models, and prompt enrichment needs the full models only when a prompt
            // actually references {results} - otherwise the query is skipped entirely.
            HashSet<long>? gateIds = null;
            string? resultsJson = null;
            List<ContentModel>? stepHits = null;
            var iterateStepFilter = step.IterateStepFilter && (step.Target == "start" || step.Target == "end");
            if (!IsEmptyQuery(step.FilterQuery))
            {
                if (iterateStepFilter)
                {
                    // The step filter results are the iteration source; each hit becomes the
                    // step's content item (no gate, no enrichment injection).
                    stepHits = await SearchFilterAsync(step.FilterQuery!, step.FilterSettings, FilterCacheKey(step.FilterId, step.FilterQuery!));
                    this.Logger.LogInformation("Step '{step}' filter returned {count} content item(s) (iteration source).", step.Name, stepHits.Count);
                    foreach (var hit in stepHits)
                        contentById.TryAdd(hit.Id, hit);
                }
                else if (step.ApplyToAutomationFilter)
                {
                    var ids = await SearchFilterIdsAsync(step.FilterQuery!, step.FilterSettings, FilterCacheKey(step.FilterId, step.FilterQuery!), filterCache);
                    this.Logger.LogInformation("Step '{step}' filter returned {count} content item(s) (gate).", step.Name, ids.Count);
                    gateIds = ids.ToHashSet();
                }
                else if (StepUsesResultsToken(step))
                {
                    // Serialize the hit list only when a prompt actually references {results} -
                    // this is one giant string (every hit with its full body) and for a large
                    // filter it is among the biggest allocations a run makes.
                    stepHits = await SearchFilterAsync(step.FilterQuery!, step.FilterSettings, FilterCacheKey(step.FilterId, step.FilterQuery!));
                    this.Logger.LogInformation("Step '{step}' filter returned {count} content item(s) (prompt enrichment).", step.Name, stepHits.Count);
                    resultsJson = JsonSerializer.Serialize(stepHits, _jsonOptions);
                }
                else
                    this.Logger.LogDebug("Step '{step}' filter not executed; results are unused (no gate, no iteration, no {{results}} token).", step.Name);
            }

            switch (step.Target)
            {
                case "content" when hasProfileFilter:
                    {
                        // Steps execute in sequence, but items within a step are independent -
                        // process them in parallel (bounded by MaxParallelContentItems).
                        var eligible = new List<ContentModel>();
                        foreach (var content in contentItems)
                        {
                            if (gateIds != null && !gateIds.Contains(content.Id)) stepSummary.Skipped++;
                            else eligible.Add(content);
                        }
                        await Parallel.ForEachAsync(eligible, parallelism, async (content, _) =>
                        {
                            try
                            {
                                await ExecuteStepInstanceAsync(step, stepLlm, content, resultsJson, executionCounts, scores, contentById, stepSummary, summary.Changes, stepResponses, executedContentByAction, collectionsByAction, llmCache);
                            }
                            catch (Exception ex)
                            {
                                // One failed item (e.g. an exhausted LLM request) must not fail the
                                // whole run; record it and continue with the next item.
                                lock (stepSummary) stepSummary.Failures++;
                                this.Logger.LogError(ex, "Step '{step}' failed for content {contentId}; continuing with the next item.", step.Name, content.Id);
                            }
                            // Keep the buffered prompt/response records bounded while the step runs.
                            await MaybeFlushRunResponsesAsync(runId, stepResponses);
                        });
                        break;
                    }
                case "start" when hasProfileFilter:
                case "end" when hasProfileFilter:
                case "none" when !hasProfileFilter:
                    if (iterateStepFilter)
                    {
                        // Iterate the step filter's results in parallel; the step executes once per hit.
                        await Parallel.ForEachAsync(stepHits ?? new List<ContentModel>(), parallelism, async (content, _) =>
                        {
                            try
                            {
                                await ExecuteStepInstanceAsync(step, stepLlm, content, null, executionCounts, scores, contentById, stepSummary, summary.Changes, stepResponses, executedContentByAction, collectionsByAction, llmCache);
                            }
                            catch (Exception ex)
                            {
                                lock (stepSummary) stepSummary.Failures++;
                                this.Logger.LogError(ex, "Step '{step}' failed for content {contentId}; continuing with the next item.", step.Name, content.Id);
                            }
                            // Keep the buffered prompt/response records bounded while the step runs.
                            await MaybeFlushRunResponsesAsync(runId, stepResponses);
                        });
                        break;
                    }
                    try
                    {
                        await ExecuteStepInstanceAsync(step, stepLlm, null, resultsJson, executionCounts, scores, contentById, stepSummary, summary.Changes, stepResponses, executedContentByAction, collectionsByAction, llmCache);
                    }
                    catch (Exception ex)
                    {
                        stepSummary.Failures++;
                        this.Logger.LogError(ex, "Step '{step}' failed; continuing with the next step.", step.Name);
                    }
                    break;
                default:
                    stepSummary.Notes = $"Step target '{step.Target}' is not valid for this profile configuration and was skipped.";
                    this.Logger.LogWarning("Step '{step}' target '{target}' skipped (profile filter: {hasFilter}).", step.Name, step.Target, hasProfileFilter);
                    break;
            }

            stepTimer.Stop();
            this.Logger.LogInformation(
                "Step '{step}' completed in {elapsed:0.0}s - executions: {executions}, skipped: {skipped}, aborts: {aborts}, failures: {failures}.",
                step.Name, stepTimer.Elapsed.TotalSeconds, stepSummary.Executions, stepSummary.Skipped, stepSummary.Aborts, stepSummary.Failures);

            // Flush this step's prompt/response records to their own table and release the memory.
            // Steps run sequentially (parallelism is within a step), so nothing is still writing to
            // the buffer here. A failure to persist the debug log must not fail the run itself.
            await FlushRunResponsesAsync(runId, stepResponses);
        }

        return summary;
    }

    /// <summary>
    /// Execute a single step instance (one LLM prompt) and apply confirmed actions.
    /// Content updates accumulated by actions are applied once for the step.
    /// </summary>
    private async Task ExecuteStepInstanceAsync(
        AdminAutomationStepModel step,
        LLMModel llm,
        ContentModel? content,
        string? resultsJson,
        Dictionary<string, int> executionCounts,
        Dictionary<string, Dictionary<long, int>> scores,
        Dictionary<long, ContentModel> contentById,
        StepSummary stepSummary,
        List<ChangeSummary> changes,
        List<ResponseSummary> responses,
        Dictionary<int, List<long>> executedContentByAction,
        Dictionary<int, Task<IReadOnlyList<ContentModel>>> collectionsByAction,
        Dictionary<int, LLMModel> llmCache)
    {
        var actions = step.Actions.Where(action => action.IsEnabled).ToArray();
        if (actions.Length == 0) return;

        var contentJson = content != null ? JsonSerializer.Serialize(content, _jsonOptions) : null;
        // 'deduplicate' actions run their own LLM comparisons, 'fetch-content' actions never call
        // an LLM at all, and 'always run' (AutoExecute) actions require no confirmation; none of
        // them contribute to the composed step prompt.
        var promptActions = actions
            .Where(action => action.ActionType != "deduplicate" && action.ActionType != "fetch-content" && !action.AutoExecute)
            .ToArray();

        // Chat-conversation mode: the step prompt becomes the system prompt and each action is
        // sent as its own user message in a shared conversation, so the model retains the
        // context of earlier action responses.
        List<(string Role, string Content)>? conversation = null;
        if (step.UseChatCompletions)
        {
            var stepSystem = PromptComposer.ComposeSystem(step.Prompt, contentJson, resultsJson);
            var systemPrompt = string.IsNullOrWhiteSpace(llm.SystemPrompt)
                ? stepSystem
                : $"{llm.SystemPrompt}\n\n{stepSystem}";
            conversation = new List<(string Role, string Content)> { ("system", systemPrompt) };
        }

        // When the step sends separate prompts, each action's prompt is sent at its position in
        // the action loop instead of one combined prompt for all actions up front.
        var sharedResponse = "";
        if (!step.UseChatCompletions && !step.SendSeparatePrompts && promptActions.Length > 0)
        {
            var promptRows = new List<(string Prompt, string? ContentField, string? Objective)>();
            foreach (var promptAction in promptActions)
                promptRows.Add((await GetActionPromptAsync(promptAction), promptAction.ContentField, promptAction.Objective));
            var prompt = PromptComposer.Compose(
                step.Prompt,
                promptRows,
                contentJson,
                resultsJson);
            prompt = ReplaceCandidatesTokens(prompt, scores, contentById);

            sharedResponse = await InvokeLLMAsync(llm, prompt);
            var responseSummary = new ResponseSummary
            {
                StepId = step.Id,
                StepName = step.Name,
                ContentId = content?.Id,
                Prompt = this.Options.IncludeLLMPromptsInSummary ? prompt : null,
                Response = sharedResponse,
            };
            lock (responses) responses.Add(responseSummary);
        }

        lock (stepSummary) stepSummary.Executions++;

        var pending = new PendingUpdates();
        // Per-item state for the Extract Data / Create Content actions. The dictionary holds
        // values extracted from the iterated item; createdContents holds new content items keyed
        // by the identifier a 'create-content' action assigns, so later actions in this step can
        // target them via their WorksOn property. Scoped per item (items run in parallel).
        var extractedData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var createdContents = new Dictionary<string, CreatedContent>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < actions.Length; index++)
        {
            var action = actions[index];
            var actionSummary = stepSummary.Actions[index];

            // Isolate each action: an unexpected failure in one action is logged and skipped so it
            // cannot abort the whole step and drop unrelated later actions (e.g. always-run
            // reports/notifications). Intentional aborts (deduplicate / stop-remaining) still break.
            try
            {
                // 'fetch-content' gathers a collection for later actions to consume. It calls no LLM
                // and acts on no content, so it neither confirms nor aborts.
                if (action.ActionType == "fetch-content")
                {
                    await ProcessFetchContentAsync(action, collectionsByAction, actionSummary);
                    continue;
                }

                // 'deduplicate' is not confirmed by the main step response; it runs its own LLM
                // comparison against the candidates its prior action supplies. A detected duplicate
                // aborts the step at this position (accumulated updates before it are still applied).
                if (action.ActionType == "deduplicate")
                {
                    var dedupeLlm = await ResolveLlmAsync(action.LLMId, llm, llmCache);
                    var isDuplicate = await DetectDuplicateAsync(
                        step, action, dedupeLlm, content, contentJson, contentById, executionCounts, index,
                        actionSummary, changes, responses, executedContentByAction, collectionsByAction);
                    if (isDuplicate)
                    {
                        lock (stepSummary) stepSummary.Aborts++;
                        break;
                    }
                    continue;
                }

                // Extract Data parses values from its response into the per-item dictionary; Create
                // Content builds a new content item registered under its identifier. Both run their own
                // prompt, so they require the step to send separate prompts per action.
                if (action.ActionType == "extract-data")
                {
                    await ProcessExtractDataAsync(step, action, llm, llmCache, content, contentJson, extractedData, resultsJson, scores, contentById, actionSummary, responses);
                    continue;
                }
                if (action.ActionType == "create-content")
                {
                    await ProcessCreateContentAsync(step, action, content, extractedData, createdContents, actionSummary, changes);
                    continue;
                }

                // Resolve which content this action operates on: the iterated item ("original"), or a
                // content item created earlier in this step (referenced by its identifier via WorksOn).
                // Created-item targeting requires separate prompts so each action's prompt and applied
                // changes use its own content.
                var target = ResolveActionTarget(action, content, contentJson, pending, createdContents, step, actionSummary);
                if (target == null) continue;
                var targetPending = target.Pending;

                // 'Always run' actions execute unconditionally; everything else requires its
                // confirmation statement in the LLM response.
                string? value = null;
                if (!action.AutoExecute)
                {
                    var matcher = new ConfirmationMatcher(action.ConfirmationStatement, action.ContentField, action.Objective);
                    if (!matcher.IsValid)
                    {
                        actionSummary.Notes = "Invalid or empty confirmation statement.";
                        continue;
                    }

                    var response = sharedResponse;
                    if (conversation != null)
                    {
                        // Chat-conversation mode: this action is the next user message; the model
                        // sees the system prompt and every earlier action exchange. An earlier
                        // abort has already exited the loop, so no further messages are sent.
                        var actionLlm = await ResolveLlmAsync(action.LLMId, llm, llmCache);
                        var userPrompt = ReplaceCandidatesTokens(
                            PromptComposer.ComposeAction(await GetActionPromptAsync(action), action.ContentField, action.Objective, contentJson, resultsJson),
                            scores, contentById);
                        // Note whether this is the first turn (which effectively also sends the system
                        // prompt) before adding the user message.
                        var isFirstTurn = conversation.Count == 1;
                        conversation.Add(("user", userPrompt));

                        response = await InvokeChatAsync(actionLlm, conversation);
                        conversation.Add(("assistant", response));

                        var responseSummary = new ResponseSummary
                        {
                            StepId = step.Id,
                            StepName = step.Name,
                            ActionName = action.Name,
                            ContentId = content?.Id,
                            // Only build the (potentially large) recorded prompt when prompts are kept
                            // in the run summary; otherwise skip the concatenation entirely so a second
                            // copy of a large prompt is never held in memory.
                            Prompt = this.Options.IncludeLLMPromptsInSummary
                                ? (isFirstTurn
                                    ? $"[system]\n{conversation[0].Content}\n\n[user]\n{userPrompt}"
                                    : userPrompt)
                                : null,
                            Response = response,
                        };
                        lock (responses) responses.Add(responseSummary);
                    }
                    else if (step.SendSeparatePrompts)
                    {
                        // One prompt per action: the step prompt plus this action's prompt only.
                        // An earlier abort has already exited the loop, so aborted items never
                        // send prompts for later actions.
                        var actionLlm = await ResolveLlmAsync(action.LLMId, llm, llmCache);
                        var prompt = PromptComposer.Compose(
                            step.Prompt,
                            new[] { (await GetActionPromptAsync(action), action.ContentField, action.Objective) },
                            target.Json,
                            resultsJson);
                        prompt = ReplaceCandidatesTokens(prompt, scores, contentById);

                        response = await InvokeLLMAsync(actionLlm, prompt);
                        var responseSummary = new ResponseSummary
                        {
                            StepId = step.Id,
                            StepName = step.Name,
                            ActionName = action.Name,
                            ContentId = content?.Id,
                            Prompt = this.Options.IncludeLLMPromptsInSummary ? prompt : null,
                            Response = response,
                        };
                        lock (responses) responses.Add(responseSummary);
                    }

                    if (!matcher.TryMatch(response, out value))
                    {
                        // Distinguish "the LLM answered but did not confirm" from "the LLM returned
                        // nothing" — the step instructions request silence when criteria are not met,
                        // so an empty response is a decision; record it for debuggability.
                        if (string.IsNullOrWhiteSpace(response))
                            lock (actionSummary) actionSummary.Notes = "No confirmation; the LLM response was empty (no criteria met).";
                        // No confirmation for this action. When configured to abort on a missing
                        // confirmation (e.g. a 'publish' that did not happen), stop the remaining
                        // actions on this step; updates accumulated by earlier actions are still
                        // applied below (same position-sensitive semantics as 'Stop Remaining Actions').
                        if (action.AbortIfNoConfirmation)
                        {
                            actionSummary.Notes = "No confirmation received; remaining actions on this step were aborted.";
                            lock (stepSummary) stepSummary.Aborts++;
                            break;
                        }
                        continue;
                    }
                }
                lock (actionSummary) actionSummary.Confirmations++;

                var key = $"{step.Id}:{index}";
                if (action.ActionType == "select-top")
                {
                    // Applies to multiple items in one execution; the remaining budget is read
                    // and consumed under the executionCounts lock.
                    int remaining;
                    lock (executionCounts)
                    {
                        executionCounts.TryGetValue(key, out var current);
                        remaining = action.MaxCalls.HasValue ? action.MaxCalls.Value - current : int.MaxValue;
                    }
                    if (remaining <= 0)
                    {
                        actionSummary.Notes = $"Max calls ({action.MaxCalls}) reached; execution skipped.";
                        continue;
                    }
                    var applied = await SelectTopAsync(action.ContentActionId, value, remaining, changes, actionSummary, action.Id, executedContentByAction);
                    if (applied > 0)
                    {
                        lock (executionCounts)
                        {
                            executionCounts.TryGetValue(key, out var current);
                            executionCounts[key] = current + applied;
                        }
                        lock (actionSummary) actionSummary.Executions += applied;
                    }
                    continue;
                }

                // Reserve the execution slot atomically (items run in parallel); release it when
                // the action does not end up executing.
                if (!TryReserveExecution(executionCounts, key, action.MaxCalls))
                {
                    actionSummary.Notes = $"Max calls ({action.MaxCalls}) reached; execution skipped.";
                    continue;
                }

                bool executed;
                if (action.ActionType == "score-content")
                {
                    executed = RecordScore(action.Objective, content, value, scores, actionSummary);
                    if (executed)
                    {
                        lock (actionSummary) actionSummary.Executions++;
                        if (content != null) TrackExecutedContent(executedContentByAction, action.Id, content.Id);
                    }
                    else ReleaseExecution(executionCounts, key);
                    continue;
                }

                executed = await ApplyActionAsync(action, value, targetPending, actionSummary, changes);
                if (executed)
                {
                    lock (actionSummary) actionSummary.Executions++;
                    if (content != null) TrackExecutedContent(executedContentByAction, action.Id, content.Id);
                }
                else ReleaseExecution(executionCounts, key);

                if (targetPending.Abort)
                {
                    // 'Stop Remaining Actions' is position sensitive: updates accumulated by actions
                    // ordered before it are still applied below; actions after it are skipped.
                    lock (stepSummary) stepSummary.Aborts++;
                    break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex,
                    "Action '{action}' ({type}) in step '{step}' failed; skipping it and continuing with the remaining actions.",
                    action.Name, action.ActionType, step.Name);
                lock (actionSummary) actionSummary.Notes = $"Action failed: {ex.Message}";
            }
        }

        if (content != null)
            await ApplyPendingUpdatesAsync(step, content.Id, pending, changes);
        else if (pending.HasContentChanges || pending.Status != null || pending.ContentActionIds.Any())
            this.Logger.LogWarning("Step '{step}' confirmed content actions but has no iterated content item to apply them to.", step.Name);

        // Persist any content items created during this step, with the changes accumulated by the
        // actions that targeted them (fields, tags, sentiment, publish). Each item is isolated so
        // one failure cannot drop the others, and the failure is surfaced on the create action's
        // summary instead of silently discarding the item.
        foreach (var (identifier, created) in createdContents)
        {
            try
            {
                await PersistCreatedContentAsync(step, identifier, created, changes);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to create content '{id}' in step '{step}'.", identifier, step.Name);
                if (created.Summary != null)
                    lock (created.Summary) created.Summary.Notes = $"Failed to create content '{identifier}': {ex.Message}";
                lock (stepSummary) stepSummary.Failures++;
            }
        }
    }

    /// <summary>
    /// Resolve an optional LLM override, falling back to the specified LLM. Overrides are
    /// fetched once per run (thread-safe; items are processed in parallel).
    /// </summary>
    private async Task<LLMModel> ResolveLlmAsync(int? llmId, LLMModel fallback, Dictionary<int, LLMModel> llmCache)
    {
        if (!llmId.HasValue || llmId.Value == 0) return fallback;
        lock (llmCache)
        {
            if (llmCache.TryGetValue(llmId.Value, out var cached)) return cached;
        }
        var llm = await this.Api.GetLLMAsync(llmId.Value)
            ?? throw new InvalidOperationException($"LLM {llmId.Value} does not exist.");
        if (llm.ProjectEndpoint == null)
            throw new InvalidOperationException($"LLM '{llm.Name}' is missing a project endpoint.");
        lock (llmCache) llmCache[llmId.Value] = llm;
        return llm;
    }

    /// <summary>
    /// Return the action's prompt, augmented with the context the LLM needs to answer it. An
    /// 'add-tags' action requires the tag vocabulary — the model cannot know the system's tag
    /// codes, and without them it can never "clearly" match one, so (per the step instructions)
    /// it stays silent and the action never confirms.
    /// </summary>
    private async Task<string> GetActionPromptAsync(AdminAutomationActionModel action)
    {
        if (action.ActionType != "add-tags") return action.Prompt;

        var lookups = await GetLookupsAsync();
        var tags = lookups?.Tags.Where(tag => tag.IsEnabled).OrderBy(tag => tag.Code).ToArray() ?? [];
        if (tags.Length == 0) return action.Prompt;

        var vocabulary = string.Join(", ", tags.Select(tag => $"{tag.Code} ({tag.Name})"));
        return $"{action.Prompt}\n<p>Available tag codes (only use codes from this list): {vocabulary}.</p>";
    }

    /// <summary>
    /// Record that the specified action successfully processed the specified content item.
    /// </summary>
    private static void TrackExecutedContent(Dictionary<int, List<long>> executedContentByAction, int actionId, long contentId)
    {
        // Unsaved actions (id 0) cannot be referenced by a 'deduplicate' action.
        if (actionId == 0) return;
        lock (executedContentByAction)
        {
            if (!executedContentByAction.TryGetValue(actionId, out var contentIds))
                executedContentByAction[actionId] = contentIds = new List<long>();
            if (!contentIds.Contains(contentId)) contentIds.Add(contentId);
        }
    }

    /// <summary>
    /// Atomically reserve one execution slot for the action; returns false when 'maxCalls' has
    /// been reached. Items are processed in parallel, so the check and increment must be one
    /// operation. Release the slot with <see cref="ReleaseExecution"/> when the action does not
    /// end up executing.
    /// </summary>
    private static bool TryReserveExecution(Dictionary<string, int> executionCounts, string key, int? maxCalls)
    {
        lock (executionCounts)
        {
            executionCounts.TryGetValue(key, out var executions);
            if (maxCalls.HasValue && executions >= maxCalls.Value) return false;
            executionCounts[key] = executions + 1;
            return true;
        }
    }

    /// <summary>
    /// Release a reserved execution slot (the action did not execute).
    /// </summary>
    private static void ReleaseExecution(Dictionary<string, int> executionCounts, string key)
    {
        lock (executionCounts)
        {
            executionCounts.TryGetValue(key, out var executions);
            executionCounts[key] = Math.Max(0, executions - 1);
        }
    }

    /// <summary>
    /// Compare the current content item against the candidates supplied by the configured prior
    /// action - either the collection a 'fetch-content' action gathered, or the items the prior
    /// action successfully processed this run. The comparison runs in one of two modes:
    /// 'iterate' sends one LLM comparison per candidate, 'batch' sends a digest of up to
    /// 'batchSize' candidates per comparison. The first response that contains the action's
    /// confirmation statement marks the current item as a duplicate.
    /// </summary>
    /// <returns>Whether the current item is a duplicate (the step should abort).</returns>
    private async Task<bool> DetectDuplicateAsync(
        AdminAutomationStepModel step,
        AdminAutomationActionModel action,
        LLMModel llm,
        ContentModel? content,
        string? contentJson,
        Dictionary<long, ContentModel> contentById,
        Dictionary<string, int> executionCounts,
        int index,
        ActionSummary actionSummary,
        List<ChangeSummary> changes,
        List<ResponseSummary> responses,
        Dictionary<int, List<long>> executedContentByAction,
        Dictionary<int, Task<IReadOnlyList<ContentModel>>> collectionsByAction)
    {
        if (content == null || contentJson == null)
        {
            actionSummary.Notes = "Deduplication requires an iterated content item.";
            return false;
        }
        if (!action.PriorActionId.HasValue)
        {
            actionSummary.Notes = "No prior action selected to compare against.";
            return false;
        }
        var matcher = new ConfirmationMatcher(action.ConfirmationStatement, action.ContentField, action.Objective);
        if (!matcher.IsValid)
        {
            actionSummary.Notes = "Invalid or empty confirmation statement.";
            return false;
        }

        var key = $"{step.Id}:{index}";
        lock (executionCounts)
        {
            executionCounts.TryGetValue(key, out var executions);
            if (action.MaxCalls.HasValue && executions >= action.MaxCalls.Value)
            {
                actionSummary.Notes = $"Max calls ({action.MaxCalls}) reached; execution skipped.";
                return false;
            }
        }

        var settings = ReadDeduplicateSettings(action.Settings);

        // Resolve the candidates to compare against. A 'fetch-content' prior action supplies its
        // gathered collection; any other prior action supplies the items it processed this run.
        var candidates = await ResolveDuplicateCandidatesAsync(
            action, content.Id, contentById, executedContentByAction, collectionsByAction);
        if (candidates.Count == 0) return false;

        if (settings.MaxComparisons > 0 && candidates.Count > settings.MaxComparisons)
        {
            this.Logger.LogInformation(
                "Deduplication '{action}' capped from {count} to {max} candidate(s) for content {contentId}.",
                action.Name, candidates.Count, settings.MaxComparisons, content.Id);
            actionSummary.Notes = $"Candidates capped at {settings.MaxComparisons} of {candidates.Count}.";
            candidates = candidates.Take(settings.MaxComparisons).ToList();
        }

        // 'iterate' compares one candidate per LLM call; 'batch' digests up to 'batchSize'
        // candidates into a single call, which is what makes a large collection affordable.
        var batchSize = settings.IsBatch ? Math.Max(1, settings.BatchSize) : 1;
        for (var offset = 0; offset < candidates.Count; offset += batchSize)
        {
            var batch = candidates.Skip(offset).Take(batchSize).ToList();
            var previousJson = settings.IsBatch
                ? JsonSerializer.Serialize(batch.Select(BuildComparisonDigest), _jsonOptions)
                : JsonSerializer.Serialize(BuildComparisonDigest(batch[0]), _jsonOptions);

            var comparePrompt = action.Prompt
                .Replace("{content}", contentJson)
                .Replace("{previous}", previousJson);
            var response = await InvokeLLMAsync(llm, comparePrompt);
            var responseSummary = new ResponseSummary
            {
                StepId = step.Id,
                StepName = step.Name,
                ActionName = action.Name,
                ContentId = content.Id,
                Prompt = this.Options.IncludeLLMPromptsInSummary ? PromptComposer.HtmlToText(comparePrompt) : null,
                Response = response,
            };
            lock (responses) responses.Add(responseSummary);

            if (!matcher.TryMatch(response, out var captured)) continue;

            // Identify which candidate matched. A single-candidate comparison is unambiguous; a
            // batch relies on the statement's {value} token capturing the duplicate's content id.
            long matchedId;
            if (!settings.IsBatch) matchedId = batch[0].Id;
            else if (long.TryParse((captured ?? "").Trim(), out var parsed) && batch.Any(item => item.Id == parsed))
                matchedId = parsed;
            else
            {
                // The model confirmed a duplicate but did not name a candidate from this batch.
                // Treating it as a match would abort the step against an unknown item, so record
                // the miss and keep comparing.
                actionSummary.Notes = $"Duplicate confirmed without a resolvable content id ('{Truncate(captured ?? "", 100)}'); batch skipped.";
                this.Logger.LogWarning(
                    "Deduplication '{action}' confirmed for content {contentId} but returned no candidate id from the batch.",
                    action.Name, content.Id);
                continue;
            }

            lock (actionSummary)
            {
                actionSummary.Confirmations++;
                actionSummary.Executions++;
            }
            lock (executionCounts)
            {
                executionCounts.TryGetValue(key, out var executions);
                executionCounts[key] = executions + 1;
            }
            actionSummary.Notes = $"Content {content.Id} is a duplicate of content {matchedId}.";
            lock (changes) changes.Add(new ChangeSummary { ContentId = content.Id, Type = "duplicate", Value = matchedId.ToString() });
            this.Logger.LogInformation("Content {contentId} detected as a duplicate of {matchedId}; step '{step}' aborted.", content.Id, matchedId, step.Name);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Resolve the content items a 'deduplicate' action compares the current item against.
    /// A 'fetch-content' prior action supplies the collection it gathered; any other prior action
    /// supplies the items it successfully processed this run (resolved from the run's content
    /// lookup, falling back to the API). The current item is never its own candidate.
    /// </summary>
    private async Task<List<ContentModel>> ResolveDuplicateCandidatesAsync(
        AdminAutomationActionModel action,
        long currentContentId,
        Dictionary<long, ContentModel> contentById,
        Dictionary<int, List<long>> executedContentByAction,
        Dictionary<int, Task<IReadOnlyList<ContentModel>>> collectionsByAction)
    {
        var priorActionId = action.PriorActionId!.Value;

        // A collection is built once and then read-only, so it can be shared without copying.
        Task<IReadOnlyList<ContentModel>>? collection;
        lock (collectionsByAction) collectionsByAction.TryGetValue(priorActionId, out collection);
        if (collection != null)
            return (await collection).Where(item => item.Id != currentContentId).ToList();

        // Items run in parallel, so snapshot the prior action's processed ids under the lock.
        long[] priorIds;
        lock (executedContentByAction)
        {
            if (!executedContentByAction.TryGetValue(priorActionId, out var tracked) || tracked.Count == 0)
                return new List<ContentModel>();
            priorIds = tracked.ToArray();
        }

        var candidates = new List<ContentModel>();
        foreach (var priorId in priorIds)
        {
            if (priorId == currentContentId) continue;
            var prior = contentById.TryGetValue(priorId, out var cached)
                ? cached
                : await this.Api.FindContentByIdAsync(priorId);
            if (prior != null) candidates.Add(prior);
        }
        return candidates;
    }

    /// <summary>
    /// Build the compact projection a duplicate comparison is made on: the headline, the summary
    /// (falling back to the body when there is no summary), and the published date. The date is
    /// rendered without its time component because two stories filed on the same day must compare
    /// as the same date regardless of the hour, and text is truncated so a batch prompt stays
    /// bounded no matter how large the candidate set is.
    /// </summary>
    private static object BuildComparisonDigest(ContentModel content)
    {
        return new
        {
            ContentId = content.Id,
            Headline = Truncate(content.Headline ?? "", 300),
            Byline = Truncate(content.Byline ?? "", 200),
            Source = content.Source?.Name ?? content.OtherSource ?? "",
            PublishedOn = content.PublishedOn?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "",
            Story = Truncate(!string.IsNullOrWhiteSpace(content.Summary) ? content.Summary : content.Body ?? "", 2000),
        };
    }

    /// <summary>
    /// Accumulate the effect of a confirmed action into the pending update set for the step.
    /// </summary>
    /// <returns>Whether the action counts as executed.</returns>
    private async Task<bool> ApplyActionAsync(
        AdminAutomationActionModel action,
        string? value,
        PendingUpdates pending,
        ActionSummary actionSummary,
        List<ChangeSummary> changes)
    {
        var actionType = action.ActionType;
        var contentField = action.ContentField;
        var contentActionId = action.ContentActionId;
        switch (actionType)
        {
            case "publish-content":
                // 'Publish' requests publishing; the content update with index=true sends the
                // indexing message that transitions it to 'Published'.
                pending.Status = Entities.ContentStatus.Publish;
                return true;
            case "unpublish-content":
                pending.Status = Entities.ContentStatus.Unpublish;
                return true;
            case "update-content-field":
                if (string.IsNullOrWhiteSpace(contentField) || string.IsNullOrWhiteSpace(value))
                {
                    actionSummary.Notes = "Missing content field or extracted value.";
                    return false;
                }
                pending.Fields[contentField] = value.Trim();
                return true;
            case "add-action":
                if (!contentActionId.HasValue)
                {
                    actionSummary.Notes = "No content action selected.";
                    return false;
                }
                pending.ContentActionIds.Add(contentActionId.Value);
                return true;
            case "add-tags":
                if (string.IsNullOrWhiteSpace(value))
                {
                    actionSummary.Notes = "No tag codes extracted from the response.";
                    return false;
                }
                // Only tags that exist in the database (matched by code or name) can be added.
                var lookups = await GetLookupsAsync();
                var requested = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var unmatched = new List<string>();
                foreach (var request in requested)
                {
                    var tag = lookups?.Tags.FirstOrDefault(t =>
                        t.Code.Equals(request, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.Equals(request, StringComparison.OrdinalIgnoreCase));
                    if (tag != null) pending.Tags.Add((tag.Id, tag.Code, tag.Name));
                    else unmatched.Add(request);
                }
                if (unmatched.Any())
                    actionSummary.Notes = $"No matching tag found for: {string.Join(", ", unmatched)}.";
                return pending.Tags.Any();
            case "select-columnist":
                if (string.IsNullOrWhiteSpace(value))
                {
                    actionSummary.Notes = "No columnist name extracted from the response.";
                    return false;
                }
                var contributor = await FindContributorAsync(value.Trim());
                if (contributor == null)
                {
                    actionSummary.Notes = $"No contributor matched the name or aliases '{Truncate(value.Trim(), 100)}'.";
                    return false;
                }
                pending.ContributorId = contributor.Id;
                return true;
            case "add-sentiment":
                if (!int.TryParse(value?.Trim(), out var sentiment))
                {
                    actionSummary.Notes = $"Sentiment value '{value}' is not a number.";
                    return false;
                }
                pending.Sentiment = Math.Clamp(sentiment, -5, 5);
                return true;
            case "abort-step":
                pending.Abort = true;
                return true;
            case "run-report":
                if (!action.ReportId.HasValue)
                {
                    actionSummary.Notes = "No report selected to run.";
                    return false;
                }
                // Publish the report via the services endpoint; the reporting service generates an
                // instance and sends it to the report's subscribers.
                try
                {
                    await this.Api.PublishReportAsync(action.ReportId.Value);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to publish report {id}.", action.ReportId.Value);
                    actionSummary.Notes = $"Failed to publish report {action.ReportId.Value}.";
                    return false;
                }
                lock (changes) changes.Add(new ChangeSummary { Type = "run-report", Value = action.ReportId.Value.ToString() });
                return true;
            case "run-notification":
                if (!action.NotificationId.HasValue)
                {
                    actionSummary.Notes = "No notification selected to run.";
                    return false;
                }
                // Publish the notification via the services endpoint; the notification service sends
                // it to the notification's subscribers. The editor '/publish' endpoint requires an
                // interactive user (username claim), which a background service does not have.
                try
                {
                    await this.Api.PublishNotificationAsync(action.NotificationId.Value);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to publish notification {id}.", action.NotificationId.Value);
                    actionSummary.Notes = $"Failed to publish notification {action.NotificationId.Value}.";
                    return false;
                }
                lock (changes) changes.Add(new ChangeSummary { Type = "run-notification", Value = action.NotificationId.Value.ToString() });
                return true;
            default:
                actionSummary.Notes = $"Action type '{actionType}' is not supported by the automation service yet. Extracted value: {value}";
                this.Logger.LogWarning("Action type '{type}' is not supported yet.", actionType);
                return false;
        }
    }

    /// <summary>
    /// Send the prompt to the profile's LLM.
    /// Two modes are supported, matching the LLM configuration:
    /// - Agent mode (AgentName set): Azure AI Foundry agent using the service's AzureAI credentials.
    /// - API key mode (DeploymentName set): direct chat-completions request against the LLM's
    ///   project endpoint authenticated with the LLM's own ApiKey - no service credentials needed.
    /// </summary>
    /// <param name="llm"></param>
    /// <param name="prompt"></param>
    /// <returns>The LLM response text.</returns>
    private async Task<string> InvokeLLMAsync(LLMModel llm, string prompt)
    {
        // Note: an empty response is a legitimate outcome — the step instructions tell the model
        // to output nothing when no action's criteria are met — so it is not retried.
        if (!string.IsNullOrWhiteSpace(llm.AgentName))
        {
            if (string.IsNullOrWhiteSpace(_azureAIOptions.TenantId) ||
                string.IsNullOrWhiteSpace(_azureAIOptions.ClientId) ||
                string.IsNullOrWhiteSpace(_azureAIOptions.ClientSecret))
                throw new InvalidOperationException(
                    $"LLM '{llm.Name}' uses an agent, which requires the AzureAI__TenantId/ClientId/ClientSecret service configuration.");

            return await _aiClient.AnalyzeAsync(new AIAgentRequestModel
            {
                AgentName = llm.AgentName!,
                ProjectEndpoint = llm.ProjectEndpoint!,
                DeploymentName = llm.DeploymentName,
                Prompt = prompt,
                TenantId = _azureAIOptions.TenantId,
                ClientId = _azureAIOptions.ClientId,
                ClientSecret = _azureAIOptions.ClientSecret,
            });
        }

        var systemPrompt = string.IsNullOrWhiteSpace(llm.SystemPrompt)
            ? "You are an automation assistant for a news media monitoring service. Follow the instructions exactly."
            : llm.SystemPrompt;

        return await InvokeChatAsync(llm, new[] { ("system", systemPrompt!), ("user", prompt) });
    }

    /// <summary>
    /// Send a chat conversation (ordered role/content messages) to a deployment-based LLM.
    /// Used for single prompts and for chat-conversation steps where each action is a user
    /// message that builds on the earlier responses.
    /// </summary>
    /// <param name="llm"></param>
    /// <param name="messages"></param>
    /// <returns>The LLM response text.</returns>
    private async Task<string> InvokeChatAsync(LLMModel llm, IReadOnlyList<(string Role, string Content)> messages)
    {
        if (!string.IsNullOrWhiteSpace(llm.AgentName))
            throw new InvalidOperationException($"LLM '{llm.Name}' uses an agent; chat conversations require a deployment-based LLM.");
        if (string.IsNullOrWhiteSpace(llm.DeploymentName))
            throw new InvalidOperationException($"LLM '{llm.Name}' requires an agent name or a deployment name.");
        if (string.IsNullOrWhiteSpace(llm.ApiKey))
            throw new InvalidOperationException($"LLM '{llm.Name}' requires an API key for deployment-based requests.");

        // The endpoint determines the request shape: the Responses API uses 'input' with typed
        // content parts ('input_text' for system/user, 'output_text' for assistant turns), the
        // classic chat-completions API uses 'messages'.
        var isResponsesApi = llm.ProjectEndpoint!.AbsolutePath.Contains("/responses", StringComparison.OrdinalIgnoreCase);
        object requestBody = isResponsesApi
            ? new
            {
                model = llm.DeploymentName,
                input = messages.Select(message => (object)new
                {
                    role = message.Role,
                    content = new object[]
                    {
                        new
                        {
                            type = message.Role == "assistant" ? "output_text" : "input_text",
                            text = message.Content,
                        },
                    },
                }).ToArray(),
            }
            : new
            {
                model = llm.DeploymentName,
                messages = messages.Select(message => (object)new
                {
                    role = message.Role,
                    content = message.Content,
                }).ToArray(),
            };

        var requestJson = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var attempts = Math.Max(1, this.Options.LLMRequestAttempts);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                // A request message can only be sent once; build a fresh one per attempt.
                using var request = new HttpRequestMessage(HttpMethod.Post, llm.ProjectEndpoint);
                request.Headers.Add("api-key", llm.ApiKey);
                request.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                // Retry transient failures (throttling and server errors).
                var status = (int)response.StatusCode;
                if ((status == 429 || status >= 500) && attempt < attempts)
                {
                    this.Logger.LogWarning("LLM '{name}' request failed ({status}); retrying attempt {next} of {attempts}.", llm.Name, status, attempt + 1, attempts);
                    await Task.Delay(TimeSpan.FromSeconds(5 * attempt));
                    continue;
                }
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"LLM '{llm.Name}' request failed ({status}): {Truncate(responseJson, 500)}");

                if (isResponsesApi) return ParseResponsesOutput(responseJson);

                var responseData = JsonSerializer.Deserialize<TNO.Models.Azure.ChatCompletionResponse>(responseJson);
                return responseData?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
            }
            catch (TaskCanceledException) when (attempt < attempts)
            {
                // The request timed out (HttpClient.Timeout); retry.
                this.Logger.LogWarning("LLM '{name}' request timed out after {timeout}s; retrying attempt {next} of {attempts}.", llm.Name, this.Options.LLMRequestTimeoutSeconds, attempt + 1, attempts);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == null && attempt < attempts)
            {
                // A connection-level failure (DNS, socket reset); retry.
                this.Logger.LogWarning(ex, "LLM '{name}' request failed to connect; retrying attempt {next} of {attempts}.", llm.Name, attempt + 1, attempts);
                await Task.Delay(TimeSpan.FromSeconds(5 * attempt));
            }
        }
    }

    /// <summary>
    /// Extract the output text from a Responses API result.
    /// </summary>
    /// <param name="responseJson"></param>
    /// <returns></returns>
    private static string ParseResponsesOutput(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        // Some implementations provide the convenience 'output_text' property.
        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
            return outputText.GetString() ?? "";

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
            return "";

        var text = new System.Text.StringBuilder();
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var type) || type.GetString() != "message") continue;
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("type", out var partType) && partType.GetString() == "output_text" &&
                    part.TryGetProperty("text", out var partText) && partText.ValueKind == JsonValueKind.String)
                {
                    text.AppendLine(partText.GetString());
                }
            }
        }
        return text.ToString().Trim();
    }

    /// <summary>
    /// Record a 'score-content' action result in the run-scoped scoring state.
    /// </summary>
    /// <returns>Whether the score was recorded.</returns>
    private bool RecordScore(
        string? objective,
        ContentModel? content,
        string? value,
        Dictionary<string, Dictionary<long, int>> scores,
        ActionSummary actionSummary)
    {
        if (string.IsNullOrWhiteSpace(objective))
        {
            actionSummary.Notes = "The score action requires an objective.";
            return false;
        }
        if (content == null)
        {
            actionSummary.Notes = "The score action requires an iterated content item (step target 'content').";
            return false;
        }
        if (!int.TryParse(value?.Trim(), out var score))
        {
            actionSummary.Notes = $"Score value '{value}' is not a number.";
            return false;
        }

        // Items are processed in parallel; the run-scoped score state must be synchronized.
        lock (scores)
        {
            if (!scores.TryGetValue(objective, out var objectiveScores))
            {
                objectiveScores = new Dictionary<long, int>();
                scores[objective] = objectiveScores;
            }
            objectiveScores[content.Id] = score;
        }
        return true;
    }

    /// <summary>
    /// Apply the configured content action to the content ids selected by a 'select-top' action.
    /// The extracted value is a comma-separated list of content ids; 'maxCalls' caps the number of
    /// content items across the run.
    /// </summary>
    /// <returns>The number of content items the action was applied to.</returns>
    private async Task<int> SelectTopAsync(
        int? contentActionId,
        string? value,
        int remaining,
        List<ChangeSummary> changes,
        ActionSummary actionSummary,
        int actionId,
        Dictionary<int, List<long>> executedContentByAction)
    {
        if (!contentActionId.HasValue)
        {
            actionSummary.Notes = "The select action requires a content action.";
            return 0;
        }
        if (string.IsNullOrWhiteSpace(value))
        {
            actionSummary.Notes = "No content ids were extracted from the response.";
            return 0;
        }

        var ids = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => long.TryParse(part, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
        {
            actionSummary.Notes = $"No valid content ids in the response value '{Truncate(value, 100)}'.";
            return 0;
        }

        if (ids.Count > remaining)
        {
            actionSummary.Notes = $"Selection of {ids.Count} exceeded the max calls limit; applied the first {remaining}.";
            ids = ids.Take(remaining).ToList();
        }

        var applied = 0;
        foreach (var id in ids)
        {
            var content = await this.Api.FindContentByIdAsync(id);
            if (content == null)
            {
                this.Logger.LogWarning("Selected content {contentId} could not be found; action skipped.", id);
                continue;
            }
            if (await SetContentActionAsync(content, contentActionId.Value))
            {
                // Persist through a content update so the item is reindexed and the
                // action is visible in the editor lists.
                await this.Api.UpdateContentAsync(content, index: true);
                lock (changes) changes.Add(new ChangeSummary { ContentId = id, Type = "select-top", Value = contentActionId.Value.ToString() });
                TrackExecutedContent(executedContentByAction, actionId, id);
                applied++;
            }
        }
        return applied;
    }

    /// <summary>
    /// Replace {candidates} / {candidates:objective} tokens with a digest of the top scored content
    /// items for the objective (content id, score, headline, source, summary).
    /// </summary>
    private string ReplaceCandidatesTokens(
        string prompt,
        Dictionary<string, Dictionary<long, int>> scores,
        Dictionary<long, ContentModel> contentById)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            prompt,
            @"\{candidates(?::([A-Za-z0-9_\-]+))?\}",
            (match) =>
            {
                var objective = match.Groups[1].Value;
                // Snapshot under the lock; score state is mutated by parallel item processing.
                Dictionary<long, int>? objectiveScores;
                lock (scores)
                {
                    Dictionary<long, int>? found;
                    if (!string.IsNullOrEmpty(objective))
                        scores.TryGetValue(objective, out found);
                    else
                        found = scores.Count == 1 ? scores.Values.First() : null;
                    objectiveScores = found != null ? new Dictionary<long, int>(found) : null;
                }

                if (objectiveScores == null || objectiveScores.Count == 0) return "[]";

                // Bound the digest: rank by score, keep the top N, and truncate the text fields so
                // the prompt never grows unbounded with the number/size of scored items (which
                // otherwise exhausts memory building this string). Selection still works because the
                // model chooses from the highest-scored candidates.
                const int maxCandidates = 500;
                if (objectiveScores.Count > maxCandidates)
                    this.Logger.LogInformation(
                        "Candidates digest for objective '{objective}' capped from {count} to {max} highest-scored items.",
                        objective, objectiveScores.Count, maxCandidates);

                var candidates = objectiveScores
                    .OrderByDescending(pair => pair.Value)
                    .Take(maxCandidates)
                    .Select(pair =>
                    {
                        contentById.TryGetValue(pair.Key, out var content);
                        return new
                        {
                            ContentId = pair.Key,
                            Score = pair.Value,
                            Headline = Truncate(content?.Headline ?? "", 300),
                            Source = content?.Source?.Name ?? content?.OtherSource ?? "",
                            Summary = Truncate(content?.Summary ?? "", 500),
                        };
                    });
                return JsonSerializer.Serialize(candidates, _jsonOptions);
            });
    }

    /// <summary>
    /// Apply the accumulated updates for a step to the content item.
    /// Field, tag, and sentiment changes are applied with a single content update request.
    /// </summary>
    private async Task ApplyPendingUpdatesAsync(
        AdminAutomationStepModel step,
        long contentId,
        PendingUpdates pending,
        List<ChangeSummary> changes)
    {
        if (!pending.HasContentChanges && pending.Status == null && !pending.ContentActionIds.Any())
            return;

        var content = await this.Api.FindContentByIdAsync(contentId);
        if (content == null)
        {
            this.Logger.LogWarning("Content {contentId} could not be found to apply step '{step}' updates.", contentId, step.Name);
            return;
        }

        // Content actions (e.g. Top Story) are applied to the model so the single update below
        // persists them AND reindexes the item - the standalone content-action endpoint does not
        // send an indexing message, which leaves Elasticsearch (and the editor lists) stale.
        var actionsChanged = false;
        foreach (var actionId in pending.ContentActionIds.Distinct())
        {
            if (await SetContentActionAsync(content, actionId))
            {
                actionsChanged = true;
                lock (changes) changes.Add(new ChangeSummary { ContentId = contentId, Type = "add-action", Value = actionId.ToString() });
            }
        }

        if (pending.HasContentChanges || pending.Status.HasValue || actionsChanged)
        {
            foreach (var (field, value) in pending.Fields)
            {
                ApplyContentField(content, field, value);
                lock (changes) changes.Add(new ChangeSummary { ContentId = contentId, Type = "update-field", Field = field, Value = Truncate(value, 500) });
            }

            if (pending.Tags.Any())
            {
                var tags = content.Tags.ToList();
                foreach (var (tagId, code, name) in pending.Tags.DistinctBy(tag => tag.Id))
                {
                    if (!tags.Any(tag => tag.Id == tagId))
                        tags.Add(new ContentTagModel(tagId, code, name));
                }
                content.Tags = tags;
                lock (changes) changes.Add(new ChangeSummary { ContentId = contentId, Type = "add-tags", Value = string.Join(",", pending.Tags.Select(tag => tag.Code)) });
            }

            if (pending.ContributorId.HasValue)
            {
                content.ContributorId = pending.ContributorId.Value;
                lock (changes) changes.Add(new ChangeSummary { ContentId = contentId, Type = "select-columnist", Value = pending.ContributorId.Value.ToString() });
            }

            if (pending.Sentiment.HasValue)
            {
                var tonePools = content.TonePools.Where(pool => pool.Id != this.Options.DefaultTonePoolId).ToList();
                tonePools.Add(new ContentTonePoolModel { Id = this.Options.DefaultTonePoolId, ContentId = contentId, Value = pending.Sentiment.Value });
                content.TonePools = tonePools;
                lock (changes) changes.Add(new ChangeSummary { ContentId = contentId, Type = "add-sentiment", Value = pending.Sentiment.Value.ToString() });
            }

            // A single update request per step for all accumulated content changes.
            // Status changes are folded into the same update so the API sends the
            // Publish/Unpublish indexing message (the status-only endpoint does not index).
            if (pending.Status.HasValue)
            {
                content.Status = pending.Status.Value;
                lock (changes) changes.Add(new ChangeSummary { ContentId = contentId, Type = pending.Status == Entities.ContentStatus.Publish ? "publish" : "unpublish" });
            }

            // Continue with the returned model so subsequent requests carry the new
            // concurrency version (otherwise later updates fail with a stale version).
            var updated = await this.Api.UpdateContentAsync(content, index: true);
            if (updated != null) content = updated;
        }

    }

    /// <summary>
    /// Set the specified action on the content model (e.g. Top Story = 'true').
    /// Boolean actions are applied with the value 'true'; other value types use the
    /// action's configured default value.
    /// The model must be saved with a content update for the change to persist and index.
    /// Returns false when the content already has the action applied.
    /// </summary>
    private async Task<bool> SetContentActionAsync(ContentModel content, int actionId)
    {
        var lookups = await GetLookupsAsync();
        var definition = lookups?.Actions.FirstOrDefault(action => action.Id == actionId);
        if (definition == null)
        {
            this.Logger.LogWarning("Action {actionId} does not exist; unable to apply it to content {contentId}.", actionId, content.Id);
            return false;
        }

        var value = definition.ValueType == Entities.ValueType.Boolean
            ? "true"
            : (!string.IsNullOrWhiteSpace(definition.DefaultValue) ? definition.DefaultValue : "true");

        var actions = content.Actions.ToList();
        var existing = actions.FirstOrDefault(action => action.Id == actionId);
        if (existing != null)
        {
            if (existing.Value == value)
            {
                this.Logger.LogDebug("Content {contentId} already has action {actionId} applied; skipped.", content.Id, actionId);
                return false;
            }
            existing.Value = value;
        }
        else
        {
            actions.Add(new ContentActionModel { Id = actionId, ContentId = content.Id, Value = value });
            content.Actions = actions;
        }
        return true;
    }

    /// <summary>
    /// Find a contributor matching the specified name, checking both the contributor
    /// name and their configured aliases.
    /// </summary>
    private async Task<API.Areas.Editor.Models.Contributor.ContributorModel?> FindContributorAsync(string name)
    {
        var lookups = await GetLookupsAsync();
        if (lookups == null) return null;

        return lookups.Contributors.FirstOrDefault(contributor =>
            contributor.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            SplitAliases(contributor.Aliases).Any(alias => alias.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Split a contributor aliases value into individual alias names.
    /// Supports comma-separated values and JSON-style array formatting.
    /// </summary>
    private static IEnumerable<string> SplitAliases(string? aliases)
    {
        if (string.IsNullOrWhiteSpace(aliases)) return Array.Empty<string>();
        return aliases
            .Replace("[", "").Replace("]", "").Replace("\"", "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Fetch the shared lookups (actions, tags, contributors) once per run.
    /// </summary>
    private async Task<API.Areas.Editor.Models.Lookup.LookupModel?> GetLookupsAsync()
    {
        _lookups ??= await this.Api.GetLookupsAsync();
        return _lookups;
    }

    /// <summary>
    /// Apply the extracted value to the specified content field.
    /// </summary>
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

    /// <summary>
    /// Page size used when fetching all filter matches.
    /// </summary>
    private const int FilterPageSize = 500;

    /// <summary>
    /// Elasticsearch's default max_result_window; from + size must not exceed it.
    /// </summary>
    private const int MaxResultWindow = 10000;

    /// <summary>
    /// Execute a filter query and return ALL matching items. The filter's stored "size" is a UI
    /// page size, not a cap - the engine pages through the full match set (bounded by
    /// Elasticsearch's max_result_window) so automation iterates every item the filter matches.
    /// Full models are intentionally NOT cached - each behavior that needs them (profile
    /// iteration, step iteration, {results} enrichment) fetches once and owns the memory; gates
    /// use <see cref="SearchFilterIdsAsync"/> instead.
    /// </summary>
    private async Task<List<ContentModel>> SearchFilterAsync(
        string query,
        string? settings,
        string label)
    {
        var index = SelectIndex(settings);
        var root = JsonNode.Parse(query)!.AsObject();
        root["size"] = FilterPageSize;
        root["track_total_hits"] = true;

        var hits = new List<ContentModel>();
        var from = 0;
        while (true)
        {
            root["from"] = from;
            var result = await _elasticClient.SearchAsync<ContentModel>(index, JsonDocument.Parse(root.ToJsonString()));
            var page = result.Hits.Hits.Select(hit => hit.Source).Where(item => item != null).ToList();
            hits.AddRange(page!);
            if (page.Count < FilterPageSize) break;
            from += FilterPageSize;
            if (from + FilterPageSize > MaxResultWindow)
            {
                this.Logger.LogWarning("Filter '{key}' exceeded the max result window ({max}); results truncated.", label, MaxResultWindow);
                break;
            }
        }

        return hits;
    }

    /// <summary>
    /// Execute a filter query and return only the matching content ids, cached for the run.
    /// Used by gate filters, which never need the content models - the Elasticsearch _source is
    /// limited to the id so neither the transfer nor the cache carries document bodies.
    /// </summary>
    private async Task<List<long>> SearchFilterIdsAsync(
        string query,
        string? settings,
        string cacheKey,
        Dictionary<string, List<long>> cache)
    {
        if (cache.TryGetValue(cacheKey, out var cached)) return cached;

        var index = SelectIndex(settings);
        var root = JsonNode.Parse(query)!.AsObject();
        root["size"] = FilterPageSize;
        root["track_total_hits"] = true;
        root["_source"] = new JsonArray("id");

        var ids = new List<long>();
        var from = 0;
        while (true)
        {
            root["from"] = from;
            var result = await _elasticClient.SearchAsync<ContentModel>(index, JsonDocument.Parse(root.ToJsonString()));
            var page = result.Hits.Hits.Select(hit => hit.Source).Where(item => item != null).ToList();
            ids.AddRange(page.Select(item => item!.Id));
            if (page.Count < FilterPageSize) break;
            from += FilterPageSize;
            if (from + FilterPageSize > MaxResultWindow)
            {
                this.Logger.LogWarning("Filter '{key}' exceeded the max result window ({max}); results truncated.", cacheKey, MaxResultWindow);
                break;
            }
        }

        cache[cacheKey] = ids;
        this.Logger.LogDebug("Filter '{key}' returned {count} id(s); ids cached for this run.", cacheKey, ids.Count);
        return ids;
    }

    /// <summary>
    /// Build the cache key for a filter; prefer the filter id, fall back to the query text.
    /// </summary>
    private static string FilterCacheKey(int? filterId, string query)
    {
        return filterId.HasValue ? $"filter:{filterId.Value}" : $"query:{query}";
    }

    /// <summary>
    /// Select the Elasticsearch index based on the filter settings (searchUnpublished).
    /// </summary>
    private string SelectIndex(string? filterSettings)
    {
        var searchUnpublished = false;
        if (!string.IsNullOrWhiteSpace(filterSettings))
        {
            try
            {
                // Disposed: nothing here outlives the scope, so the pooled buffer is returned.
                using var settings = JsonDocument.Parse(filterSettings);
                if (settings.RootElement.TryGetProperty("searchUnpublished", out var property))
                    searchUnpublished = property.ValueKind == JsonValueKind.True;
            }
            catch (JsonException)
            {
                // Ignore malformed settings and default to the published index.
            }
        }
        return searchUnpublished ? _elasticOptions.ContentIndex : _elasticOptions.PublishedIndex;
    }

    /// <summary>
    /// Check whether the specified filter query is missing or empty.
    /// </summary>
    private static bool IsEmptyQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query) || query == "{}") return true;
        try
        {
            // Disposed: only a bool leaves this scope, so the pooled buffer is returned.
            using var document = JsonDocument.Parse(query);
            return !document.RootElement.TryGetProperty("query", out _);
        }
        catch (JsonException)
        {
            return true;
        }
    }

    /// <summary>
    /// Persist the buffered prompt/response records for a completed step to the run's response table,
    /// then clear the buffer to release the memory. A failure to persist the records (a debug aid) is
    /// logged but never fails the run.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="responses"></param>
    /// <returns></returns>
    /// <summary>
    /// Whether the step's prompt or any enabled action prompt references the {results} token
    /// (including indexed/path forms like {results[0].headline}).
    /// </summary>
    private static bool StepUsesResultsToken(AdminAutomationStepModel step)
    {
        if (step.Prompt?.Contains("{results") == true) return true;
        return step.Actions.Any(action => action.IsEnabled && action.Prompt?.Contains("{results") == true);
    }

    /// <summary>
    /// Cap for each stored prompt/response (applied at capture in ResponseSummary). This is the
    /// diagnostic/audit copy only - the value an action actually applied to content was already
    /// parsed and applied in full.
    /// </summary>
    private const int MaxStoredResponseChars = 20000;

    /// <summary>
    /// Number of buffered response records that triggers an incremental flush while a step is
    /// still executing, so a step with thousands of items cannot accumulate them all in memory.
    /// </summary>
    private const int ResponseFlushThreshold = 20;

    /// <summary>
    /// Flush the buffer when it has reached the threshold. Safe to call concurrently from the
    /// parallel item loops: the buffer is snapshotted and cleared under its lock, and the
    /// snapshot is persisted outside the lock.
    /// </summary>
    private async Task MaybeFlushRunResponsesAsync(long runId, List<ResponseSummary> responses)
    {
        List<ResponseSummary>? snapshot = null;
        lock (responses)
        {
            if (responses.Count >= ResponseFlushThreshold)
            {
                snapshot = new List<ResponseSummary>(responses);
                responses.Clear();
            }
        }
        if (snapshot != null) await FlushRunResponsesAsync(runId, snapshot);
    }

    private async Task FlushRunResponsesAsync(long runId, List<ResponseSummary> responses)
    {
        if (responses.Count == 0) return;
        try
        {
            // Map and send in bounded batches instead of materializing every model at once - a
            // large step can buffer thousands of records and the all-at-once projection is what
            // previously exhausted memory. Values are already truncated at capture.
            foreach (var chunk in responses.Chunk(ResponseFlushThreshold))
            {
                var models = chunk.Select(r => new AdminAutomationRunResponseModel
                {
                    StepId = r.StepId,
                    StepName = r.StepName,
                    ActionName = r.ActionName,
                    ContentId = r.ContentId,
                    Prompt = r.Prompt,
                    Response = r.Response,
                }).ToArray();
                await this.Api.AddAutomationRunResponsesAsync(runId, models);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to persist {count} response record(s) for run {runId}; continuing.", responses.Count, runId);
        }
        finally
        {
            responses.Clear();
        }
    }

    private static string BuildV2RunNote(string? note, V2.V2RunSummary summary)
    {
        var variant = summary.VariantA;
        var result = summary.IsComparison
            ? $"Comparison run: {summary.Differences.Count} item(s) differ between the variants."
            : $"Executed {variant?.Steps.Count ?? 0} step(s), {variant?.LlmCalls ?? 0} LLM call(s), {variant?.Changes.Count ?? 0} change(s), {variant?.Excluded.Count ?? 0} exclusion(s).";
        if (summary.IsDryRun) result = $"DRY RUN - nothing was written. {result}";
        return string.IsNullOrWhiteSpace(note) ? result : $"{note} | {result}";
    }

    private static string BuildRunNote(string? note, RunSummary summary)
    {
        var executions = summary.Steps.Sum(step => step.Executions);
        var actionExecutions = summary.Steps.Sum(step => step.Actions.Sum(action => action.Executions));
        var result = $"Executed {summary.Steps.Count} step(s), {executions} prompt(s), {actionExecutions} action(s), {summary.Changes.Count} change(s).";
        return string.IsNullOrWhiteSpace(note) ? result : $"{note} | {result}";
    }

    private static string Truncate(string value, int length)
    {
        return value.Length <= length ? value : value[..length];
    }

    /// <summary>
    /// Truncate a stored prompt/response to a maximum length, appending a marker. Preserves null.
    /// </summary>
    private static string? TruncateForStorage(string? value, int length)
    {
        if (value == null) return null;
        return value.Length <= length ? value : value[..length] + "…[truncated]";
    }
    #endregion

    private DateTime GetProfileNow(string? timeZone)
    {
        var timeZoneName = string.IsNullOrWhiteSpace(timeZone) ? this.Options.DefaultTimeZone : timeZone;
        try
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName!);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, info);
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Prune automation run history older than the configured retention period.
    /// Runs at most once per calendar day (in the default time zone).
    /// </summary>
    /// <returns></returns>
    private async Task PruneRunHistoryAsync()
    {
        if (this.Options.RunRetentionDays <= 0) return;

        var today = GetProfileNow(this.Options.DefaultTimeZone).ToString("yyyyMMdd");
        if (_lastPruneDate == today) return;

        try
        {
            var deleted = await this.Api.PruneAutomationRunsAsync(this.Options.RunRetentionDays);
            _lastPruneDate = today;
            if (deleted > 0)
                this.Logger.LogInformation("Pruned {count} automation run(s) older than {days} day(s).", deleted, this.Options.RunRetentionDays);

            // The v2 decision log keeps the current date only (independent of run retention):
            // cut off at the start of today in the service's default time zone.
            if (this.Options.RunLogRetentionDays > 0)
            {
                var localNow = GetProfileNow(this.Options.DefaultTimeZone);
                var localCutoff = localNow.Date.AddDays(-(this.Options.RunLogRetentionDays - 1));
                var cutoffUtc = DateTime.SpecifyKind(localCutoff + (DateTime.UtcNow - localNow), DateTimeKind.Utc);
                var logsDeleted = await this.Api.PruneAutomationRunLogsAsync(cutoffUtc);
                if (logsDeleted > 0)
                    this.Logger.LogInformation("Pruned {count} automation run log entrie(s) older than {cutoff:u}.", logsDeleted, cutoffUtc);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogWarning(ex, "Failed to prune automation run history.");
        }
    }

    #region Summary Models
    private sealed class RunSummary
    {
        public List<StepSummary> Steps { get; } = new();
        public List<ChangeSummary> Changes { get; } = new();
    }

    private sealed class ResponseSummary
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = "";
        public string? ActionName { get; set; }
        public long? ContentId { get; set; }

        /// <summary>
        /// The prompt sent to the LLM; only recorded when IncludeLLMPromptsInSummary is enabled
        /// (prompts embed the full content payload, which makes summaries large).
        /// Truncated at capture so buffered records stay bounded — a step can produce thousands
        /// of records and the untruncated originals must not live until the flush.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prompt { get => _prompt; set => _prompt = TruncateForStorage(value, MaxStoredResponseChars); }
        private string? _prompt;

        public string Response { get => _response; set => _response = TruncateForStorage(value, MaxStoredResponseChars) ?? ""; }
        private string _response = "";
    }

    private sealed class StepSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Target { get; set; } = "";
        public int Executions { get; set; }
        public int Skipped { get; set; }
        public int Aborts { get; set; }
        public int Failures { get; set; }
        public string? Notes { get; set; }
        public List<ActionSummary> Actions { get; set; } = new();
    }

    private sealed class ActionSummary
    {
        public string Name { get; set; } = "";
        public string ActionType { get; set; } = "";
        public int Confirmations { get; set; }
        public int Executions { get; set; }
        public int? MaxCalls { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class ChangeSummary
    {
        public long ContentId { get; set; }
        public string Type { get; set; } = "";
        public string? Field { get; set; }
        public string? Value { get; set; }
    }

    #region Extract Data & Create Content

    /// <summary>
    /// The content an action applies its changes to, plus the accumulated pending updates.
    /// </summary>
    private sealed class ActionTarget
    {
        public ContentModel? Content { get; init; }
        public string? Json { get; init; }
        public PendingUpdates Pending { get; init; } = new();
    }

    /// <summary>
    /// A content item created during a step by a 'create-content' action, held in memory until the
    /// end of the step when it is persisted with the changes its downstream actions accumulate.
    /// </summary>
    private sealed class CreatedContent
    {
        public ContentModel Model { get; init; } = new();
        public PendingUpdates Pending { get; } = new();
        /// <summary>The iterated item the create action ran against; used to backfill required fields.</summary>
        public ContentModel? Original { get; init; }
        /// <summary>The create action's summary so persistence failures are visible in the run summary.</summary>
        public ActionSummary? Summary { get; init; }
    }

    /// <summary>
    /// Resolve which content an action operates on. Returns the iterated item for "original" (or no)
    /// WorksOn; otherwise the content created earlier in the step under that identifier. Returns null
    /// (skip the action) when a created target cannot be honoured.
    /// </summary>
    private ActionTarget? ResolveActionTarget(
        AdminAutomationActionModel action,
        ContentModel? content,
        string? contentJson,
        PendingUpdates originalPending,
        Dictionary<string, CreatedContent> createdContents,
        AdminAutomationStepModel step,
        ActionSummary actionSummary)
    {
        var handle = action.WorksOn;
        if (string.IsNullOrWhiteSpace(handle) || handle.Equals("original", StringComparison.OrdinalIgnoreCase))
            return new ActionTarget
            {
                Content = content,
                Json = content != null && originalPending.HasContentChanges
                    ? SerializeTargetView(content, originalPending)
                    : contentJson,
                Pending = originalPending,
            };

        if (!step.SendSeparatePrompts)
        {
            actionSummary.Notes = $"Action targets created content '{handle}' but the step does not send separate prompts; skipped.";
            return null;
        }
        if (!createdContents.TryGetValue(handle, out var created))
        {
            actionSummary.Notes = $"No content was created with identifier '{handle}' earlier in this step; skipped.";
            return null;
        }
        return new ActionTarget
        {
            Content = created.Model,
            Json = SerializeTargetView(created.Model, created.Pending),
            Pending = created.Pending,
        };
    }

    /// <summary>
    /// Serialize the target with its pending changes (fields, tags, sentiment, contributor) folded
    /// in, so later actions' prompts see the step's evolving state — e.g. tags confirmed by an
    /// earlier action appear in the next action's content JSON. The model itself is not modified;
    /// pending changes are still applied once when the step's updates are persisted.
    /// </summary>
    private string SerializeTargetView(ContentModel model, PendingUpdates pending)
    {
        if (!pending.HasContentChanges) return JsonSerializer.Serialize(model, _jsonOptions);

        var view = JsonSerializer.Deserialize<ContentModel>(JsonSerializer.Serialize(model, _jsonOptions), _jsonOptions) ?? model;
        foreach (var (field, value) in pending.Fields) ApplyContentField(view, field, value);
        if (pending.Tags.Any())
        {
            var tags = view.Tags.ToList();
            foreach (var (tagId, code, name) in pending.Tags.DistinctBy(tag => tag.Id))
                if (!tags.Any(tag => tag.Id == tagId)) tags.Add(new ContentTagModel(tagId, code, name));
            view.Tags = tags;
        }
        if (pending.ContributorId.HasValue) view.ContributorId = pending.ContributorId.Value;
        if (pending.Sentiment.HasValue)
        {
            var tonePools = view.TonePools.Where(pool => pool.Id != this.Options.DefaultTonePoolId).ToList();
            tonePools.Add(new ContentTonePoolModel { Id = this.Options.DefaultTonePoolId, Value = pending.Sentiment.Value });
            view.TonePools = tonePools;
        }
        return JsonSerializer.Serialize(view, _jsonOptions);
    }


    /// <summary>
    /// Execute an 'extract-data' action: seed the dictionary with the iterated item's field values
    /// (default shape), then override/add keys parsed from the LLM response markers.
    /// </summary>
    private async Task ProcessExtractDataAsync(
        AdminAutomationStepModel step,
        AdminAutomationActionModel action,
        LLMModel llm,
        Dictionary<int, LLMModel> llmCache,
        ContentModel? content,
        string? contentJson,
        Dictionary<string, string> data,
        string? resultsJson,
        Dictionary<string, Dictionary<long, int>> scores,
        Dictionary<long, ContentModel> contentById,
        ActionSummary actionSummary,
        List<ResponseSummary> responses)
    {
        if (!step.SendSeparatePrompts)
        {
            actionSummary.Notes = "Extract Data requires the step to send separate prompts per action.";
            return;
        }
        if (content == null)
        {
            actionSummary.Notes = "Extract Data requires an iterated content item.";
            return;
        }

        var rows = ReadExtractRows(action.Settings);
        if (rows.Count == 0)
        {
            actionSummary.Notes = "Extract Data has no keys configured.";
            return;
        }

        // Token-only rows are resolved directly (a content-property copy) and quoted values are
        // literal constants; instruction rows are sent to the LLM in a single prompt that returns
        // a marker block per key.
        var copied = 0;
        var generateRows = new List<(string Key, string Instruction)>();
        foreach (var (key, value) in rows)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            var trimmed = (value ?? "").Trim();
            if (trimmed.Length == 0) continue;
            if (IsContentTokenOnly(trimmed))
            {
                // The aggregate fields (tags/topics/sentiment) are not scalar JSON properties, so
                // resolve them to the code/name-list / value form Create Content can apply; every
                // other token resolves against the content JSON.
                data[key] = ResolveSpecialCopyToken(trimmed, content)
                    ?? PromptComposer.ResolveContentTokens(trimmed, contentJson).Trim();
                copied++;
            }
            else if (trimmed.Length > 1 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
            {
                // A double-quoted value is a literal constant — no LLM round-trip required.
                data[key] = trimmed[1..^1];
                copied++;
            }
            else
            {
                generateRows.Add((key, PromptComposer.ResolveContentTokens(trimmed, contentJson)));
            }
        }

        var generated = 0;
        if (generateRows.Count > 0)
        {
            var builder = new System.Text.StringBuilder();
            // The step prompt participates like the composed prompts do: {content}/{content.*} and
            // {results} tokens are replaced. When the author places the bare {content} token the
            // content is inserted there and the automatic 'Content:' block below is skipped
            // (mirrors how Compose handles the {actions} token).
            var stepText = PromptComposer.HtmlToText(step.Prompt);
            var stepPlacesContent = stepText.Contains("{content}");
            var preamble = PromptComposer.ResolveResultsTokens(
                PromptComposer.ResolveContentTokens(stepText, contentJson), resultsJson);
            if (!string.IsNullOrWhiteSpace(preamble)) builder.AppendLine(preamble).AppendLine();
            builder.AppendLine("Produce a value for each key below. Follow each key's instruction and wrap its value in the markers exactly as shown (omit a block when there is no value):");
            foreach (var (key, instruction) in generateRows)
            {
                builder.AppendLine();
                builder.AppendLine($"Key '{key}': {instruction}");
                builder.AppendLine($"[UPDATE FIELD START:{key}]");
                builder.AppendLine("{value}");
                builder.AppendLine($"[UPDATE FIELD END:{key}]");
            }
            // The instructions typically reference the content's fields (e.g. "parse the
            // content.body") — include the content itself or the model has nothing to work from.
            // Skipped when the step prompt already placed it via the {content} token.
            if (!stepPlacesContent && !string.IsNullOrWhiteSpace(contentJson))
            {
                builder.AppendLine();
                builder.AppendLine("Content:");
                builder.AppendLine(contentJson);
            }
            var actionLlm = await ResolveLlmAsync(action.LLMId, llm, llmCache);
            var prompt = builder.ToString();
            var response = await InvokeLLMAsync(actionLlm, prompt);
            var responseSummary = new ResponseSummary
            {
                StepId = step.Id,
                StepName = step.Name,
                ActionName = action.Name,
                ContentId = content.Id,
                Prompt = this.Options.IncludeLLMPromptsInSummary ? prompt : null,
                Response = response,
            };
            lock (responses) responses.Add(responseSummary);
            foreach (var (k, v) in ParseDataFields(response)) { data[k] = v; generated++; }
        }

        lock (actionSummary)
        {
            actionSummary.Confirmations++;
            actionSummary.Executions++;
            actionSummary.Notes = $"Extracted {copied} copied and {generated} generated value(s).";
        }
    }

    /// <summary>
    /// Execute a 'create-content' action: build a new content item (optionally cloned from the
    /// iterated item) and apply the extracted dictionary to its properties using the configured
    /// property-to-key mapping. Registered under the action's identifier for later actions.
    /// </summary>
    private async Task ProcessCreateContentAsync(
        AdminAutomationStepModel step,
        AdminAutomationActionModel action,
        ContentModel? original,
        Dictionary<string, string> data,
        Dictionary<string, CreatedContent> createdContents,
        ActionSummary actionSummary,
        List<ChangeSummary> changes)
    {
        if (!step.SendSeparatePrompts)
        {
            actionSummary.Notes = "Create Content requires the step to send separate prompts per action.";
            return;
        }
        if (string.IsNullOrWhiteSpace(action.CreateIdentifier))
        {
            actionSummary.Notes = "Create Content requires an identifier so later actions can reference it.";
            return;
        }

        var model = action.CreateClone && original != null ? CloneContentModel(original) : new ContentModel();

        // Apply the content-property <- extracted-key mapping (defaults to identity for the
        // standard fields when the action has no explicit mapping configured). Handles scalar
        // fields, enums (status/contentType), foreign keys (source/mediaType/series) and
        // collections (tags/topics/sentiment).
        var mapping = ReadMapping(action.Settings);
        if (mapping.Count == 0) mapping = DefaultContentPropertyMap();
        var applied = 0;
        foreach (var (property, key) in mapping)
        {
            if (string.IsNullOrWhiteSpace(key) || !data.TryGetValue(key, out var val)) continue;
            if (await ApplyContentValueAsync(model, property, val ?? "")) applied++;
        }

        // A cloned item must not share the original's business key, or the system treats it as the
        // same story. Derive a fresh uid from the original uid + action name + date.
        var stamp = GetProfileNow(this.Options.DefaultTimeZone).ToString("yyyyMMdd");
        model.Id = 0;
        model.Uid = original != null && !string.IsNullOrWhiteSpace(original.Uid)
            ? $"{original.Uid}-{Slug(action.Name)}-{stamp}"
            : $"{Slug(action.Name)}-{stamp}-{step.Id}";
        model.ExternalUid = "";

        createdContents[action.CreateIdentifier] = new CreatedContent { Model = model, Original = original, Summary = actionSummary };
        lock (actionSummary)
        {
            actionSummary.Confirmations++;
            actionSummary.Executions++;
            actionSummary.Notes = $"Prepared new content '{action.CreateIdentifier}' ({applied} field(s) applied).";
        }
        // The 'create-content' change entry is recorded when the item is actually persisted (with
        // its real id); recording it here would report a phantom item when persistence fails.
    }

    /// <summary>
    /// Persist a created content item and apply the changes accumulated by actions that targeted it.
    /// Created as Draft to obtain an id, then published (with indexing) when a 'publish' targeted it.
    /// </summary>
    private async Task PersistCreatedContentAsync(
        AdminAutomationStepModel step,
        string identifier,
        CreatedContent created,
        List<ChangeSummary> changes)
    {
        var model = created.Model;
        var pending = created.Pending;

        // Fold the target's pending field/tag/sentiment/contributor changes into the new model.
        foreach (var (field, value) in pending.Fields) ApplyContentField(model, field, value);
        if (pending.Tags.Any())
        {
            var tags = model.Tags.ToList();
            foreach (var (tagId, code, name) in pending.Tags.DistinctBy(tag => tag.Id))
                if (!tags.Any(tag => tag.Id == tagId)) tags.Add(new ContentTagModel(tagId, code, name));
            model.Tags = tags;
        }
        if (pending.ContributorId.HasValue) model.ContributorId = pending.ContributorId.Value;
        if (pending.Sentiment.HasValue)
        {
            var tonePools = model.TonePools.Where(pool => pool.Id != this.Options.DefaultTonePoolId).ToList();
            tonePools.Add(new ContentTonePoolModel { Id = this.Options.DefaultTonePoolId, Value = pending.Sentiment.Value });
            model.TonePools = tonePools;
        }

        var publish = pending.Status == Entities.ContentStatus.Publish
            || model.Status == Entities.ContentStatus.Publish
            || model.Status == Entities.ContentStatus.Published;

        // Backfill required fields from the original iterated item when the extract/mapping did not
        // supply them — the Content entity rejects a blank source, and license/media-type are
        // required foreign keys.
        var original = created.Original;
        if (string.IsNullOrWhiteSpace(model.OtherSource) && original != null) model.OtherSource = original.OtherSource;
        if (model.LicenseId == 0 && original != null) model.LicenseId = original.LicenseId;
        if (model.MediaTypeId == 0 && original != null) model.MediaTypeId = original.MediaTypeId;
        // Published content without a published-on date is invisible to every date-filtered query
        // (filters, reports, the subscriber app) even though it is indexed.
        model.PublishedOn ??= original?.PublishedOn ?? DateTime.UtcNow;
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(model.OtherSource)) missing.Add("source");
        if (model.LicenseId == 0) missing.Add("license");
        if (model.MediaTypeId == 0) missing.Add("mediaType");
        if (missing.Any())
        {
            this.Logger.LogWarning("Cannot create content '{id}' in step '{step}'; missing required field(s): {fields}.", identifier, step.Name, string.Join(", ", missing));
            if (created.Summary != null)
                lock (created.Summary) created.Summary.Notes = $"Failed to create content '{identifier}'; missing required field(s): {string.Join(", ", missing)}.";
            return;
        }

        // Create as Draft to obtain an id; content actions and publishing are applied via a
        // follow-up update that also sends the indexing message.
        model.Status = Entities.ContentStatus.Draft;
        var createdModel = await this.Api.AddContentAsync(model);
        if (createdModel == null)
        {
            this.Logger.LogWarning("Failed to create content '{id}' in step '{step}'.", identifier, step.Name);
            if (created.Summary != null)
                lock (created.Summary) created.Summary.Notes = $"Failed to create content '{identifier}'; the API returned no result.";
            return;
        }
        lock (changes) changes.Add(new ChangeSummary { ContentId = createdModel.Id, Type = "create-content", Value = identifier });

        var actionsChanged = false;
        foreach (var actionId in pending.ContentActionIds.Distinct())
            if (await SetContentActionAsync(createdModel, actionId)) actionsChanged = true;

        if (publish || actionsChanged)
        {
            if (publish)
            {
                createdModel.Status = Entities.ContentStatus.Publish;
                lock (changes) changes.Add(new ChangeSummary { ContentId = createdModel.Id, Type = "publish" });
            }
            await this.Api.UpdateContentAsync(createdModel, index: true);
        }
    }

    /// <summary>
    /// Parse '[UPDATE FIELD START:key] value [UPDATE FIELD END:key]' markers from an LLM response
    /// into a key/value dictionary. Reuses the marker convention of 'update-content-field'.
    /// </summary>
    private static Dictionary<string, string> ParseDataFields(string? response)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(response)) return result;
        var matches = System.Text.RegularExpressions.Regex.Matches(
            response,
            @"\[UPDATE FIELD START:(?<key>[^\]]+)\](?<val>.*?)\[UPDATE FIELD END:\k<key>\]",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var key = match.Groups["key"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(key)) result[key] = match.Groups["val"].Value.Trim();
        }
        return result;
    }

    /// <summary>
    /// Whether an Extract Data row value is only content tokens (e.g. {content.headline}), meaning
    /// it should be copied directly rather than sent to the LLM as an instruction.
    /// </summary>
    private static bool IsContentTokenOnly(string value)
        => System.Text.RegularExpressions.Regex.IsMatch(
            value.Trim(),
            @"^(\s*\{content(?:\.[A-Za-z0-9_.]+)?\}\s*)+$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>
    /// Resolve the aggregate copy tokens that are not scalar JSON properties into the form Create
    /// Content applies: {content.tags} -> comma-separated tag codes, {content.topics} -> comma
    /// -separated topic names, {content.sentiment} -> the default tone-pool value. Returns null for
    /// any other token so the caller falls back to the generic JSON resolver.
    /// </summary>
    private string? ResolveSpecialCopyToken(string value, ContentModel content)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "{content.tags}":
                return string.Join(",", content.Tags.Select(tag => tag.Code).Where(code => !string.IsNullOrWhiteSpace(code)));
            case "{content.topics}":
                return string.Join(",", content.Topics.Select(topic => topic.Name).Where(name => !string.IsNullOrWhiteSpace(name)));
            case "{content.sentiment}":
                var pool = content.TonePools.FirstOrDefault(p => p.Id == this.Options.DefaultTonePoolId)
                    ?? content.TonePools.FirstOrDefault();
                return pool != null ? pool.Value.ToString() : "";
            default:
                return null;
        }
    }

    /// <summary>
    /// Execute a 'fetch-content' action: run its filter once per run and hold the results for
    /// later actions that reference it. The action acts on no content and calls no LLM.
    /// </summary>
    private async Task ProcessFetchContentAsync(
        AdminAutomationActionModel action,
        Dictionary<int, Task<IReadOnlyList<ContentModel>>> collectionsByAction,
        ActionSummary actionSummary)
    {
        // Unsaved actions (id 0) have no stable identity, so nothing could reference the result.
        if (action.Id == 0)
        {
            actionSummary.Notes = "Save the profile before this action can supply a collection.";
            return;
        }
        if (IsEmptyQuery(action.FilterQuery))
        {
            actionSummary.Notes = "No filter selected; no content fetched.";
            return;
        }

        var settings = ReadCollectionSettings(action.Settings);

        // Get-or-add the task under the lock and await it outside, so the filter runs exactly once
        // per run however many items (or steps) reach this action concurrently.
        Task<IReadOnlyList<ContentModel>> collection;
        var fetched = false;
        lock (collectionsByAction)
        {
            if (!collectionsByAction.TryGetValue(action.Id, out var existing))
            {
                existing = SearchFilterProjectedAsync(
                    action.FilterQuery!, action.FilterSettings, $"action:{action.Id}", settings);
                collectionsByAction[action.Id] = existing;
                fetched = true;
            }
            collection = existing;
        }

        var items = await collection;
        // Only the call that started the fetch counts as an execution - later items on a parallel
        // step reuse the same collection and must not inflate the count.
        if (!fetched) return;
        lock (actionSummary)
        {
            actionSummary.Executions++;
            actionSummary.Notes = items.Count >= settings.MaxItems
                ? $"Fetched {items.Count} content item(s); capped at {settings.MaxItems}."
                : $"Fetched {items.Count} content item(s).";
        }
    }

    /// <summary>
    /// The content fields a fetched collection carries by default. Driven by what a duplicate
    /// comparison reads - the headline, the summary or body, and the published date - plus the
    /// identity and attribution fields the prompts and run summary need. The body is included but
    /// truncated on ingest: excluding it would break comparisons against stories with no summary,
    /// while keeping it whole is what makes a large collection expensive to hold.
    /// </summary>
    private static readonly string[] DefaultCollectionFields = new[]
    {
        "id", "headline", "byline", "summary", "body", "publishedOn", "source", "otherSource",
    };

    /// <summary>
    /// Default number of items a collection holds. A collection lives for the whole run, so it is
    /// capped rather than paged to the Elasticsearch max result window like the profile filter.
    /// </summary>
    private const int DefaultCollectionMaxItems = 500;

    /// <summary>
    /// Execute a filter and return a bounded, projected collection to be held for the run.
    /// Unlike <see cref="SearchFilterAsync"/> this limits the Elasticsearch _source to the fields
    /// the comparison needs and truncates the long text fields on arrival, so holding the result
    /// for the whole run costs megabytes rather than hundreds of megabytes.
    /// </summary>
    private async Task<IReadOnlyList<ContentModel>> SearchFilterProjectedAsync(
        string query,
        string? settings,
        string label,
        CollectionSettings collectionSettings)
    {
        var index = SelectIndex(settings);
        var root = JsonNode.Parse(query)!.AsObject();
        root["track_total_hits"] = true;
        root["_source"] = new JsonArray(collectionSettings.Fields.Select(field => (JsonNode)field!).ToArray());

        var hits = new List<ContentModel>();
        var from = 0;
        while (hits.Count < collectionSettings.MaxItems)
        {
            var pageSize = Math.Min(FilterPageSize, collectionSettings.MaxItems - hits.Count);
            // Elasticsearch rejects a request whose from + size exceeds the result window, so the
            // last page is clamped to it rather than allowed to overrun.
            if (from + pageSize > MaxResultWindow)
            {
                pageSize = MaxResultWindow - from;
                if (pageSize <= 0)
                {
                    this.Logger.LogWarning("Collection '{key}' reached the max result window ({max}); results truncated.", label, MaxResultWindow);
                    break;
                }
            }
            root["size"] = pageSize;
            root["from"] = from;
            var result = await _elasticClient.SearchAsync<ContentModel>(index, JsonDocument.Parse(root.ToJsonString()));
            var page = result.Hits.Hits.Select(hit => hit.Source).Where(item => item != null).ToList();
            foreach (var item in page)
            {
                item!.Headline = Truncate(item.Headline ?? "", collectionSettings.TruncateHeadline);
                item.Summary = Truncate(item.Summary ?? "", collectionSettings.TruncateSummary);
                item.Body = Truncate(item.Body ?? "", collectionSettings.TruncateBody);
                hits.Add(item);
            }
            if (page.Count < pageSize) break;
            from += pageSize;
        }

        this.Logger.LogInformation("Collection '{key}' fetched {count} content item(s).", label, hits.Count);
        return hits;
    }

    /// <summary>
    /// Configuration for a 'fetch-content' action's collection.
    /// </summary>
    private sealed class CollectionSettings
    {
        public string[] Fields { get; init; } = DefaultCollectionFields;
        public int MaxItems { get; init; } = DefaultCollectionMaxItems;
        public int TruncateHeadline { get; init; } = 300;
        public int TruncateSummary { get; init; } = 500;
        public int TruncateBody { get; init; } = 2000;
    }

    /// <summary>
    /// Configuration for a 'deduplicate' action's comparison.
    /// </summary>
    private sealed class DeduplicateSettings
    {
        public bool IsBatch { get; init; }
        public int BatchSize { get; init; } = 25;
        public int MaxComparisons { get; init; }
    }

    /// <summary>
    /// Read the collection configuration (settings.collection) from a 'fetch-content' action.
    /// </summary>
    private static CollectionSettings ReadCollectionSettings(System.Text.Json.JsonDocument? settings)
    {
        if (settings == null
            || settings.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object
            || !settings.RootElement.TryGetProperty("collection", out var obj)
            || obj.ValueKind != System.Text.Json.JsonValueKind.Object)
            return new CollectionSettings();

        var fields = DefaultCollectionFields;
        if (obj.TryGetProperty("fields", out var arr) && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            var configured = arr.EnumerateArray()
                .Where(item => item.ValueKind == System.Text.Json.JsonValueKind.String)
                .Select(item => item.GetString() ?? "")
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
            // 'id' identifies the match, so it is always retrieved even if it was not configured.
            if (configured.Length > 0)
                fields = configured.Contains("id") ? configured : configured.Prepend("id").ToArray();
        }

        return new CollectionSettings
        {
            Fields = fields,
            MaxItems = ReadPositiveInt(obj, "maxItems", DefaultCollectionMaxItems),
            TruncateHeadline = ReadTruncation(obj, "headline", 300),
            TruncateSummary = ReadTruncation(obj, "summary", 500),
            TruncateBody = ReadTruncation(obj, "body", 2000),
        };
    }

    /// <summary>
    /// Read the comparison configuration (settings.deduplicate) from a 'deduplicate' action.
    /// Absent settings keep the original behaviour: one LLM comparison per candidate.
    /// </summary>
    private static DeduplicateSettings ReadDeduplicateSettings(System.Text.Json.JsonDocument? settings)
    {
        if (settings == null
            || settings.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object
            || !settings.RootElement.TryGetProperty("deduplicate", out var obj)
            || obj.ValueKind != System.Text.Json.JsonValueKind.Object)
            return new DeduplicateSettings();

        var mode = obj.TryGetProperty("mode", out var modeValue) && modeValue.ValueKind == System.Text.Json.JsonValueKind.String
            ? modeValue.GetString() ?? ""
            : "";

        return new DeduplicateSettings
        {
            IsBatch = mode.Equals("batch", StringComparison.OrdinalIgnoreCase),
            BatchSize = ReadPositiveInt(obj, "batchSize", 25),
            MaxComparisons = Math.Max(0, ReadPositiveInt(obj, "maxComparisons", 0)),
        };
    }

    /// <summary>
    /// Read a positive integer from the settings object, falling back to the default.
    /// </summary>
    private static int ReadPositiveInt(System.Text.Json.JsonElement obj, string property, int fallback)
    {
        if (obj.TryGetProperty(property, out var value)
            && value.ValueKind == System.Text.Json.JsonValueKind.Number
            && value.TryGetInt32(out var parsed)
            && parsed > 0)
            return parsed;
        return fallback;
    }

    /// <summary>
    /// Read a per-field truncation length from the settings object's 'truncate' map.
    /// </summary>
    private static int ReadTruncation(System.Text.Json.JsonElement obj, string field, int fallback)
    {
        if (obj.TryGetProperty("truncate", out var truncate) && truncate.ValueKind == System.Text.Json.JsonValueKind.Object)
            return ReadPositiveInt(truncate, field, fallback);
        return fallback;
    }

    /// <summary>
    /// Read the Extract Data rows (settings.extract = [{ key, value }]) from the action settings.
    /// </summary>
    private static List<(string Key, string Value)> ReadExtractRows(System.Text.Json.JsonDocument? settings)
    {
        var rows = new List<(string, string)>();
        if (settings != null
            && settings.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object
            && settings.RootElement.TryGetProperty("extract", out var arr)
            && arr.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                if (item.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                var key = item.TryGetProperty("key", out var k) && k.ValueKind == System.Text.Json.JsonValueKind.String ? k.GetString() ?? "" : "";
                var value = item.TryGetProperty("value", out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() ?? "" : "";
                if (!string.IsNullOrWhiteSpace(key)) rows.Add((key, value));
            }
        }
        return rows;
    }

    /// <summary>
    /// Read the Create Content property-to-key mapping (settings.mapping = { property: key }).
    /// </summary>
    private static Dictionary<string, string> ReadMapping(System.Text.Json.JsonDocument? settings)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (settings != null
            && settings.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object
            && settings.RootElement.TryGetProperty("mapping", out var obj)
            && obj.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var prop in obj.EnumerateObject())
                map[prop.Name] = prop.Value.ValueKind == System.Text.Json.JsonValueKind.String ? (prop.Value.GetString() ?? "") : "";
        }
        return map;
    }

    /// <summary>
    /// The default Create Content mapping: each supported content property maps to a same-named key.
    /// </summary>
    private static Dictionary<string, string> DefaultContentPropertyMap()
        => MappableContentProperties.ToDictionary(p => p, p => p, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The content properties Extract Data / Create Content can map. Scalar fields, enums, foreign
    /// keys and collections are all supported by <see cref="ApplyContentValueAsync"/>.
    /// </summary>
    private static readonly string[] MappableContentProperties = new[]
    {
        "headline", "byline", "summary", "body", "edition", "section", "page",
        "status", "contentType", "source", "mediaType", "series", "tags", "topics", "sentiment",
    };

    /// <summary>
    /// Apply a single extracted value to a content property: scalar text, enums (status,
    /// contentType), foreign keys (source/mediaType/series by id or code/name), and collections
    /// (tags/topics by code/name, sentiment as a tone value). Returns whether anything was applied.
    /// </summary>
    private async Task<bool> ApplyContentValueAsync(ContentModel content, string property, string value)
    {
        var val = (value ?? "").Trim();
        switch (property.Trim().ToLowerInvariant())
        {
            case "headline":
            case "byline":
            case "summary":
            case "body":
            case "edition":
            case "section":
            case "page":
                ApplyContentField(content, property, val);
                return true;
            case "othersource":
                content.OtherSource = val;
                return true;
            case "otherseries":
                content.OtherSeries = val;
                return true;
            case "status":
                if (Enum.TryParse<Entities.ContentStatus>(val, true, out var status)) { content.Status = status; return true; }
                return false;
            case "contenttype":
                if (Enum.TryParse<Entities.ContentType>(val, true, out var contentType)) { content.ContentType = contentType; return true; }
                return false;
            case "ownerid":
                if (int.TryParse(val, out var ownerId)) { content.OwnerId = ownerId; return true; }
                return false;
            case "source":
                if (string.IsNullOrWhiteSpace(val)) return false;
                {
                    // Resolve numeric ids and code/name matches through the lookups so the source
                    // code can be carried on OtherSource — the Content entity requires a non-empty
                    // source string on create (its convention is OtherSource = source.Code).
                    var lookups = await GetLookupsAsync();
                    var source = int.TryParse(val, out var sourceId)
                        ? lookups?.Sources.FirstOrDefault(s => s.Id == sourceId)
                        : lookups?.Sources.FirstOrDefault(s =>
                            s.Code.Equals(val, StringComparison.OrdinalIgnoreCase) || s.Name.Equals(val, StringComparison.OrdinalIgnoreCase));
                    if (source != null)
                    {
                        content.SourceId = source.Id;
                        content.OtherSource = source.Code;
                        if (content.LicenseId == 0) content.LicenseId = source.LicenseId;
                    }
                    else if (int.TryParse(val, out var unknownSourceId)) content.SourceId = unknownSourceId;
                    else { content.SourceId = null; content.OtherSource = val; }
                    return true;
                }
            case "mediatype":
                if (string.IsNullOrWhiteSpace(val)) return false;
                if (int.TryParse(val, out var mediaTypeId)) { content.MediaTypeId = mediaTypeId; return true; }
                else
                {
                    var lookups = await GetLookupsAsync();
                    var mediaType = lookups?.MediaTypes.FirstOrDefault(m => m.Name.Equals(val, StringComparison.OrdinalIgnoreCase));
                    if (mediaType != null) { content.MediaTypeId = mediaType.Id; return true; }
                    return false;
                }
            case "series":
                if (string.IsNullOrWhiteSpace(val)) return false;
                if (int.TryParse(val, out var seriesId)) { content.SeriesId = seriesId; return true; }
                else
                {
                    var lookups = await GetLookupsAsync();
                    var series = lookups?.Series.FirstOrDefault(s => s.Name.Equals(val, StringComparison.OrdinalIgnoreCase));
                    if (series != null) { content.SeriesId = series.Id; content.OtherSeries = ""; }
                    else { content.SeriesId = null; content.OtherSeries = val; }
                    return true;
                }
            case "tags":
                if (string.IsNullOrWhiteSpace(val)) return false;
                {
                    var lookups = await GetLookupsAsync();
                    var tags = content.Tags.ToList();
                    var added = false;
                    foreach (var request in val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        var tag = lookups?.Tags.FirstOrDefault(t =>
                            t.Code.Equals(request, StringComparison.OrdinalIgnoreCase) || t.Name.Equals(request, StringComparison.OrdinalIgnoreCase));
                        if (tag != null && !tags.Any(x => x.Id == tag.Id)) { tags.Add(new ContentTagModel(tag.Id, tag.Code, tag.Name)); added = true; }
                    }
                    if (added) content.Tags = tags;
                    return added;
                }
            case "topics":
                if (string.IsNullOrWhiteSpace(val)) return false;
                {
                    var lookups = await GetLookupsAsync();
                    var topics = content.Topics.ToList();
                    var added = false;
                    foreach (var request in val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        var topic = lookups?.Topics.FirstOrDefault(t => t.Name.Equals(request, StringComparison.OrdinalIgnoreCase));
                        if (topic != null && !topics.Any(x => x.Id == topic.Id))
                        {
                            topics.Add(new TNO.API.Areas.Services.Models.Content.ContentTopicModel { Id = topic.Id, Name = topic.Name, TopicType = topic.TopicType });
                            added = true;
                        }
                    }
                    if (added) content.Topics = topics;
                    return added;
                }
            case "sentiment":
                if (int.TryParse(val, out var sentiment))
                {
                    var pools = content.TonePools.Where(p => p.Id != this.Options.DefaultTonePoolId).ToList();
                    pools.Add(new ContentTonePoolModel { Id = this.Options.DefaultTonePoolId, Value = Math.Clamp(sentiment, -5, 5) });
                    content.TonePools = pools;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Deep clone a content model for a new derived item, resetting identity and clearing the
    /// collections that should start fresh (actions, tags, tone pools, topics, labels, files).
    /// </summary>
    private ContentModel CloneContentModel(ContentModel content)
    {
        var clone = JsonSerializer.Deserialize<ContentModel>(JsonSerializer.Serialize(content, _jsonOptions), _jsonOptions)
            ?? new ContentModel();
        clone.Id = 0;
        clone.Uid = "";
        clone.ExternalUid = "";
        clone.Status = Entities.ContentStatus.Draft;
        clone.Actions = Array.Empty<ContentActionModel>();
        clone.Tags = Array.Empty<ContentTagModel>();
        clone.TonePools = Array.Empty<ContentTonePoolModel>();
        clone.Topics = Array.Empty<TNO.API.Areas.Services.Models.Content.ContentTopicModel>();
        clone.Labels = Array.Empty<TNO.API.Areas.Services.Models.Content.ContentLabelModel>();
        clone.FileReferences = Array.Empty<TNO.API.Areas.Services.Models.Content.FileReferenceModel>();
        clone.TimeTrackings = Array.Empty<TNO.API.Areas.Services.Models.Content.TimeTrackingModel>();
        clone.Quotes = Array.Empty<TNO.API.Areas.Services.Models.Content.QuoteModel>();
        return clone;
    }

    /// <summary>
    /// Produce a uid-safe slug from a name.
    /// </summary>
    private static string Slug(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "content";
        var slug = System.Text.RegularExpressions.Regex.Replace(name.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "content" : slug;
    }

    #endregion

    private sealed class PendingUpdates
    {
        public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Entities.ContentStatus? Status { get; set; }
        public List<int> ContentActionIds { get; } = new();
        public List<(int Id, string Code, string Name)> Tags { get; } = new();
        public int? Sentiment { get; set; }
        public int? ContributorId { get; set; }
        public bool Abort { get; set; }
        public bool HasContentChanges => Fields.Any() || Tags.Any() || Sentiment.HasValue || ContributorId.HasValue;
    }
    #endregion
    #endregion
}
