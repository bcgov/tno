using TNO.Kafka;
using TNO.Kafka.Models;

namespace TNO.Services.ContentAnalysis;

/// <summary>
/// AnalysisConsumerKind enum, the analysis requests a consumer handles.
/// </summary>
public enum AnalysisConsumerKind
{
    /// <summary>Requests for added and changed content, and editor and administrator requests.</summary>
    Lifecycle = 0,
    /// <summary>Failed requests waiting out their backoff.</summary>
    Retry = 1,
    /// <summary>Requests sent by a backfill.</summary>
    Backfill = 2,
}

/// <summary>
/// AnalysisConsumer class, a Kafka consumer of one kind of analysis request, with the task that runs it.
/// </summary>
public class AnalysisConsumer
{
    #region Variables
    private CancellationTokenSource _cancellation = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The requests the consumer handles.
    /// </summary>
    public AnalysisConsumerKind Kind { get; }

    /// <summary>
    /// get - The topics it consumes.
    /// </summary>
    public string[] Topics { get; }

    /// <summary>
    /// get - The Kafka listener.
    /// </summary>
    public IKafkaListener<string, AnalysisRequestModel> Listener { get; }

    /// <summary>
    /// get/set - The task consuming messages.
    /// </summary>
    public Task? Task { get; set; }

    /// <summary>
    /// get/set - Whether the listener is left paused until the LLM is available again.
    /// </summary>
    public bool IsHeld { get; set; }

    /// <summary>
    /// get - Cancelled when the consumer stops.
    /// </summary>
    public CancellationToken Token => _cancellation.Token;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisConsumer object, initializes with specified parameters.
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="topics"></param>
    /// <param name="listener"></param>
    public AnalysisConsumer(AnalysisConsumerKind kind, string[] topics, IKafkaListener<string, AnalysisRequestModel> listener)
    {
        this.Kind = kind;
        this.Topics = topics;
        this.Listener = listener;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Cancel the running task.
    /// </summary>
    public void Cancel()
    {
        if (!_cancellation.IsCancellationRequested) _cancellation.Cancel();
    }

    /// <summary>
    /// Cancel the prior task and return the token for a new one.
    /// </summary>
    /// <returns></returns>
    public CancellationToken Restart()
    {
        Cancel();
        _cancellation.Dispose();
        _cancellation = new CancellationTokenSource();
        return _cancellation.Token;
    }
    #endregion
}
