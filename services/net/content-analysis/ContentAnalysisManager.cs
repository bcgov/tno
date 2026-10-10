using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.AI;
using TNO.AI.Analysis;
using TNO.AI.Tokens;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.ContentAnalysis.Config;
using TNO.Services.Managers;

namespace TNO.Services.ContentAnalysis;

/// <summary>
/// ContentAnalysisManager class, the Content-Analysis consumer. Kafka is the source of work: each
/// analysis request is a message keyed by content ID, so requests for the same content are handled
/// in order and instances scale with the topic's partitions. Lifecycle requests, retries, and
/// backfill each have their own consumer, so a backfill never delays new content.
///
/// A request is handled one at a time per consumer and committed once its outcome is recorded:
/// - a lifecycle request waits out the quiet period, so a burst of edits is analyzed once;
/// - a request whose input has since changed is skipped (a newer request follows it), as is one
///   whose analysis is already current, unless it is forced;
/// - a failure is sent to the retry topic with a backoff, and to the dead-letter topic once its
///   attempts are exhausted;
/// - a request for a reason the service is not configured to analyze is committed unanalyzed;
/// - a misconfigured LLM, or an unreachable API, is not the content's fault: the message is not
///   committed, so it is received again once the service recovers. While the LLM is misconfigured
///   the consumers hold at the first request that needs it, and one email is sent, rather than
///   failing (and emailing) every request.
/// </summary>
public class ContentAnalysisManager : ServiceManager<ContentAnalysisOptions>
{
    #region Variables
    private readonly TaskStatus[] _notRunning = new[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly IKafkaAdmin _kafkaAdmin;
    private readonly HttpClient _httpClient;
    private readonly AnalysisProcess _processes;
    private readonly bool _usesModel;
    private readonly AnalysisRequestReason[] _reasons;
    private readonly AnalysisConsumer[] _consumers;

    private readonly SemaphoreSlim _settingsLock = new(1, 1);
    private ContentAnalysisSettingsModel? _settings;
    private API.Areas.Services.Models.LLM.LLMModel? _llm;
    private IReadOnlyList<AnalyzerTag> _tags = Array.Empty<AnalyzerTag>();
    private DateTime _settingsRefreshedOn = DateTime.MinValue;

    private readonly object _llmLock = new();
    private string? _llmError;
    private string? _llmErrorNotified;
    private DateTime _llmHeldUntil = DateTime.MinValue;

    private readonly object _workOrderLock = new();
    private readonly Dictionary<long, (WorkOrderStatus Status, DateTime CheckedOn)> _workOrders = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="kafkaAdmin"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentAnalysisManager(
        IApiService api,
        IKafkaAdmin kafkaAdmin,
        IServiceProvider serviceProvider,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<ContentAnalysisOptions> options,
        ILogger<ContentAnalysisManager> logger)
        : base(api, chesService, chesOptions, options, logger)
    {
        _kafkaAdmin = kafkaAdmin;
        _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(Math.Max(30, this.Options.LLMRequestTimeoutSeconds)) };
        _processes = this.Options.GetProcesses();
        if (_processes == AnalysisProcess.None) this.Logger.LogWarning("Content-Analysis has no processes configured (Service:Processes); it will not consume analysis requests.");
        else this.Logger.LogInformation("Content-Analysis processes: {processes}", _processes);
        _usesModel = CreateAnalyzerOptions().UsesModel;
        _reasons = this.Options.GetReasons();
        this.Logger.LogInformation("Content-Analysis analyzes requests for: {reasons}", _reasons.Length > 0 ? String.Join(", ", _reasons) : "none (every request is committed unanalyzed)");

        var consumers = new List<AnalysisConsumer>();
        void Add(AnalysisConsumerKind kind, params string[] topics)
        {
            topics = topics.Where(t => !String.IsNullOrWhiteSpace(t)).ToArray();
            if (topics.Length == 0) return;
            var listener = serviceProvider.GetRequiredService<IKafkaListener<string, AnalysisRequestModel>>();
            // One message at a time; the listener pauses while it is handled, so the consumer stays
            // in its group however long the analysis takes.
            listener.IsLongRunningJob = true;
            var consumer = new AnalysisConsumer(kind, topics, listener);
            listener.OnError += (_, e) => this.Logger.LogError(e.GetException(), "Content-Analysis {kind} consumer failed", kind);
            listener.OnStop += (_, _) => consumer.Cancel();
            consumers.Add(consumer);
        }
        Add(AnalysisConsumerKind.Lifecycle, this.Options.GetTopics());
        Add(AnalysisConsumerKind.Retry, this.Options.RetryTopic);
        Add(AnalysisConsumerKind.Backfill, this.Options.BackfillTopic);
        _consumers = consumers.ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Keep the consumers running until the service stops.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();
                foreach (var consumer in _consumers) consumer.Listener.Stop();
            }
            else if (this.State.Status == ServiceStatus.Failed && this.Options.AutoRestartAfterCriticalFailure)
            {
                await Task.Delay(this.Options.RetryAfterCriticalFailureDelayMS);
                this.State.Resume();
            }
            else if (this.State.Status == ServiceStatus.Running && _processes != AnalysisProcess.None)
            {
                try
                {
                    await RefreshSettingsAsync();
                    await CheckLlmAsync();
                    StartConsumers();
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Content-Analysis had an unexpected failure.");
                    this.State.RecordFailure();
                    await this.SendErrorEmailAsync("Content-Analysis had an unexpected failure", ex);
                }
            }

            await Task.Delay(Math.Max(1000, this.Options.DefaultDelayMS));
        }
    }

    /// <summary>
    /// Subscribe each consumer to its topics that exist, and start consuming.
    /// </summary>
    private void StartConsumers()
    {
        var existing = _kafkaAdmin.ListTopics();
        foreach (var consumer in _consumers)
        {
            var topics = consumer.Topics.Intersect(existing).ToArray();
            if (topics.Length == 0)
            {
                this.Logger.LogWarning("Content-Analysis {kind} topics do not exist: {topics}", consumer.Kind, String.Join(", ", consumer.Topics));
                continue;
            }
            consumer.Listener.Subscribe(topics);
            if (consumer.Task == null || _notRunning.Contains(consumer.Task.Status))
            {
                var token = consumer.Restart();
                consumer.Task = Task.Run(async () =>
                {
                    while (this.State.Status == ServiceStatus.Running && !token.IsCancellationRequested)
                        await consumer.Listener.ConsumeAsync(result => HandleMessageAsync(consumer, result), token);
                    consumer.Listener.Stop();
                }, token);
            }
        }
    }

    /// <summary>
    /// Handle a request and commit it. When it cannot be handled through no fault of the content
    /// (the LLM is misconfigured, or the API or Kafka is unreachable) it is not committed: the
    /// consumer returns to it, and the failure counts toward the service sleeping.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(AnalysisConsumer consumer, ConsumeResult<string, AnalysisRequestModel> result)
    {
        var hold = false;
        try
        {
            if (this.State.Status != ServiceStatus.Running)
            {
                ReturnTo(consumer, result);
                return;
            }
            if (NeedsLlm(result.Message.Value) && !IsLlmAvailable())
            {
                ReturnTo(consumer, result);
                hold = true;
                return;
            }

            await ProcessRequestAsync(consumer, result.Message.Value, consumer.Token);
            consumer.Listener.Commit(result);
            this.State.ResetFailures();
        }
        catch (OperationCanceledException) when (consumer.Token.IsCancellationRequested)
        {
            // The consumer is stopping; the request is received again when it restarts.
            ReturnTo(consumer, result);
        }
        catch (Exception ex) when (LlmConfigurationException.IsConfigurationError(ex))
        {
            // Not counted as a failure: every request would fail the same way, so the consumers hold
            // until the LLM is available rather than putting the service to sleep.
            ReturnTo(consumer, result);
            hold = true;
            await HoldForLlmAsync(ex.Message, ex, DateTime.UtcNow.AddMilliseconds(Math.Max(1000, this.Options.RetryAfterCriticalFailureDelayMS)));
        }
        catch (Exception ex)
        {
            ReturnTo(consumer, result);
            var failures = this.State.RecordFailure();
            this.Logger.LogError(ex, "Content-Analysis failed to handle a request for content {contentId}. This is failure [{failures}] out of [{max}] before the service sleeps.", result.Message.Key, failures, this.State.MaxFailureLimit);
            await this.SendErrorEmailAsync("Content-Analysis failed to handle a request", ex);
            // Give what failed time to recover before the request is received again.
            await Task.Delay(Math.Max(1000, this.Options.RetryDelayMS));
        }
        finally
        {
            if (this.State.Status == ServiceStatus.Running)
            {
                // A request held for the LLM leaves the listener paused, so it is not received again
                // until the LLM is available; CheckLlmAsync resumes it.
                lock (_llmLock)
                {
                    consumer.IsHeld = hold && _llmError != null;
                }
                if (!consumer.IsHeld) consumer.Listener.Resume();
            }
        }
    }

    /// <summary>
    /// Return the consumer to the message so it is received again. After a rebalance the partition
    /// may belong to another instance, which receives it from the last commit instead.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="result"></param>
    private void ReturnTo(AnalysisConsumer consumer, ConsumeResult<string, AnalysisRequestModel> result)
    {
        try
        {
            consumer.Listener.Seek(result);
        }
        catch (KafkaException ex)
        {
            this.Logger.LogWarning(ex, "Content-Analysis could not return to topic {topic} partition {partition} offset {offset}", result.Topic, result.Partition.Value, result.Offset.Value);
        }
    }

    /// <summary>
    /// Analyze the content when the request is due and still current, and record the outcome.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessRequestAsync(AnalysisConsumer consumer, AnalysisRequestModel? request, CancellationToken cancellationToken)
    {
        if (request == null || request.ContentId == 0)
        {
            this.Logger.LogWarning("Content-Analysis received an empty request");
            return;
        }
        if (!_reasons.Contains(request.Reason))
        {
            this.Logger.LogDebug("Content {contentId} is not analyzed: {reason} requests are not analyzed (Service:Reasons)", request.ContentId, request.Reason);
            return;
        }
        if (request.WorkOrderId.HasValue && await IsCancelledAsync(request.WorkOrderId.Value))
        {
            this.Logger.LogDebug("Content {contentId} is not analyzed: backfill {workOrderId} was cancelled", request.ContentId, request.WorkOrderId);
            return;
        }

        // Every request in a partition waits the same quiet period, so they become due in order and
        // waiting on this one holds up none that are due sooner.
        var dueOn = consumer.Kind switch
        {
            AnalysisConsumerKind.Retry => request.NotBefore ?? DateTime.UtcNow,
            AnalysisConsumerKind.Lifecycle when request.Reason == AnalysisRequestReason.Lifecycle => request.RequestedOn.AddSeconds(Math.Max(0, this.Options.QuietPeriodSeconds)),
            _ => DateTime.UtcNow,
        };
        var wait = dueOn - DateTime.UtcNow;
        if (wait > TimeSpan.Zero) await Task.Delay(wait, cancellationToken);

        await RefreshSettingsAsync();
        var input = await this.Api.GetAnalysisInputAsync(request.ContentId);
        if (input == null)
        {
            this.Logger.LogDebug("Content {contentId} is not analyzed: it was deleted", request.ContentId);
            return;
        }
        if (input.InputHash != request.InputHash)
        {
            this.Logger.LogDebug("Content {contentId} is not analyzed: it changed after request {requestId}, and a newer request follows", request.ContentId, request.RequestId);
            return;
        }
        if (!request.Force && input.AnalysisInputHash == input.InputHash)
        {
            this.Logger.LogDebug("Content {contentId} is not analyzed: its analysis is current", request.ContentId);
            return;
        }
        if (!input.IsEligible)
        {
            this.Logger.LogDebug("Content {contentId} is not analyzed: {reason}", request.ContentId, input.IneligibleReason);
            await RecordRunAsync(request, AnalysisRunStatus.Skipped, request.Attempts, input.IneligibleReason);
            return;
        }

        var attempt = request.Attempts + 1;
        try
        {
            await AnalyzeAsync(consumer, request, input, attempt, cancellationToken);
        }
        catch (Exception ex) when (!LlmConfigurationException.IsConfigurationError(ex) && ex is not OperationCanceledException && ex is not HttpClientRequestException)
        {
            await HandleFailureAsync(request, attempt, ex);
        }
    }

    /// <summary>
    /// Analyze the content's input and submit the result with the request's run.
    /// </summary>
    private async Task AnalyzeAsync(AnalysisConsumer consumer, AnalysisRequestModel request, AnalysisInputModel input, int attempt, CancellationToken cancellationToken)
    {
        var analyzerOptions = CreateAnalyzerOptions();

        // Only the model-based processes need the LLM.
        var llm = _llm;
        if (analyzerOptions.UsesModel) ValidateLlm(llm);
        var limits = GetLimits(llm);
        var endpoint = new LlmEndpoint(llm?.ProjectEndpoint ?? new Uri("http://localhost"), llm?.ApiKey ?? "", llm?.DeploymentName ?? "");
        var isBackfill = consumer.Kind == AnalysisConsumerKind.Backfill || request.Reason == AnalysisRequestReason.Backfill;

        var analyzer = new ContentAnalyzer(new LlmDirectClient(_httpClient, this.Logger), analyzerOptions, this.Logger);
        var result = await analyzer.AnalyzeAsync(
            new AnalyzerInput(input.ContentId, input.Headline, input.Body, input.Byline, input.Summary, input.Source, input.MediaType, input.Series, input.PublishedOn),
            endpoint,
            limits,
            _tags,
            (tokens, token) => WaitForCapacityAsync(endpoint, limits, tokens, isBackfill, token),
            cancellationToken);

        var submitted = await this.Api.SubmitAnalysisAsync(ToModel(request, attempt, input, analyzerOptions.UsesModel ? llm : null, _processes, result));
        if (analyzerOptions.UsesModel) ReportLlmWorking();
        this.Logger.LogInformation("Analysis of content {contentId} {status}{reason}{fields}",
            input.ContentId, submitted?.Status, submitted?.Reason != null ? $": {submitted.Reason}" : "",
            submitted?.PopulatedFields.Any() == true ? $" (populated {String.Join(", ", submitted.PopulatedFields)})" : "");
    }

    /// <summary>
    /// Send a failed request to the retry topic with a backoff, or, once its attempts are exhausted
    /// or its failure is permanent, to the dead-letter topic; and record the run.
    /// </summary>
    private async Task HandleFailureAsync(AnalysisRequestModel request, int attempt, Exception ex)
    {
        // Input that cannot be split is permanent; the rest retry.
        var isTransient = ex is not InvalidOperationException;
        var canRetry = isTransient && attempt < Math.Max(1, this.Options.MaxAttempts) && !String.IsNullOrWhiteSpace(this.Options.RetryTopic);
        var failed = new AnalysisRequestModel(request.RequestId, request.ContentId, request.InputHash, request.Reason, request.Force, request.RequestedOn)
        {
            WorkOrderId = request.WorkOrderId,
            Attempts = attempt,
            Error = ex.Message,
        };

        if (canRetry)
        {
            var delay = Math.Min(Math.Max(1, this.Options.MaxRetryDelaySeconds), Math.Max(1, this.Options.RetryDelaySeconds) * Math.Pow(2, attempt - 1)) + Random.Shared.Next(0, 30);
            failed.NotBefore = DateTime.UtcNow.AddSeconds(delay);
            this.Logger.LogWarning(ex, "Analysis of content {contentId} failed (attempt {attempt} of {max}); retrying after {notBefore}", request.ContentId, attempt, this.Options.MaxAttempts, failed.NotBefore);
            await this.Api.SendAnalysisRequestsAsync(this.Options.RetryTopic, new[] { failed });
            await RecordRunAsync(request, AnalysisRunStatus.Retrying, attempt, ex.Message);
            return;
        }

        this.Logger.LogError(ex, "Analysis of content {contentId} failed after {attempt} attempt(s)", request.ContentId, attempt);
        if (!String.IsNullOrWhiteSpace(this.Options.DeadLetterTopic))
            await this.Api.SendAnalysisRequestsAsync(this.Options.DeadLetterTopic, new[] { failed });
        await RecordRunAsync(request, AnalysisRunStatus.Failed, attempt, ex.Message);
    }

    /// <summary>
    /// Record the outcome of a request that produced no result.
    /// </summary>
    private Task<TNO.Entities.Models.AnalysisMetadata?> RecordRunAsync(AnalysisRequestModel request, AnalysisRunStatus status, int attempts, string? error)
    {
        return this.Api.RecordAnalysisRunAsync(request.ContentId, new AnalysisRunRequestModel()
        {
            RequestId = request.RequestId,
            Reason = request.Reason,
            InputHash = request.InputHash,
            RequestedOn = request.RequestedOn,
            Attempts = attempts,
            WorkOrderId = request.WorkOrderId,
            Status = status,
            Error = error,
        });
    }

    /// <summary>
    /// Whether the backfill work order was cancelled; its status is cached briefly.
    /// </summary>
    private async Task<bool> IsCancelledAsync(long workOrderId)
    {
        lock (_workOrderLock)
        {
            if (_workOrders.TryGetValue(workOrderId, out var cached) && DateTime.UtcNow - cached.CheckedOn < TimeSpan.FromSeconds(Math.Max(1, this.Options.WorkOrderRefreshSeconds)))
                return cached.Status == WorkOrderStatus.Cancelled;
        }
        var workOrder = await this.Api.FindWorkOrderAsync(workOrderId);
        var status = workOrder?.Status ?? WorkOrderStatus.Cancelled;
        lock (_workOrderLock)
        {
            _workOrders[workOrderId] = (status, DateTime.UtcNow);
        }
        return status == WorkOrderStatus.Cancelled;
    }

    /// <summary>
    /// The analyzer options for the configured processes.
    /// </summary>
    /// <returns></returns>
    private AnalyzerOptions CreateAnalyzerOptions()
    {
        return new AnalyzerOptions()
        {
            SafetyMarginPercent = this.Options.SafetyMarginPercent,
            OverlapTokens = this.Options.OverlapTokens,
            MinTextCharacters = this.Options.MinTextCharacters,
            RequestAttempts = this.Options.LLMRequestAttempts,
            ExtractMetadata = _processes.HasFlag(AnalysisProcess.Metadata),
            Summarize = _processes.HasFlag(AnalysisProcess.Summary),
            ExtractQuotes = _processes.HasFlag(AnalysisProcess.Quotes),
            SuggestTags = _processes.HasFlag(AnalysisProcess.Tags),
            SuggestContributor = _processes.HasFlag(AnalysisProcess.Contributor),
            ChooseTopic = _processes.HasFlag(AnalysisProcess.Topics),
        };
    }

    /// <summary>
    /// Whether handling the request may call the LLM. A request this service does not analyze is
    /// committed without it, so it is never held.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private bool NeedsLlm(AnalysisRequestModel? request) => _usesModel && request != null && _reasons.Contains(request.Reason);

    /// <summary>
    /// Whether the LLM is available, i.e. not held for a misconfiguration.
    /// </summary>
    /// <returns></returns>
    private bool IsLlmAvailable()
    {
        lock (_llmLock)
        {
            return _llmError == null;
        }
    }

    /// <summary>
    /// Check the LLM's configuration. A misconfiguration holds the consumers before any request
    /// fails; once the LLM is configured (and any hold for an error the LLM returned has passed),
    /// the held consumers resume.
    /// </summary>
    /// <returns></returns>
    private async Task CheckLlmAsync()
    {
        var error = _usesModel ? GetLlmError(_llm) : null;
        if (error != null)
        {
            await HoldForLlmAsync(error, new LlmConfigurationException(error), DateTime.MinValue);
            return;
        }

        AnalysisConsumer[] held;
        lock (_llmLock)
        {
            if (_llmError == null || DateTime.UtcNow < _llmHeldUntil) return;
            _llmError = null;
            held = _consumers.Where(c => c.IsHeld).ToArray();
            foreach (var consumer in held) consumer.IsHeld = false;
        }
        this.Logger.LogInformation("Content-Analysis LLM is configured; analysis resumes.");
        foreach (var consumer in held) consumer.Listener.Resume();
    }

    /// <summary>
    /// Hold the consumers until the LLM is available, and send one email for each new misconfiguration
    /// (not one for every request it fails).
    /// </summary>
    /// <param name="error">The misconfiguration.</param>
    /// <param name="ex">The exception to email.</param>
    /// <param name="heldUntil">The earliest the consumers may resume, for an error the LLM returned
    /// that its configuration cannot reveal (e.g. a rejected key).</param>
    /// <returns></returns>
    private async Task HoldForLlmAsync(string error, Exception ex, DateTime heldUntil)
    {
        bool isNew, notify;
        lock (_llmLock)
        {
            isNew = _llmError != error;
            notify = _llmErrorNotified != error;
            _llmError = error;
            _llmErrorNotified = error;
            if (heldUntil > _llmHeldUntil) _llmHeldUntil = heldUntil;
        }
        if (isNew) this.Logger.LogError(ex, "Content-Analysis LLM is misconfigured; analysis is held until it is configured.");
        if (notify) await this.SendErrorEmailAsync("Content-Analysis LLM is misconfigured", ex);
    }

    /// <summary>
    /// The LLM answered, so a later misconfiguration is emailed again, even if it is the same one.
    /// </summary>
    private void ReportLlmWorking()
    {
        lock (_llmLock)
        {
            _llmErrorNotified = null;
        }
    }

    /// <summary>
    /// The LLM's misconfiguration, if any.
    /// </summary>
    /// <param name="llm"></param>
    /// <returns>Why the LLM cannot be used, or null.</returns>
    private static string? GetLlmError(API.Areas.Services.Models.LLM.LLMModel? llm)
    {
        if (llm?.ProjectEndpoint == null || String.IsNullOrWhiteSpace(llm.ApiKey) || String.IsNullOrWhiteSpace(llm.DeploymentName))
            return "Content-Analysis has no LLM configured (ContentAnalysisLLMId), or it has no endpoint, key, or deployment.";
        if (!GetLimits(llm).IsValid)
            return $"The LLM '{llm.Name}' has no context window or output limit configured, or its output limit is not less than its context window.";
        return null;
    }

    /// <summary>
    /// Ensure the LLM the configured processes need is configured.
    /// </summary>
    /// <exception cref="LlmConfigurationException">The LLM is missing, or lacks an endpoint, key, deployment, or limits.</exception>
    private static void ValidateLlm(API.Areas.Services.Models.LLM.LLMModel? llm)
    {
        var error = GetLlmError(llm);
        if (error != null) throw new LlmConfigurationException(error);
    }

    /// <summary>
    /// The LLM's request limits.
    /// </summary>
    /// <param name="llm"></param>
    /// <returns></returns>
    private static LlmLimits GetLimits(API.Areas.Services.Models.LLM.LLMModel? llm) =>
        llm != null ? new LlmLimits(llm.ContextWindow ?? 0, llm.MaxOutputTokens ?? 0, llm.TokenEstimation, llm.RequestsPerMinute, llm.TokensPerMinute) : new LlmLimits(0, 0, null, null, null);

    /// <summary>
    /// Refresh the runtime settings, the LLM, and the tags analysis may suggest.
    /// </summary>
    private async Task RefreshSettingsAsync()
    {
        if (DateTime.UtcNow - _settingsRefreshedOn < TimeSpan.FromSeconds(Math.Max(5, this.Options.SettingsRefreshSeconds))) return;
        await _settingsLock.WaitAsync();
        try
        {
            if (DateTime.UtcNow - _settingsRefreshedOn < TimeSpan.FromSeconds(Math.Max(5, this.Options.SettingsRefreshSeconds))) return;
            _settings = await this.Api.GetContentAnalysisSettingsAsync();
            _llm = _settings?.LLMId.HasValue == true ? await this.Api.GetLLMAsync(_settings.LLMId.Value) : null;
            var lookups = await this.Api.GetLookupsAsync();
            _tags = lookups?.Tags.Where(t => t.IsEnabled).Select(t => new AnalyzerTag(t.Code, t.Name)).ToArray() ?? Array.Empty<AnalyzerTag>();
            _settingsRefreshedOn = DateTime.UtcNow;
        }
        finally
        {
            _settingsLock.Release();
        }
    }

    /// <summary>
    /// Keep requests within the LLM's rate limits; backfill work is further held to its share.
    /// </summary>
    private async Task WaitForCapacityAsync(LlmEndpoint endpoint, LlmLimits limits, int tokens, bool isBackfill, CancellationToken cancellationToken)
    {
        var key = LlmRateLimiter.GetKey(endpoint);
        if (isBackfill)
        {
            var share = Math.Clamp(this.Options.BackfillShare, 0.01, 1);
            int? Scale(int? value) => value.HasValue && value > 0 ? Math.Max(1, (int)(value.Value * share)) : null;
            await LlmRateLimiter.WaitAsync($"{key}|backfill", Scale(limits.RequestsPerMinute), Scale(limits.TokensPerMinute), tokens, cancellationToken);
        }
        await LlmRateLimiter.WaitAsync(key, limits.RequestsPerMinute, limits.TokensPerMinute, tokens, cancellationToken);
    }

    /// <summary>
    /// The submission for an analysis.
    /// </summary>
    private static AnalysisResultModel ToModel(AnalysisRequestModel request, int attempt, AnalysisInputModel input, API.Areas.Services.Models.LLM.LLMModel? llm, AnalysisProcess processes, AnalyzerResult result)
    {
        return new AnalysisResultModel()
        {
            ContentId = input.ContentId,
            Request = new AnalysisRequestRunModel()
            {
                RequestId = request.RequestId,
                Reason = request.Reason,
                InputHash = request.InputHash,
                RequestedOn = request.RequestedOn,
                Attempts = attempt,
                WorkOrderId = request.WorkOrderId,
            },
            InputHash = input.InputHash,
            IsMetadataOnly = result.IsMetadataOnly,
            Processes = processes,
            NormalizationVersion = ContentAnalyzer.NormalizationVersion,
            SchemaVersion = ContentAnalyzer.SchemaVersion,
            PromptVersion = ContentAnalyzer.PromptVersion,
            LLMId = llm?.Id,
            Model = llm?.DeploymentName ?? "",
            Summary = result.Summary,
            KeyFacts = result.KeyFacts.Select(f => new AnalysisFactModel() { Statement = f.Statement, IsInferred = f.IsInferred, Span = f.Span != null ? new AnalysisSpanModel() { Start = f.Span.Start, Length = f.Span.Length } : null }).ToArray(),
            Entities = result.Entities.Select(e => new AnalysisEntityModel() { Type = e.Type, Name = e.Name, Aliases = e.Aliases.ToArray(), Roles = e.Roles.ToArray(), IsAmbiguous = e.IsAmbiguous }).ToArray(),
            Places = result.Places.Select(p => new AnalysisPlaceModel() { Name = p.Name, Role = p.Role }).ToArray(),
            Topics = result.Topics.Select(t => new AnalysisTopicModel() { Label = t.Label, Relevance = t.Relevance }).ToArray(),
            PrimaryTopic = result.PrimaryTopic,
            SuggestedTags = result.SuggestedTags.ToArray(),
            SuggestedContributor = result.SuggestedContributor,
            Events = result.Events.Select(e => new AnalysisEventModel() { Actor = e.Actor, Action = e.Action, Date = e.Date, Location = e.Location }).ToArray(),
            Quotes = result.Quotes.Select(q => new AnalysisQuoteModel() { Statement = q.Statement, Speaker = q.Speaker, Span = new AnalysisSpanModel() { Start = q.Span.Start, Length = q.Span.Length } }).ToArray(),
            Validation = result.Validation,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
        };
    }
    #endregion
}
