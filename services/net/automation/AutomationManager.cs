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
            else if (this.State.Status != ServiceStatus.Running)
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

            var summary = await ExecuteRunAsync(profile, run.Id);
            run.Status = AdminAutomationRunStatus.Completed;
            run.CompletedOn = DateTime.UtcNow;
            run.Summary = JsonSerializer.Serialize(summary, _jsonOptions);
            run.Note = BuildRunNote(run.Note, summary);
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
        // Run-scoped filter result cache: each filter query executes at most once per run.
        // Filters return every match (paged); the run iterates all of them.
        var filterCache = new Dictionary<string, List<ContentModel>>();
        _lookups = null; // Refresh lookup caches (actions, tags, contributors) once per run.

        // Load lookup caches up front so parallel item processing never races the lazy fetch.
        await GetLookupsAsync();

        var contentItems = new List<ContentModel>();
        if (hasProfileFilter)
        {
            contentItems = new List<ContentModel>(
                await SearchFilterAsync(profile.FilterQuery!, profile.FilterSettings, FilterCacheKey(profile.FilterId, profile.FilterQuery!), filterCache));
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

            // Step filter behaviors.
            HashSet<long>? gateIds = null;
            string? resultsJson = null;
            List<ContentModel>? stepHits = null;
            var iterateStepFilter = step.IterateStepFilter && (step.Target == "start" || step.Target == "end");
            if (!IsEmptyQuery(step.FilterQuery))
            {
                stepHits = await SearchFilterAsync(step.FilterQuery!, step.FilterSettings, FilterCacheKey(step.FilterId, step.FilterQuery!), filterCache);
                var filterBehaviour = iterateStepFilter ? "iteration source"
                    : step.ApplyToAutomationFilter ? "gate"
                    : "prompt enrichment";
                this.Logger.LogInformation("Step '{step}' filter returned {count} content item(s) ({behaviour}).", step.Name, stepHits.Count, filterBehaviour);
                if (iterateStepFilter)
                {
                    // The step filter results are the iteration source; each hit becomes the
                    // step's content item (no gate, no enrichment injection).
                    foreach (var hit in stepHits)
                        contentById.TryAdd(hit.Id, hit);
                }
                else if (step.ApplyToAutomationFilter)
                    gateIds = stepHits.Select(item => item.Id).ToHashSet();
                else
                    resultsJson = JsonSerializer.Serialize(stepHits, _jsonOptions);
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
                            await ExecuteStepInstanceAsync(step, stepLlm, content, resultsJson, executionCounts, scores, contentById, stepSummary, summary.Changes, stepResponses, executedContentByAction, llmCache);
                        }
                        catch (Exception ex)
                        {
                            // One failed item (e.g. an exhausted LLM request) must not fail the
                            // whole run; record it and continue with the next item.
                            lock (stepSummary) stepSummary.Failures++;
                            this.Logger.LogError(ex, "Step '{step}' failed for content {contentId}; continuing with the next item.", step.Name, content.Id);
                        }
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
                                await ExecuteStepInstanceAsync(step, stepLlm, content, null, executionCounts, scores, contentById, stepSummary, summary.Changes, stepResponses, executedContentByAction, llmCache);
                            }
                            catch (Exception ex)
                            {
                                lock (stepSummary) stepSummary.Failures++;
                                this.Logger.LogError(ex, "Step '{step}' failed for content {contentId}; continuing with the next item.", step.Name, content.Id);
                            }
                        });
                        break;
                    }
                    try
                    {
                        await ExecuteStepInstanceAsync(step, stepLlm, null, resultsJson, executionCounts, scores, contentById, stepSummary, summary.Changes, stepResponses, executedContentByAction, llmCache);
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
        Dictionary<int, LLMModel> llmCache)
    {
        var actions = step.Actions.Where(action => action.IsEnabled).ToArray();
        if (actions.Length == 0) return;

        var contentJson = content != null ? JsonSerializer.Serialize(content, _jsonOptions) : null;
        // 'deduplicate' actions run their own LLM comparisons and 'always run' (AutoExecute)
        // actions require no confirmation; neither contributes to the composed step prompt.
        var promptActions = actions
            .Where(action => action.ActionType != "deduplicate" && !action.AutoExecute)
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
            var prompt = PromptComposer.Compose(
                step.Prompt,
                promptActions.Select(action => (action.Prompt, action.ContentField, action.Objective)),
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
        for (var index = 0; index < actions.Length; index++)
        {
            var action = actions[index];
            var actionSummary = stepSummary.Actions[index];

            // 'deduplicate' is not confirmed by the main step response; it runs its own LLM
            // comparison per previously-processed content item. A detected duplicate aborts
            // the step at this position (accumulated updates before it are still applied).
            if (action.ActionType == "deduplicate")
            {
                var dedupeLlm = await ResolveLlmAsync(action.LLMId, llm, llmCache);
                var isDuplicate = await DetectDuplicateAsync(
                    step, action, dedupeLlm, content, contentJson, contentById, executionCounts, index,
                    actionSummary, changes, responses, executedContentByAction);
                if (isDuplicate)
                {
                    lock (stepSummary) stepSummary.Aborts++;
                    break;
                }
                continue;
            }

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
                        PromptComposer.ComposeAction(action.Prompt, action.ContentField, action.Objective, contentJson, resultsJson),
                        scores, contentById);
                    // The first turn effectively sends the system prompt too; include it in the
                    // recorded prompt so the run information shows what the model received.
                    var recordedPrompt = conversation.Count == 1
                        ? $"[system]\n{conversation[0].Content}\n\n[user]\n{userPrompt}"
                        : userPrompt;
                    conversation.Add(("user", userPrompt));

                    response = await InvokeChatAsync(actionLlm, conversation);
                    conversation.Add(("assistant", response));

                    var responseSummary = new ResponseSummary
                    {
                        StepId = step.Id,
                        StepName = step.Name,
                        ActionName = action.Name,
                        ContentId = content?.Id,
                        Prompt = this.Options.IncludeLLMPromptsInSummary ? recordedPrompt : null,
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
                        new[] { (action.Prompt, action.ContentField, action.Objective) },
                        contentJson,
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

            executed = await ApplyActionAsync(action, value, pending, actionSummary, changes);
            if (executed)
            {
                lock (actionSummary) actionSummary.Executions++;
                if (content != null) TrackExecutedContent(executedContentByAction, action.Id, content.Id);
            }
            else ReleaseExecution(executionCounts, key);

            if (pending.Abort)
            {
                // 'Stop Remaining Actions' is position sensitive: updates accumulated by actions
                // ordered before it are still applied below; actions after it are skipped.
                lock (stepSummary) stepSummary.Aborts++;
                break;
            }
        }

        if (content != null)
            await ApplyPendingUpdatesAsync(step, content.Id, pending, changes);
        else if (pending.HasContentChanges || pending.Status != null || pending.ContentActionIds.Any())
            this.Logger.LogWarning("Step '{step}' confirmed content actions but has no iterated content item to apply them to.", step.Name);
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
    /// Compare the current content item against each item the configured prior action
    /// successfully processed. One LLM comparison per prior item; the first response that
    /// contains the action's confirmation statement marks the current item as a duplicate.
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
        Dictionary<int, List<long>> executedContentByAction)
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

        // Nothing processed by the prior action yet; nothing to compare against.
        // Items run in parallel, so snapshot the prior action's processed ids under the lock.
        long[] priorIds;
        lock (executedContentByAction)
        {
            if (!executedContentByAction.TryGetValue(action.PriorActionId.Value, out var tracked) || tracked.Count == 0)
                return false;
            priorIds = tracked.ToArray();
        }

        foreach (var priorId in priorIds)
        {
            if (priorId == content.Id) continue;

            var prior = contentById.TryGetValue(priorId, out var cached)
                ? cached
                : await this.Api.FindContentByIdAsync(priorId);
            if (prior == null) continue;

            var comparePrompt = action.Prompt
                .Replace("{content}", contentJson)
                .Replace("{previous}", JsonSerializer.Serialize(prior, _jsonOptions));
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

            if (matcher.TryMatch(response, out _))
            {
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
                actionSummary.Notes = $"Content {content.Id} is a duplicate of content {priorId}.";
                lock (changes) changes.Add(new ChangeSummary { ContentId = content.Id, Type = "duplicate", Value = priorId.ToString() });
                this.Logger.LogInformation("Content {contentId} detected as a duplicate of {priorId}; step '{step}' aborted.", content.Id, priorId, step.Name);
                return true;
            }
        }
        return false;
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
                // Queue the report with the reporting service; it generates an instance and
                // sends it to the report's subscribers.
                var reportRequest = new ReportRequestModel(ReportDestination.ReportingService, Entities.ReportType.Content, action.ReportId.Value, JsonDocument.Parse("{}"));
                var delivery = await this.Api.SendMessageAsync(reportRequest);
                if (delivery == null)
                {
                    actionSummary.Notes = $"Failed to send the report request for report {action.ReportId.Value}.";
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
                // Queue the notification with the notification service; it sends to the
                // notification's subscribers.
                try
                {
                    await this.Api.PublishNotificationAsync(action.NotificationId.Value);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to send the notification request for notification {id}.", action.NotificationId.Value);
                    actionSummary.Notes = $"Failed to send the notification request for notification {action.NotificationId.Value}.";
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

                var candidates = objectiveScores
                    .OrderByDescending(pair => pair.Value)
                    .Select(pair =>
                    {
                        contentById.TryGetValue(pair.Key, out var content);
                        return new
                        {
                            ContentId = pair.Key,
                            Score = pair.Value,
                            Headline = content?.Headline ?? "",
                            Source = content?.Source?.Name ?? content?.OtherSource ?? "",
                            Summary = content?.Summary ?? "",
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
    /// Execute a filter query and return ALL matching items, using the run-scoped cache so each
    /// filter is only requested once per run. The filter's stored "size" is a UI page size, not a
    /// cap - the engine pages through the full match set (bounded by Elasticsearch's
    /// max_result_window) so automation iterates every item the filter matches.
    /// </summary>
    private async Task<List<ContentModel>> SearchFilterAsync(
        string query,
        string? settings,
        string cacheKey,
        Dictionary<string, List<ContentModel>> cache)
    {
        if (cache.TryGetValue(cacheKey, out var cached)) return cached;

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
                this.Logger.LogWarning("Filter '{key}' exceeded the max result window ({max}); results truncated.", cacheKey, MaxResultWindow);
                break;
            }
        }

        cache[cacheKey] = hits;
        this.Logger.LogDebug("Filter '{key}' returned {count} item(s); results cached for this run.", cacheKey, hits.Count);
        return hits;
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
                var settings = JsonDocument.Parse(filterSettings);
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
            var document = JsonDocument.Parse(query);
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
    private async Task FlushRunResponsesAsync(long runId, List<ResponseSummary> responses)
    {
        if (responses.Count == 0) return;
        try
        {
            var models = responses.Select(r => new AdminAutomationRunResponseModel
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
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to persist {count} response record(s) for run {runId}; continuing.", responses.Count, runId);
        }
        finally
        {
            responses.Clear();
        }
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
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prompt { get; set; }

        public string Response { get; set; } = "";
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
