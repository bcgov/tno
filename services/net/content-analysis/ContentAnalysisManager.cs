using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.AI;
using TNO.AI.Analysis;
using TNO.AI.Tokens;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.ContentAnalysis.Config;
using TNO.Services.Managers;

namespace TNO.Services.ContentAnalysis;

/// <summary>
/// ContentAnalysisManager class, the Content-Analysis worker. The job table is the source of truth:
/// the worker claims due jobs through the API (lease + fencing token), analyzes the content's
/// current input, and submits the result with the input hash and token. Kafka 'analysis' messages
/// only wake it; it also polls, so a lost message delays work but never loses it.
/// </summary>
public class ContentAnalysisManager : ServiceManager<ContentAnalysisOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly IKafkaAdmin _kafkaAdmin;
    private readonly IKafkaListener<string, AnalysisRequestModel> _listener;
    private readonly SemaphoreSlim _wake = new(0);
    private readonly object _wakeLock = new();
    private DateTime? _nextWake;
    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}";
    private readonly HttpClient _httpClient;

    private ContentAnalysisSettingsModel? _settings;
    private API.Areas.Services.Models.LLM.LLMModel? _llm;
    private IReadOnlyList<AnalyzerTag> _tags = Array.Empty<AnalyzerTag>();
    private DateTime _settingsRefreshedOn = DateTime.MinValue;
    private readonly AnalysisProcess _processes;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="kafkaAdmin"></param>
    /// <param name="listener"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentAnalysisManager(
        IApiService api,
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, AnalysisRequestModel> listener,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<ContentAnalysisOptions> options,
        ILogger<ContentAnalysisManager> logger)
        : base(api, chesService, chesOptions, options, logger)
    {
        _kafkaAdmin = kafkaAdmin;
        _listener = listener;
        _listener.OnError += (_, e) => this.Logger.LogError(e.GetException(), "Content-Analysis wake listener failed");
        _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(Math.Max(30, this.Options.LLMRequestTimeoutSeconds)) };
        _processes = this.Options.GetProcesses();
        if (_processes == AnalysisProcess.None) this.Logger.LogWarning("Content-Analysis has no processes configured (Service:Processes); it will not claim jobs.");
        else this.Logger.LogInformation("Content-Analysis processes: {processes}", _processes);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the worker loop.
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
                _listener.Stop();
            }

            if (this.State.Status == ServiceStatus.Failed && this.Options.AutoRestartAfterCriticalFailure)
            {
                await Task.Delay(this.Options.RetryAfterCriticalFailureDelayMS);
                this.State.Resume();
            }

            if (this.State.Status == ServiceStatus.Running)
            {
                try
                {
                    ListenForWakeMessages();
                    await RefreshSettingsAsync();
                    if (_processes != AnalysisProcess.None)
                    {
                        ValidateLlm();
                        await ProcessAvailableJobsAsync();
                    }
                    this.State.ResetFailures();
                }
                catch (Exception ex) when (LlmConfigurationException.IsConfigurationError(ex))
                {
                    // No job is failed for it; the service retries, then sleeps until the LLM is fixed.
                    var failures = this.State.RecordFailure();
                    this.Logger.LogError(ex, "Content-Analysis LLM is misconfigured. This is failure [{failures}] out of [{max}] before the service sleeps.", failures, this.State.MaxFailureLimit);
                    await this.SendErrorEmailAsync("Content-Analysis LLM is misconfigured", ex);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Content-Analysis had an unexpected failure.");
                    this.State.RecordFailure();
                    await this.SendErrorEmailAsync("Content-Analysis had an unexpected failure", ex);
                }
            }

            await WaitForWorkAsync();
        }
    }

    /// <summary>
    /// Ensure the LLM the configured processes need is configured. Without it no jobs are claimed,
    /// so they wait rather than fail.
    /// </summary>
    /// <exception cref="LlmConfigurationException">The LLM is missing, or lacks an endpoint, key, deployment, or limits.</exception>
    private void ValidateLlm()
    {
        const AnalysisProcess modelProcesses = AnalysisProcess.Metadata | AnalysisProcess.Summary | AnalysisProcess.Quotes | AnalysisProcess.Tags | AnalysisProcess.Topics;
        if ((_processes & modelProcesses) == AnalysisProcess.None) return;
        if (_llm?.ProjectEndpoint == null || String.IsNullOrWhiteSpace(_llm.ApiKey) || String.IsNullOrWhiteSpace(_llm.DeploymentName))
            throw new LlmConfigurationException("Content-Analysis has no LLM configured (ContentAnalysisLLMId), or it has no endpoint, key, or deployment.");
        if (!GetLimits(_llm).IsValid)
            throw new LlmConfigurationException($"The LLM '{_llm.Name}' has no context window or output limit configured, or its output limit is not less than its context window.");
    }

    /// <summary>
    /// The LLM's request limits.
    /// </summary>
    /// <param name="llm"></param>
    /// <returns></returns>
    private static LlmLimits GetLimits(API.Areas.Services.Models.LLM.LLMModel? llm) =>
        llm != null ? new LlmLimits(llm.ContextWindow ?? 0, llm.MaxOutputTokens ?? 0, llm.TokenEstimation, llm.RequestsPerMinute, llm.TokensPerMinute) : new LlmLimits(0, 0, null, null, null);

    /// <summary>
    /// Subscribe to the wake topic, when it exists.
    /// </summary>
    private void ListenForWakeMessages()
    {
        var topics = this.Options.GetTopics();
        var existing = _kafkaAdmin.ListTopics();
        topics = topics.Intersect(existing).ToArray();
        if (topics.Length == 0) return;
        _listener.Subscribe(topics);
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            if (_cancelToken?.IsCancellationRequested == false) _cancelToken.Cancel();
            _cancelToken = new CancellationTokenSource();
            var token = _cancelToken.Token;
            _consumer = Task.Run(async () =>
            {
                while (this.State.Status == ServiceStatus.Running && !token.IsCancellationRequested)
                    await _listener.ConsumeAsync(HandleWakeAsync, token);
            }, token);
        }
    }

    /// <summary>
    /// A wake message: poll when its work becomes due. Committed on receipt.
    /// </summary>
    private Task HandleWakeAsync(ConsumeResult<string, AnalysisRequestModel> result)
    {
        _listener.Commit(result);
        var dueOn = result.Message.Value?.DueOn ?? DateTime.UtcNow;
        lock (_wakeLock)
        {
            if (dueOn <= DateTime.UtcNow) _wake.Release();
            else if (_nextWake == null || dueOn < _nextWake) _nextWake = dueOn;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Wait for the poll interval, a wake message, or the next scheduled wake, whichever is first.
    /// </summary>
    private async Task WaitForWorkAsync()
    {
        var wait = TimeSpan.FromSeconds(Math.Max(1, this.Options.PollIntervalSeconds));
        lock (_wakeLock)
        {
            if (_nextWake.HasValue)
            {
                var untilWake = _nextWake.Value - DateTime.UtcNow + TimeSpan.FromSeconds(1);
                if (untilWake < wait) wait = untilWake < TimeSpan.Zero ? TimeSpan.Zero : untilWake;
                if (_nextWake <= DateTime.UtcNow + wait) _nextWake = null;
            }
        }
        await _wake.WaitAsync(wait);
    }

    /// <summary>
    /// Refresh the runtime settings, the LLM, and the tags analysis may suggest.
    /// </summary>
    private async Task RefreshSettingsAsync()
    {
        if (DateTime.UtcNow - _settingsRefreshedOn < TimeSpan.FromSeconds(Math.Max(5, this.Options.SettingsRefreshSeconds))) return;
        _settings = await this.Api.GetContentAnalysisSettingsAsync();
        _llm = _settings?.LLMId.HasValue == true ? await this.Api.GetLLMAsync(_settings.LLMId.Value) : null;
        var lookups = await this.Api.GetLookupsAsync();
        _tags = lookups?.Tags.Where(t => t.IsEnabled).Select(t => new AnalyzerTag(t.Code, t.Name)).ToArray() ?? Array.Empty<AnalyzerTag>();
        _settingsRefreshedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Claim and process due jobs until none are left. Lifecycle work is claimed first; backfill
    /// takes at most its share of this instance's concurrency.
    /// </summary>
    private async Task ProcessAvailableJobsAsync()
    {
        var concurrency = Math.Max(1, this.Options.MaxConcurrency);
        var maxBackfill = Math.Max(1, (int)Math.Floor(concurrency * Math.Clamp(this.Options.BackfillShare, 0, 1)));
        while (this.State.Status == ServiceStatus.Running)
        {
            var jobs = (await this.Api.ClaimAnalysisJobsAsync(new AnalysisClaimRequestModel()
            {
                WorkerId = _workerId,
                Quantity = concurrency,
                MaxBackfill = maxBackfill,
            }))?.ToArray() ?? Array.Empty<AnalysisJobModel>();
            if (jobs.Length == 0) return;
            await Task.WhenAll(jobs.Select(ProcessJobAsync));
        }
    }

    /// <summary>
    /// Analyze one claimed job and submit the result, renewing the lease while it runs.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    private async Task ProcessJobAsync(AnalysisJobModel job)
    {
        var lease = new AnalysisLeaseModel() { JobId = job.Id, FencingToken = job.FencingToken };
        using var claim = new CancellationTokenSource();
        using var renewal = new CancellationTokenSource();
        var renewTask = RenewLeaseAsync(job, lease, claim, renewal.Token);
        try
        {
            var input = await this.Api.GetAnalysisInputAsync(lease);
            if (input == null)
            {
                this.Logger.LogDebug("Analysis job {jobId} was no longer claimed or its content was deleted", job.Id);
                return;
            }
            if (!input.IsEligible)
            {
                this.Logger.LogDebug("Content {contentId} is not analyzed: {reason}", input.ContentId, input.IneligibleReason);
                return;
            }

            var analyzerOptions = new AnalyzerOptions()
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

            // Only the model-based processes need the LLM.
            var llm = _llm;
            if (analyzerOptions.UsesModel) ValidateLlm();
            var limits = GetLimits(llm);
            var endpoint = new LlmEndpoint(llm?.ProjectEndpoint ?? new Uri("http://localhost"), llm?.ApiKey ?? "", llm?.DeploymentName ?? "");
            var isBackfill = job.Reason == AnalysisJobReason.Backfill;

            var analyzer = new ContentAnalyzer(new LlmDirectClient(_httpClient, this.Logger), analyzerOptions, this.Logger);
            var result = await analyzer.AnalyzeAsync(
                new AnalyzerInput(input.ContentId, input.Headline, input.Body, input.Byline, input.Summary, input.Source, input.MediaType, input.Series, input.PublishedOn),
                endpoint,
                limits,
                _tags,
                (tokens, token) => WaitForCapacityAsync(endpoint, limits, tokens, isBackfill, token),
                claim.Token);

            var submitted = await this.Api.SubmitAnalysisAsync(ToModel(lease, input, analyzerOptions.UsesModel ? llm : null, _processes, result));
            this.Logger.LogInformation("Analysis of content {contentId} {status}{reason}{fields}",
                input.ContentId, submitted?.Status, submitted?.Reason != null ? $": {submitted.Reason}" : "",
                submitted?.PopulatedFields.Any() == true ? $" (populated {String.Join(", ", submitted.PopulatedFields)})" : "");
        }
        catch (OperationCanceledException) when (claim.IsCancellationRequested)
        {
            this.Logger.LogWarning("Analysis job {jobId} lost its claim and was abandoned", job.Id);
        }
        catch (Exception ex) when (LlmConfigurationException.IsConfigurationError(ex))
        {
            // The content is not at fault: return the job without counting an attempt, and fail the
            // cycle so the service retries, then sleeps.
            this.Logger.LogWarning("Analysis job {jobId} for content {contentId} was returned to the queue: {error}", job.Id, job.ContentId, ex.Message);
            await this.Api.FailAnalysisAsync(new AnalysisFailureModel() { JobId = job.Id, FencingToken = job.FencingToken, Error = ex.Message, IsAttempt = false });
            throw;
        }
        catch (Exception ex)
        {
            // Input that cannot be split is permanent; the rest retry.
            var isTransient = ex is not InvalidOperationException;
            this.Logger.LogError(ex, "Analysis job {jobId} for content {contentId} failed", job.Id, job.ContentId);
            await this.Api.FailAnalysisAsync(new AnalysisFailureModel() { JobId = job.Id, FencingToken = job.FencingToken, Error = ex.Message, IsTransient = isTransient });
        }
        finally
        {
            renewal.Cancel();
            try { await renewTask; } catch (OperationCanceledException) { }
        }
    }

    /// <summary>
    /// Renew the lease at a third of its length; cancel the work when the claim is lost.
    /// </summary>
    private async Task RenewLeaseAsync(AnalysisJobModel job, AnalysisLeaseModel lease, CancellationTokenSource claim, CancellationToken cancellationToken)
    {
        var leaseLength = (job.LeaseExpiresOn ?? DateTime.UtcNow.AddMinutes(10)) - DateTime.UtcNow;
        var interval = TimeSpan.FromSeconds(Math.Max(10, leaseLength.TotalSeconds / 3));
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(interval, cancellationToken);
            var renewed = await this.Api.RenewAnalysisLeaseAsync(lease);
            if (renewed == null)
            {
                claim.Cancel();
                return;
            }
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
    private static AnalysisResultModel ToModel(AnalysisLeaseModel lease, AnalysisInputModel input, API.Areas.Services.Models.LLM.LLMModel? llm, AnalysisProcess processes, AnalyzerResult result)
    {
        return new AnalysisResultModel()
        {
            JobId = lease.JobId,
            FencingToken = lease.FencingToken,
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
