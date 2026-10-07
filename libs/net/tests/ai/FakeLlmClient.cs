using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using TNO.AI;
using TNO.AI.Tokens;

namespace TNO.Test.AI;

/// <summary>
/// FakeLlmClient class, a deterministic stand-in for a model deployment. Map requests return one
/// finding per story handle; reduce requests merge pairs of findings (halving them); the final
/// request cites the first handles it was given. It enforces a context window, so a request that
/// would not fit is rejected the way a provider rejects it.
/// </summary>
public partial class FakeLlmClient : ILlmClient
{
    [GeneratedRegex(@"^\[(?<handle>[SP][\d\-]+)\]", RegexOptions.Multiline)]
    private static partial Regex StoryHandleRegex();

    [GeneratedRegex(@"\[(?<handle>S\d+)\]")]
    private static partial Regex QuotedHandleRegex();

    [GeneratedRegex(@"^\[(?<handle>F\d+)\] \((?<topic>[^)]*)\) (?<statement>.*)$", RegexOptions.Multiline)]
    private static partial Regex ReduceLineRegex();

    private readonly ITokenEstimator _estimator = new HeuristicTokenEstimator();

    /// <summary>The context window the fake enforces (tokens, estimated).</summary>
    public int ContextWindow { get; set; }

    /// <summary>A map request carrying more stories than this returns truncated output.</summary>
    public int? TruncateWhenStoriesExceed { get; set; }

    /// <summary>Reduce requests return their input unchanged, so reduction never makes progress.</summary>
    public bool ReduceNeverShrinks { get; set; }

    /// <summary>The text the final request returns; null cites the handles given.</summary>
    public string? FinalOutput { get; set; }

    /// <summary>Choices returned by the final request.</summary>
    public int FinalChoices { get; set; } = 1;

    /// <summary>Every request, in the order sent.</summary>
    public ConcurrentQueue<IReadOnlyList<(string Role, string Content)>> Requests { get; } = new();

    /// <summary>Estimated size of every request.</summary>
    public ConcurrentBag<int> RequestSizes { get; } = new();

    /// <summary>Story handles each map request carried, in order.</summary>
    public ConcurrentQueue<string[]> MapHandles { get; } = new();

    public FakeLlmClient(int contextWindow)
    {
        this.ContextWindow = contextWindow;
    }

    public Task<LlmResult> InvokeAsync(
        LlmEndpoint llm,
        IReadOnlyList<(string Role, string Content)> messages,
        bool jsonMode = false,
        int attempts = 3,
        LlmRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var size = messages.Sum(m => _estimator.Count(m.Content)) + LlmLimits.RequestOverheadTokens + LlmLimits.MessageOverheadTokens * messages.Count;
        this.Requests.Enqueue(messages);
        this.RequestSizes.Add(size);
        if (size + (options?.MaxOutputTokens ?? 0) > this.ContextWindow)
            throw new LlmContextLengthException("This model's maximum context length was exceeded.");

        var system = messages[0].Content;
        var input = messages[^1].Content;

        if (system.Contains("You read a batch of news stories"))
        {
            var handles = StoryHandleRegex().Matches(input).Select(m => m.Groups["handle"].Value).Distinct().ToArray();
            this.MapHandles.Enqueue(handles);
            if (this.TruncateWhenStoriesExceed.HasValue && handles.Length > this.TruncateWhenStoriesExceed)
                return Task.FromResult(new LlmResult("{\"findings\":[", size, 10, 1) { FinishReason = "length" });
            var findings = handles.Select(h => new { topic = h.StartsWith("P") ? "History" : "Topic " + (int.Parse(h[1..].Split('-')[0]) % 3), statement = $"Fact from {h}.", sources = new[] { h } });
            return Json(new { findings }, size);
        }

        if (system.Contains("You consolidate findings"))
        {
            var lines = ReduceLineRegex().Matches(input).Select(m => (Handle: m.Groups["handle"].Value, Topic: m.Groups["topic"].Value, Statement: m.Groups["statement"].Value)).ToArray();
            var groups = this.ReduceNeverShrinks ? lines.Select(l => new[] { l }) : lines.Chunk(2);
            var merged = groups.Select(pair => new { topic = pair[0].Topic, statement = pair[0].Statement, sources = pair.Select(p => p.Handle).ToArray() });
            return Json(new { findings = merged }, size);
        }

        // The final free-text request.
        var cited = QuotedHandleRegex().Matches(input).Select(m => m.Groups["handle"].Value).Distinct().Take(3).ToArray();
        var text = this.FinalOutput ?? $"<p>Summary {String.Join("", cited.Select(h => $"[{h}]"))}</p>";
        var choices = Enumerable.Range(1, this.FinalChoices).Select(i => this.FinalChoices == 1 ? text : $"{text} choice {i}").ToArray();
        return Task.FromResult(new LlmResult(choices[0], size, 50, 1) { FinishReason = "stop", Choices = choices });
    }

    private static Task<LlmResult> Json(object value, int size)
        => Task.FromResult(new LlmResult(JsonSerializer.Serialize(value), size, 20, 1) { FinishReason = "stop", Choices = new[] { JsonSerializer.Serialize(value) } });
}
