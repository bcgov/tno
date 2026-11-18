using System.Collections.Concurrent;

namespace TNO.AI.Synthesis;

/// <summary>
/// LlmMessage record, one message of a request.
/// </summary>
/// <param name="Role">'system', 'user' or 'assistant'.</param>
/// <param name="Content">The text.</param>
public record LlmMessage(string Role, string Content);

/// <summary>
/// LlmExchangeKind class, the kinds of request a synthesis sends.
/// </summary>
public static class LlmExchangeKind
{
    /// <summary>Stories of the current report, read for findings.</summary>
    public const string Map = "Map";
    /// <summary>Findings consolidated.</summary>
    public const string Reduce = "Reduce";
    /// <summary>The section written from the findings.</summary>
    public const string Final = "Final";
    /// <summary>Stories of a previous report, read for findings.</summary>
    public const string HistoryMap = "HistoryMap";
    /// <summary>Findings of a previous report consolidated.</summary>
    public const string HistoryReduce = "HistoryReduce";
    /// <summary>An agent-backed section, which receives only its prompts.</summary>
    public const string Agent = "Agent";
}

/// <summary>
/// LlmExchange record, one request to the model and what came back. A planned exchange, built
/// without sending anything, has no response.
/// </summary>
/// <param name="Stage">The display label of the step, e.g. "Map batch 2/5", "Reduce round 1", "Final".</param>
/// <param name="Kind">One of <see cref="LlmExchangeKind"/>.</param>
/// <param name="Sequence">The send order, from 1.</param>
/// <param name="Messages">The messages sent.</param>
/// <param name="Response">The response text, or null when planned or failed.</param>
/// <param name="PromptTokens">Prompt tokens the provider reported.</param>
/// <param name="CompletionTokens">Completion tokens the provider reported.</param>
/// <param name="DurationMs">How long the request took.</param>
/// <param name="JsonMode">Whether structured JSON output was requested.</param>
/// <param name="IsTruncated">Whether the response stopped at the output limit.</param>
/// <param name="Error">Why the request failed, when it did.</param>
/// <param name="EstimatedTokens">The request's estimated size before it was sent.</param>
public record LlmExchange(
    string Stage,
    string Kind,
    int Sequence,
    IReadOnlyList<LlmMessage> Messages,
    string? Response,
    int? PromptTokens,
    int? CompletionTokens,
    long DurationMs,
    bool JsonMode,
    bool IsTruncated,
    string? Error,
    int EstimatedTokens);

/// <summary>
/// SynthesisTranscript class, records every request a synthesis sends. Requests run concurrently,
/// so each takes its sequence number as it is sent and the exchanges are read back in that order.
/// </summary>
public sealed class SynthesisTranscript
{
    #region Variables
    private readonly ConcurrentQueue<LlmExchange> _exchanges = new();
    private int _sequence;
    #endregion

    #region Properties
    /// <summary>
    /// get/init - The most characters kept of any one message or response; 0 keeps everything.
    /// </summary>
    public int MaxMessageChars { get; init; } = 100_000;

    /// <summary>
    /// get - The exchanges recorded so far, in send order.
    /// </summary>
    public IReadOnlyList<LlmExchange> Exchanges => _exchanges.OrderBy(e => e.Sequence).ToArray();
    #endregion

    #region Methods
    /// <summary>
    /// Take the next sequence number, as a request is about to be sent.
    /// </summary>
    /// <returns></returns>
    public int NextSequence() => Interlocked.Increment(ref _sequence);

    /// <summary>
    /// Record an exchange, cutting any text longer than <see cref="MaxMessageChars"/>.
    /// </summary>
    /// <param name="exchange"></param>
    public void Add(LlmExchange exchange)
    {
        if (this.MaxMessageChars > 0)
        {
            exchange = exchange with
            {
                Messages = exchange.Messages.Select(m => m with { Content = Cap(m.Content, this.MaxMessageChars) }).ToArray(),
                Response = exchange.Response == null ? null : Cap(exchange.Response, this.MaxMessageChars),
            };
        }
        _exchanges.Enqueue(exchange);
    }

    /// <summary>
    /// Cut text to 'maxChars', marking how much was left out.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxChars"></param>
    /// <returns></returns>
    public static string Cap(string text, int maxChars)
        => maxChars <= 0 || text.Length <= maxChars ? text : $"{text[..maxChars]}\n… [{text.Length - maxChars:N0} more characters omitted]";
    #endregion
}
