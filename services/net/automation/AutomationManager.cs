using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AdminAutomationProfileModel = TNO.API.Areas.Admin.Models.Automation.AutomationProfileModel;
using AdminAutomationRunModel = TNO.API.Areas.Admin.Models.Automation.AutomationRunModel;
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
    private readonly ITNOElasticClient _elasticClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly IKafkaAdmin _kafkaAdmin;
    private readonly IKafkaListener<string, AutomationRequestModel> _listener;
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
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public AutomationManager(
        IApiService api,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
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
        _kafkaAdmin = kafkaAdmin;
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
                // The definition-document engine (run context, collections, phases, analyses,
                // always-on decision log, dry runs). Schema version 1 is no longer supported.
                // Drafts persist with validation errors; runs must not execute one.
                var parsed = TNO.API.Areas.Admin.Models.Automation.AutomationDefinition.Parse(profile.Definition!);
                var invalid = TNO.API.Areas.Admin.Models.Automation.AutomationDefinitionValidator.Validate(parsed)
                    .Where(e => e.Severity == "error")
                    .ToArray();
                if (invalid.Length > 0)
                    throw new InvalidOperationException(
                        $"The definition is invalid and cannot run ({invalid.Length} error(s)): " +
                        string.Join("; ", invalid.Take(5).Select(e => $"{e.Path}: {e.Message}")));
                var engine = new Engine.AutomationEngine(this.Api, _elasticClient, _elasticOptions, this.Options, this.Logger);
                var runSummary = await engine.ExecuteAsync(profile, run);
                run.Status = AdminAutomationRunStatus.Completed;
                run.CompletedOn = DateTime.UtcNow;
                run.Summary = JsonSerializer.Serialize(runSummary, _jsonOptions);
                run.Note = BuildRunNote(run.Note, runSummary);
            }
            else
            {
                throw new InvalidOperationException("Schema version 1 profiles are no longer supported; only v2 definition profiles can run.");
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


    private static string BuildRunNote(string? note, Engine.RunSummary summary)
    {
        var variant = summary.VariantA;
        var result = summary.IsComparison
            ? $"Comparison run: {summary.Differences.Count} item(s) differ between the variants."
            : $"Executed {variant?.Steps.Count ?? 0} step(s), {variant?.LlmCalls ?? 0} LLM call(s), {variant?.Changes.Count ?? 0} change(s), {variant?.Excluded.Count ?? 0} exclusion(s).";
        if (summary.IsDryRun) result = $"DRY RUN - nothing was written. {result}";
        return string.IsNullOrWhiteSpace(note) ? result : $"{note} | {result}";
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

            // The decision log keeps the current date only (independent of run retention):
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

    #endregion
}
