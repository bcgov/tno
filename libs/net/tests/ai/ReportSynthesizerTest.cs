using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.AI;
using TNO.AI.Synthesis;
using TNO.AI.Tokens;

namespace TNO.Test.AI;

/// <summary>
/// Tests for bounded report synthesis.
/// </summary>
public class ReportSynthesizerTest
{
    #region Helpers
    private static readonly LlmEndpoint Endpoint = new(new Uri("https://example.test/openai/deployments/x/chat/completions"), "key", "x");

    private static LlmLimits Limits(int contextWindow = 8000, int maxOutput = 1000) => new(contextWindow, maxOutput, TokenEstimationStrategy.Heuristic, null, null);

    private static List<SynthesisStory> Stories(int count, int words = 40, string? group = null)
        => Enumerable.Range(1, count)
            .Select(i => new SynthesisStory(i, $"Headline {i}", "Source | 2026-09-30", String.Join(" ", Enumerable.Repeat($"word{i}", words)), $"https://mmi.test/view/{i}", null, group))
            .ToList();

    private static SynthesisRequest Request(IReadOnlyList<SynthesisStory> stories, SynthesisOutputMode mode = SynthesisOutputMode.FreeText, LlmLimits? limits = null, IReadOnlyList<SynthesisHistoricalInstance>? history = null, int? choiceIndex = null, int? choiceCount = null)
        => new("Summary", "You summarize news.", "Summarize the key issues as HTML.", mode, stories, history ?? Array.Empty<SynthesisHistoricalInstance>(), Endpoint, limits ?? Limits(), null, choiceCount, choiceIndex);

    private static ReportSynthesizer Synthesizer(FakeLlmClient client, int maxDepth = 8)
        => new(client, new SynthesisOptions() { MaxReductionDepth = maxDepth, MaxConcurrentRequests = 4 }, NullLogger.Instance);
    #endregion

    #region Coverage and budgets
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task EveryStoryIsProcessedAndEveryRequestFits(int count)
    {
        var limits = Limits();
        var client = new FakeLlmClient(limits.ContextWindow);

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(count), limits: limits));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Usage.StoriesProcessed.Should().Be(count);
        client.MapHandles.SelectMany(h => h).Distinct().Should().HaveCount(count);
        client.RequestSizes.Max().Should().BeLessThanOrEqualTo(limits.ContextWindow - limits.MaxOutputTokens);
        result.Usage.LargestRequestTokens.Should().BeLessThanOrEqualTo(limits.ContextWindow - limits.MaxOutputTokens);
    }

    [Fact]
    public async Task LongStoriesAreChunkedNotTruncated()
    {
        var limits = Limits(4000, 500);
        var client = new FakeLlmClient(limits.ContextWindow);
        var words = Enumerable.Range(1, 6000).Select(i => $"w{i}").ToArray();
        var story = new SynthesisStory(1, "A very long transcript", "Radio | 2026-09-30", String.Join(" ", words) + ".");

        var result = await Synthesizer(client).SynthesizeAsync(Request(new[] { story }, limits: limits));

        result.IsSuccess.Should().BeTrue(result.Error);
        var mapInputs = client.Requests.Where(r => r[0].Content.Contains("You read a batch")).Select(r => r[^1].Content).ToArray();
        mapInputs.Length.Should().BeGreaterThan(1);
        var sent = String.Join(" ", mapInputs);
        words.Should().OnlyContain(w => sent.Contains(w + " ") || sent.Contains(w + "."));
    }
    #endregion

    #region Splitting and failure
    [Fact]
    public async Task ContextRejectionSplitsAndRetries()
    {
        // The fake's window is smaller than the configured one, like an estimator that under-counts.
        var limits = Limits(8000, 1000);
        var client = new FakeLlmClient(5000);

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(200), limits: limits));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Usage.StoriesProcessed.Should().Be(200);
    }

    [Fact]
    public async Task TruncatedOutputSplitsAndRetries()
    {
        var client = new FakeLlmClient(8000) { TruncateWhenStoriesExceed = 5 };

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(60)));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Usage.StoriesProcessed.Should().Be(60);
        // The first attempt was too large for the output and was split until each part fit.
        client.MapHandles.Should().Contain(h => h.Length > 5);
        client.MapHandles.Where(h => h.Length <= 5).SelectMany(h => h).Distinct().Should().HaveCount(60);
    }

    [Fact]
    public async Task DepthLimitFailureIsReported()
    {
        var client = new FakeLlmClient(8000) { ReduceNeverShrinks = true };

        var result = await Synthesizer(client, 2).SynthesizeAsync(Request(Stories(2000)));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("rounds");
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public async Task MissingLimitsIsAConfigurationError()
    {
        var client = new FakeLlmClient(8000);

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(3), limits: new LlmLimits(0, 0, null, null, null)));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("context window");
        client.Requests.Should().BeEmpty();
    }
    #endregion

    #region Output
    [Fact]
    public async Task FreeTextCitationsBecomeLinksAndHtmlIsSanitized()
    {
        var client = new FakeLlmClient(8000) { FinalOutput = "<script>alert(1)</script><p onclick=\"x()\">Budget [S2] and [S999]</p>" };

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(3)));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Output.Should().NotContain("<script");
        result.Output.Should().NotContain("onclick");
        result.Output.Should().Contain("https://mmi.test/view/2");
        result.Output.Should().NotContain("S999");
    }

    [Fact]
    public async Task TopicSummaryIsAssembledFromProvenance()
    {
        var client = new FakeLlmClient(8000);
        var stories = Stories(4);
        stories[0] = stories[0] with { Headline = "<b>Bold</b> & co" };

        var result = await Synthesizer(client).SynthesizeAsync(Request(stories, SynthesisOutputMode.TopicSummary));

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Output.Should().Contain("<h3>");
        result.Output.Should().Contain("href=\"https://mmi.test/view/1\"");
        result.Output.Should().Contain("&lt;b&gt;Bold&lt;/b&gt; &amp; co");
        // Topic summaries are written in application code, not by a final free-text request.
        client.Requests.Should().OnlyContain(r => r[0].Content.Contains("You read a batch") || r[0].Content.Contains("You consolidate"));
    }

    [Fact]
    public async Task GroupsBecomeHeadings()
    {
        var client = new FakeLlmClient(8000);
        var stories = Stories(2, group: "Health").Concat(Stories(2, group: "Transit").Select(s => s with { ContentId = s.ContentId + 10 })).ToList();

        var result = await Synthesizer(client).SynthesizeAsync(Request(stories, SynthesisOutputMode.TopicSummary));

        result.Output.Should().Contain("<h3>Health</h3>");
        result.Output.Should().Contain("<h3>Transit</h3>");
    }

    [Fact]
    public async Task AllChoicesAreJoinedWhenChoiceIndexIsMinusOne()
    {
        var client = new FakeLlmClient(8000) { FinalOutput = "<p>text</p>", FinalChoices = 2 };

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(2), choiceIndex: -1, choiceCount: 2));

        result.Output.Should().Contain("Choice 1");
        result.Output.Should().Contain("Choice 2");
    }
    #endregion

    #region History
    [Fact]
    public async Task HistoricalInstancesAreProcessedSeparatelyInOrder()
    {
        var client = new FakeLlmClient(8000);
        var history = new[]
        {
            new SynthesisHistoricalInstance("Report of 2026-09-28", Stories(3)),
            new SynthesisHistoricalInstance("Report of 2026-09-29", Stories(2)),
        };

        var result = await Synthesizer(client).SynthesizeAsync(Request(Stories(2), history: history));

        result.IsSuccess.Should().BeTrue(result.Error);
        var maps = client.MapHandles.ToArray();
        maps[0].Should().OnlyContain(h => h.StartsWith("P1-"));
        maps[1].Should().OnlyContain(h => h.StartsWith("P2-"));
        maps[2].Should().OnlyContain(h => h.StartsWith("S"));
        var final = client.Requests.Last();
        final.Select(m => m.Content).Should().Contain(c => c.Contains("Report of 2026-09-28"));
        // Historical stories are context only; they are never cited in the output.
        result.Output.Should().NotContain("P1-");
    }
    #endregion
}
