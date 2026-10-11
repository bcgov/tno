using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.AI;
using TNO.AI.Synthesis;
using TNO.AI.Tokens;

namespace TNO.Test.AI;

/// <summary>
/// Tests that a synthesis records every request it sends, and that the planner describes the same
/// requests without sending any.
/// </summary>
public class ReportSynthesizerTranscriptTest
{
    #region Helpers
    private static readonly LlmEndpoint Endpoint = new(new Uri("https://example.test/openai/deployments/x/chat/completions"), "key", "x");

    private static LlmLimits Limits(int contextWindow = 8000, int maxOutput = 1000) => new(contextWindow, maxOutput, TokenEstimationStrategy.Heuristic, null, null);

    private static List<SynthesisStory> Stories(int count, int words = 40)
        => Enumerable.Range(1, count)
            .Select(i => new SynthesisStory(i, $"Headline {i}", "section: News | Source", String.Join(" ", Enumerable.Repeat($"word{i}", words)), $"https://mmi.test/view/{i}"))
            .ToList();

    private static SynthesisRequest Request(IReadOnlyList<SynthesisStory> stories, LlmLimits? limits = null, IReadOnlyList<SynthesisHistoricalInstance>? history = null)
        => new("Summary", null, "Summarize the key issues as HTML.", SynthesisOutputMode.FreeText, stories, history ?? Array.Empty<SynthesisHistoricalInstance>(), Endpoint, limits ?? Limits());

    private static ReportSynthesizer Synthesizer(FakeLlmClient client, int maxBatchInputTokens = 0)
        => new(client, new SynthesisOptions() { MaxConcurrentRequests = 4, MaxBatchInputTokens = maxBatchInputTokens }, NullLogger.Instance);

    private static bool SameMessages(IReadOnlyList<(string Role, string Content)> sent, IReadOnlyList<LlmMessage> recorded)
        => sent.Count == recorded.Count && sent.Zip(recorded).All(p => p.First.Role == p.Second.Role && p.First.Content == p.Second.Content);
    #endregion

    #region Recording
    [Fact]
    public async Task TranscriptRecordsEveryRequestInSendOrder()
    {
        var client = new FakeLlmClient(8000);
        var transcript = new SynthesisTranscript();

        var result = await Synthesizer(client, 2000).SynthesizeAsync(Request(Stories(60)), transcript);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Transcript.Should().NotBeNull();
        result.Transcript!.Count.Should().Be(client.Requests.Count).And.Be(result.Usage.Requests);
        result.Transcript.Select(e => e.Sequence).Should().Equal(Enumerable.Range(1, result.Transcript.Count));
        result.Transcript.Should().Contain(e => e.Stage == "Map batch 1/" + result.Transcript.Count(x => x.Kind == LlmExchangeKind.Map) && e.Kind == LlmExchangeKind.Map && e.JsonMode);
        result.Transcript.Count(e => e.Kind == LlmExchangeKind.Map).Should().BeGreaterThan(1);
        var final = result.Transcript.Last();
        final.Stage.Should().Be("Final");
        final.Kind.Should().Be(LlmExchangeKind.Final);
        final.JsonMode.Should().BeFalse();
        final.Response.Should().Contain("<p>Summary");
        result.Transcript.Should().OnlyContain(e => e.Response != null && e.Error == null && !e.IsTruncated);
        // The provider's prompt count is what the fake reports, which is the estimate it was sent.
        result.Transcript.Should().OnlyContain(e => e.PromptTokens == e.EstimatedTokens && e.CompletionTokens > 0);
        foreach (var exchange in result.Transcript)
            client.Requests.Should().Contain(sent => SameMessages(sent, exchange.Messages));
    }

    [Fact]
    public async Task TranscriptLabelsReduceRounds()
    {
        var limits = Limits(4000, 500);
        var client = new FakeLlmClient(limits.ContextWindow);
        var transcript = new SynthesisTranscript();

        var result = await Synthesizer(client, 1500).SynthesizeAsync(Request(Stories(300), limits), transcript);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Usage.ReductionDepth.Should().BeGreaterThan(0);
        // Findings are reduced either before the final step or, when only the final request is too full, within it.
        result.Transcript.Should().Contain(e => e.Kind == LlmExchangeKind.Reduce && (e.Stage.StartsWith("Reduce round 1") || e.Stage.StartsWith("Final reduce round 1")));
    }

    [Fact]
    public async Task TranscriptIsNullWhenNotRequested()
    {
        var client = new FakeLlmClient(8000);

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(5)));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Transcript.Should().BeNull();
    }

    [Fact]
    public async Task TranscriptRecordsRejectionsAndSplits()
    {
        // The fake's window is smaller than the configured one, so some map requests are rejected and split.
        var limits = Limits(8000, 1000);
        var client = new FakeLlmClient(5000);
        var transcript = new SynthesisTranscript();

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(200), limits), transcript);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Transcript!.Count.Should().Be(client.Requests.Count);
        result.Transcript.Should().Contain(e => e.Error != null && e.Response == null && e.Error.Contains("context"));
        result.Transcript.Should().Contain(e => e.Stage.Contains("(split 1/2)") && e.Response != null);
    }

    [Fact]
    public async Task TranscriptLabelsHistoryStages()
    {
        var client = new FakeLlmClient(8000);
        var transcript = new SynthesisTranscript();
        var history = new[] { new SynthesisHistoricalInstance("Report of 2026-09-28", Stories(3)) };

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(2), history: history), transcript);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Transcript.Should().Contain(e => e.Kind == LlmExchangeKind.HistoryMap && e.Stage == "Previous report 'Report of 2026-09-28' map batch 1/1");
        result.Transcript!.Last().Messages.Should().HaveCount(3);
        result.Transcript.Last().Messages[1].Content.Should().StartWith("## Report of 2026-09-28 (previous report)");
    }

    [Fact]
    public async Task TranscriptCapsLongText()
    {
        var client = new FakeLlmClient(8000);
        var transcript = new SynthesisTranscript() { MaxMessageChars = 200 };

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(20)), transcript);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Transcript.Should().Contain(e => e.Messages.Any(m => m.Content.Contains("more characters omitted")));
        result.Transcript.Should().OnlyContain(e => e.Messages.All(m => m.Content.Length < 260));
    }
    #endregion

    #region Planning
    [Fact]
    public async Task PlanIsDeterministicAndMatchesTheMapRequestsSent()
    {
        var client = new FakeLlmClient(8000);
        var synthesizer = Synthesizer(client, 2000);
        var request = Request(Stories(60));

        var first = synthesizer.Plan(request);
        var second = synthesizer.Plan(request);

        client.Requests.Should().BeEmpty();
        JsonSerializer.Serialize(first).Should().Be(JsonSerializer.Serialize(second));
        first.Select(e => e.Sequence).Should().Equal(Enumerable.Range(1, first.Count));
        first.Should().OnlyContain(e => e.Response == null && e.EstimatedTokens > 0);
        first.Should().Contain(e => e.Stage == "Reduce round 1" && e.Kind == LlmExchangeKind.Reduce);
        var final = first.Last();
        final.Stage.Should().Be("Final");
        final.Messages[0].Content.Should().Be("You write the 'Summary' section of a media monitoring report.");
        final.Messages[^1].Content.Should().StartWith(SynthesisPrompts.FinalCitationRule)
            .And.Contain("{coverage by topic")
            .And.EndWith("Summarize the key issues as HTML.");

        var transcript = new SynthesisTranscript();
        await synthesizer.SynthesizeAsync(request, transcript);
        var planned = first.Where(e => e.Kind == LlmExchangeKind.Map).ToDictionary(e => e.Stage, e => e.Messages);
        var actual = transcript.Exchanges.Where(e => e.Kind == LlmExchangeKind.Map).ToDictionary(e => e.Stage, e => e.Messages);
        actual.Keys.Should().BeEquivalentTo(planned.Keys);
        foreach (var (stage, messages) in planned)
            actual[stage].Should().BeEquivalentTo(messages, options => options.WithStrictOrdering());
    }

    [Fact]
    public void PlanWithHistoryAddsAUserTurnPerPreviousReport()
    {
        var client = new FakeLlmClient(8000);
        var history = new[]
        {
            new SynthesisHistoricalInstance("Report of 2026-09-28", Stories(3)),
            new SynthesisHistoricalInstance("Report of 2026-09-29", Stories(2)),
        };

        var plan = Synthesizer(client).Plan(Request(Stories(2), history: history));

        plan.Should().Contain(e => e.Kind == LlmExchangeKind.HistoryMap && e.Stage.Contains("'Report of 2026-09-28' map batch 1/1"));
        plan.Should().Contain(e => e.Kind == LlmExchangeKind.HistoryReduce && e.Stage.Contains("'Report of 2026-09-29' reduce round 1"));
        plan.Last().Messages.Should().HaveCount(4);
        plan.Last().Messages[1].Content.Should().StartWith("## Report of 2026-09-28 (previous report)");
    }

    [Fact]
    public void PlanRejectsInvalidLimits()
    {
        var synthesizer = Synthesizer(new FakeLlmClient(8000));

        var act = () => synthesizer.Plan(Request(Stories(2), new LlmLimits(0, 0, null, null, null)));

        act.Should().Throw<SynthesisException>().WithMessage("*context window*");
    }
    #endregion
}
