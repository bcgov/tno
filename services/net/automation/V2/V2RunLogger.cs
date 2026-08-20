using Microsoft.Extensions.Logging;
using TNO.API.Areas.Admin.Models.Automation;

namespace TNO.Services.Automation.V2;

/// <summary>
/// V2RunLogger class, the buffered writer behind the run's decision log. Every decision is
/// recorded - LLM exchanges (prompts always included, no capture flag), condition evaluations,
/// exclusions, skips, and flushes - and flushed incrementally so a failed run still has its log
/// up to the failure. Also accumulates the run's instrumentation totals (calls, tokens).
/// </summary>
public class V2RunLogger
{
    private const int MaxStoredChars = 20000;
    private const int FlushThreshold = 25;

    private readonly IApiService _api;
    private readonly ILogger _logger;
    private readonly long _runId;
    private readonly string? _variant;
    private readonly List<AutomationRunLogModel> _buffer = new();
    private readonly object _sync = new();

    private int _llmCalls;
    private long _promptTokens;
    private long _completionTokens;

    /// <summary>get - Total LLM calls recorded.</summary>
    public int LlmCalls => _llmCalls;

    /// <summary>get - Total prompt tokens reported by the provider.</summary>
    public long PromptTokens => Interlocked.Read(ref _promptTokens);

    /// <summary>get - Total completion tokens reported by the provider.</summary>
    public long CompletionTokens => Interlocked.Read(ref _completionTokens);

    /// <summary>
    /// Creates a new instance of a V2RunLogger.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="runId"></param>
    /// <param name="variant">The comparison variant ('A'/'B'), or null for a normal run.</param>
    /// <param name="logger"></param>
    public V2RunLogger(IApiService api, long runId, string? variant, ILogger logger)
    {
        _api = api;
        _runId = runId;
        _variant = variant;
        _logger = logger;
    }

    /// <summary>
    /// Record an LLM exchange.
    /// </summary>
    public void LogLlm(string step, string? analysis, string? action, string? actionType, long? contentId, string prompt, string response, int? promptTokens, int? completionTokens, long durationMs, string outcome, int attempt = 1, string? detail = null)
    {
        Interlocked.Increment(ref _llmCalls);
        if (promptTokens.HasValue) Interlocked.Add(ref _promptTokens, promptTokens.Value);
        if (completionTokens.HasValue) Interlocked.Add(ref _completionTokens, completionTokens.Value);
        Add(new AutomationRunLogModel
        {
            StepName = Truncate(step, 100),
            AnalysisName = analysis == null ? null : Truncate(analysis, 100),
            ActionName = action == null ? null : Truncate(action, 100),
            ActionType = actionType == null ? null : Truncate(actionType, 50),
            ContentId = contentId,
            Attempt = attempt,
            IsLLM = true,
            Variant = _variant,
            Prompt = TruncateStored(prompt),
            Response = TruncateStored(response),
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            DurationMs = durationMs,
            Outcome = outcome,
            Detail = detail,
        });
    }

    /// <summary>
    /// Record an engine decision (no LLM involved; carries no token cost).
    /// </summary>
    public void LogDecision(string step, string? action, string? actionType, long? contentId, string outcome, string description, string? detail = null, long durationMs = 0)
    {
        Add(new AutomationRunLogModel
        {
            StepName = Truncate(step, 100),
            ActionName = action == null ? null : Truncate(action, 100),
            ActionType = actionType == null ? null : Truncate(actionType, 50),
            ContentId = contentId,
            IsLLM = false,
            Variant = _variant,
            Response = TruncateStored(description),
            DurationMs = durationMs,
            Outcome = outcome,
            Detail = detail,
        });
    }

    /// <summary>get - The API rejected this run as gone (deleted); logging has stopped and the
    /// engine should stop the run.</summary>
    public bool IsAbandoned { get; private set; }

    private void Add(AutomationRunLogModel entry)
    {
        if (IsAbandoned) return;
        bool flush;
        lock (_sync)
        {
            _buffer.Add(entry);
            flush = _buffer.Count >= FlushThreshold;
        }
        if (flush)
            // Fire-and-forget is deliberate here would lose ordering; flushing synchronously in the
            // hot path would serialize items. Kick an async flush and let failures log-and-retry on
            // the next threshold.
            _ = FlushAsync();
    }

    /// <summary>
    /// Flush buffered entries to the API. A persistence failure must not fail the run; entries
    /// stay buffered and the next flush retries them.
    /// </summary>
    public async Task FlushAsync()
    {
        if (IsAbandoned) return;
        AutomationRunLogModel[] batch;
        lock (_sync)
        {
            if (_buffer.Count == 0) return;
            batch = _buffer.ToArray();
            _buffer.Clear();
        }
        try
        {
            await _api.AddAutomationRunLogsAsync(_runId, batch);
        }
        catch (System.Net.Http.HttpRequestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.BadRequest or System.Net.HttpStatusCode.NotFound)
        {
            // The run record is gone (deleted while executing). Retrying forever spams the API
            // and burns the run's remaining work - drop the buffer and signal the engine to stop.
            IsAbandoned = true;
            _logger.LogWarning("Run {runId} no longer exists; dropped {count} log entrie(s) and stopping the run.", _runId, batch.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist {count} run log entrie(s) for run {runId}; they will be retried on the next flush.", batch.Length, _runId);
            lock (_sync) _buffer.InsertRange(0, batch);
        }
    }

    private static string Truncate(string value, int length)
        => value.Length <= length ? value : value[..length];

    private static string? TruncateStored(string? value)
    {
        if (value == null) return null;
        // Truncation is marked rather than silent so the viewer can say so.
        return value.Length <= MaxStoredChars ? value : value[..MaxStoredChars] + "…[truncated]";
    }
}
