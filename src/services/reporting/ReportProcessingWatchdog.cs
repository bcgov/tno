using Confluent.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Reporting.Config;

namespace TNO.Services.Reporting;

/// <summary>
/// Tracks consumer progress and report deadlines using a monotonic clock.
/// Failures are latched until process replacement so a late task cannot make the worker healthy again.
/// </summary>
public sealed class ReportProcessingWatchdog
{
    private readonly object _sync = new();
    private readonly TimeProvider _clock;
    private readonly TimeSpan _stallTimeout;
    private readonly TimeSpan _reportTimeout;
    private readonly Dictionary<TopicPartitionOffset, (long Started, long LastProgress, string Stage, int ReportId, long? InstanceId)> _active = new();
    private long _lastPoll;
    private long? _pausedWithoutWork;
    private bool _expected;
    private bool _pollObserved;
    private string? _failure;

    /// <summary>
    /// Creates a watchdog with positive consumer and report deadlines.
    /// </summary>
    /// <param name="options">Reporting deadlines.</param>
    /// <param name="clock">Monotonic clock; defaults to the system clock.</param>
    public ReportProcessingWatchdog(ReportingOptions options, TimeProvider? clock = null)
    {
        if (options.ConsumerStallTimeoutSeconds <= 0 || options.ReportProcessingTimeoutSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Reporting watchdog timeouts must be positive.");
        _clock = clock ?? TimeProvider.System;
        _stallTimeout = TimeSpan.FromSeconds(options.ConsumerStallTimeoutSeconds);
        _reportTimeout = TimeSpan.FromSeconds(options.ReportProcessingTimeoutSeconds);
        _lastPoll = _clock.GetTimestamp();
    }

    /// <summary>
    /// Record a completed consumer poll, including polls that return no messages.
    /// </summary>
    public void PollCompleted()
    {
        lock (_sync)
        {
            _lastPoll = _clock.GetTimestamp();
            _pollObserved = true;
        }
    }

    /// <summary>
    /// Register a report before starting any asynchronous work.
    /// </summary>
    /// <param name="message">Message whose processing is starting.</param>
    /// <returns>False if the worker is unhealthy or a rebalance delivered overlapping work.</returns>
    public bool Started(ConsumeResult<string, ReportRequestModel> message)
    {
        lock (_sync)
        {
            if (_failure != null) return false;
            if (_active.Count != 0)
            {
                _failure = $"Overlapping reporting messages require consumer recovery. Offset:{message.TopicPartitionOffset}";
                return false;
            }
            var now = _clock.GetTimestamp();
            _active[message.TopicPartitionOffset] = (now, now, "Loading report", message.Message.Value.ReportId, message.Message.Value.ReportInstanceId);
            _pausedWithoutWork = null;
            return true;
        }
    }

    /// <summary>
    /// Remove a completed handler, including handlers whose commit failed.
    /// </summary>
    /// <param name="offset">Completed message.</param>
    public void Completed(TopicPartitionOffset offset)
    {
        lock (_sync) _active.Remove(offset);
    }

    /// <summary>
    /// Record the current stage of the single active report without extending its overall deadline.
    /// </summary>
    /// <param name="stage">Fixed operation name, without report content.</param>
    public void Progress(string stage)
    {
        lock (_sync)
        {
            if (_failure != null || _active.Count == 0) return;
            var (offset, report) = _active.Single();
            _active[offset] = (report.Started, _clock.GetTimestamp(), stage, report.ReportId, report.InstanceId);
        }
    }

    /// <summary>
    /// Permanently mark this process unhealthy. Subsequent successes cannot clear the failure.
    /// </summary>
    /// <param name="reason">Bounded diagnostic without message payloads.</param>
    public void Fail(string reason)
    {
        lock (_sync) _failure ??= reason;
    }

    /// <summary>
    /// Evaluate deadlines without making network calls or changing Kafka state.
    /// Intentional pause/sleep and empty queues do not fail consumer health.
    /// </summary>
    /// <param name="expectConsumer">Whether the service should currently be consuming.</param>
    /// <param name="paused">Whether Kafka consumption is paused.</param>
    /// <param name="consumerStopped">Whether a previously started consumer task has ended.</param>
    /// <returns>The latched failure reason, or null when healthy.</returns>
    public string? Check(bool expectConsumer, bool paused, bool consumerStopped)
    {
        lock (_sync)
        {
            if (_failure != null) return _failure;
            var now = _clock.GetTimestamp();
            foreach (var (offset, report) in _active)
            {
                if (_clock.GetElapsedTime(report.Started, now) >= _reportTimeout)
                    return _failure = $"Report processing deadline exceeded. ReportId:{report.ReportId} InstanceId:{report.InstanceId} Offset:{offset} Stage:{report.Stage} SecondsSinceProgress:{_clock.GetElapsedTime(report.LastProgress, now).TotalSeconds:F0}";
            }

            if (!expectConsumer)
            {
                _expected = false;
                _pollObserved = false;
                _pausedWithoutWork = null;
                return null;
            }
            if (!_expected)
            {
                _expected = true;
                _lastPoll = now;
            }
            if (consumerStopped && _pollObserved) return _failure = "Reporting consumer task stopped unexpectedly.";
            if (_clock.GetElapsedTime(_lastPoll, now) >= _stallTimeout)
                return _failure = "Reporting consumer has stopped polling.";

            if (paused && _active.Count == 0)
            {
                _pausedWithoutWork ??= now;
                if (_clock.GetElapsedTime(_pausedWithoutWork.Value, now) >= _stallTimeout)
                    return _failure = "Reporting consumer is paused without an active report.";
            }
            else _pausedWithoutWork = null;
            return null;
        }
    }
}
