using System.Collections.Concurrent;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.AI;
using TNO.AI.Analysis;
using TNO.AI.Tokens;

namespace TNO.Test.AI;

/// <summary>
/// Behavioural tests of content analysis against a scripted model: chunking without truncation,
/// verbatim quote validation, entity merging, tag validation, and metadata-only content.
/// </summary>
public class ContentAnalyzerTest
{
    #region Helpers
    /// <summary>
    /// A model whose extraction and summary responses come from the test. It rejects requests
    /// larger than its context window the way a provider does.
    /// </summary>
    private class ScriptedLlmClient : ILlmClient
    {
        private readonly ITokenEstimator _estimator = new HeuristicTokenEstimator();
        public int ContextWindow { get; set; } = 100_000;
        public Func<string, object> Extract { get; set; } = _ => new { };
        public Func<string, object> Summarize { get; set; } = _ => new { summary = "A summary.", primaryTopic = "Topic", tags = Array.Empty<string>() };
        public ConcurrentQueue<(string System, string User)> Requests { get; } = new();

        public Task<LlmResult> InvokeAsync(LlmEndpoint llm, IReadOnlyList<(string Role, string Content)> messages, bool jsonMode = false, int attempts = 3, LlmRequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var system = messages[0].Content;
            var user = messages[^1].Content;
            this.Requests.Enqueue((system, user));
            var size = messages.Sum(m => _estimator.Count(m.Content)) + LlmLimits.RequestOverheadTokens + LlmLimits.MessageOverheadTokens * messages.Count;
            if (size + (options?.MaxOutputTokens ?? 0) > this.ContextWindow)
                throw new LlmContextLengthException("This model's maximum context length was exceeded.");

            object response = system == AnalysisPrompts.Extract ? this.Extract(user)
                : system == AnalysisPrompts.Summarize ? this.Summarize(user)
                : new { facts = Array.Empty<string>() };
            return Task.FromResult(new LlmResult(JsonSerializer.Serialize(response), size, 50, 1) { FinishReason = "stop" });
        }
    }

    private static readonly LlmEndpoint Endpoint = new(new Uri("https://example.test/openai/deployments/gpt/chat/completions"), "key", "gpt");

    private static AnalyzerInput Input(string body, string byline = "") => new(1, "Hospital to open", body, byline, "", "Daily News", "Print", "", new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc));

    private static Task<AnalyzerResult> AnalyzeAsync(ScriptedLlmClient client, string body, int contextWindow = 100_000, IReadOnlyList<AnalyzerTag>? tags = null, string byline = "", AnalyzerOptions? options = null)
        => new ContentAnalyzer(client, options ?? new AnalyzerOptions(), NullLogger.Instance)
            .AnalyzeAsync(Input(body, byline), Endpoint, new LlmLimits(contextWindow, 500, "heuristic", null, null), tags ?? Array.Empty<AnalyzerTag>());

    private const string Story = "<p>The minister said, “We are ready to open the hospital in the spring.”</p><p>Construction finished in June after two years of work.</p>";
    #endregion

    [Fact]
    public async Task ContentWithoutTextRecordsOnlyMetadata()
    {
        var client = new ScriptedLlmClient();

        var result = await AnalyzeAsync(client, "<p>Photo.</p>", byline: "By Jane Doe");

        result.IsMetadataOnly.Should().BeTrue();
        result.SuggestedContributor.Should().Be("Jane Doe");
        client.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task OnlyVerbatimQuotesAreKept()
    {
        var client = new ScriptedLlmClient()
        {
            Extract = _ => new
            {
                facts = new[] { new { statement = "Construction finished in June.", evidence = "Construction finished in June", inferred = false } },
                quotes = new[]
                {
                    // Straight quote marks and extra whitespace still match the text.
                    new { text = "We are ready to open  the hospital in the spring.", speaker = "The minister" },
                    new { text = "The hospital will cost nothing.", speaker = "The minister" },
                },
            },
        };

        var result = await AnalyzeAsync(client, Story);

        var text = ContentAnalyzer.NormalizeText(Story);
        result.Quotes.Should().ContainSingle();
        var quote = result.Quotes[0];
        text.Substring(quote.Span.Start, quote.Span.Length).Should().Be(quote.Statement);
        quote.Statement.Should().Contain("We are ready to open the hospital");
        result.Validation["droppedQuotes"].Should().Be(1);
        result.KeyFacts.Should().ContainSingle(f => f.Span != null);
    }

    [Fact]
    public async Task AmbiguousEntitiesAreNotMerged()
    {
        var client = new ScriptedLlmClient()
        {
            Extract = _ => new
            {
                facts = new[] { new { statement = "The hospital opens in spring.", evidence = "", inferred = true } },
                entities = new object[]
                {
                    new { type = "person", name = "Adrian Dix", aliases = new[] { "Dix" }, roles = new[] { "Health minister" }, ambiguous = false },
                    new { type = "person", name = "Dix", aliases = Array.Empty<string>(), roles = new[] { "Minister" }, ambiguous = false },
                    new { type = "person", name = "Smith", aliases = Array.Empty<string>(), roles = Array.Empty<string>(), ambiguous = true },
                    new { type = "person", name = "Smith", aliases = Array.Empty<string>(), roles = Array.Empty<string>(), ambiguous = true },
                },
            },
        };

        var result = await AnalyzeAsync(client, Story);

        result.Entities.Where(e => e.Name == "Adrian Dix").Should().ContainSingle()
            .Which.Roles.Should().Contain(new[] { "Health minister", "Minister" });
        result.Entities.Count(e => e.Name == "Smith").Should().Be(2);
    }

    [Fact]
    public async Task LongTextIsChunkedAndEveryPartIsRead()
    {
        var paragraphs = Enumerable.Range(1, 60).Select(i => $"<p>Paragraph {i} reports that the council approved item {i} after a long debate about the budget.</p>");
        var body = String.Concat(paragraphs);
        var client = new ScriptedLlmClient()
        {
            ContextWindow = 1500,
            Extract = user => new { facts = new[] { new { statement = $"Fact from part {user.Length}.", evidence = "", inferred = true } } },
        };

        var result = await AnalyzeAsync(client, body, contextWindow: 1500);

        var extracts = client.Requests.Where(r => r.System == AnalysisPrompts.Extract).Select(r => r.User).ToArray();
        extracts.Length.Should().BeGreaterThan(1);
        foreach (var i in Enumerable.Range(1, 60))
            extracts.Should().Contain(u => u.Contains($"Paragraph {i} reports"), $"paragraph {i} must be read");
        ((int)result.Validation["chunks"]!).Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task OnlyListedTagsAreSuggested()
    {
        var client = new ScriptedLlmClient()
        {
            Extract = _ => new { facts = new[] { new { statement = "The hospital opens in spring.", evidence = "", inferred = true } } },
            Summarize = _ => new { summary = "The hospital opens in spring.", primaryTopic = "Health care", tags = new[] { "HLTH", "MADEUP" } },
        };

        var result = await AnalyzeAsync(client, Story, tags: new[] { new AnalyzerTag("HLTH", "Health") });

        result.SuggestedTags.Should().Equal("HLTH");
        result.PrimaryTopic.Should().Be("Health care");
        result.Summary.Should().Be("The hospital opens in spring.");
    }

    [Fact]
    public async Task MissingEventDetailsStayEmpty()
    {
        var client = new ScriptedLlmClient()
        {
            Extract = _ => new
            {
                facts = new[] { new { statement = "The hospital opens in spring.", evidence = "", inferred = true } },
                events = new[] { new { actor = "The province", action = "opens the hospital", date = "", location = " " } },
            },
        };

        var result = await AnalyzeAsync(client, Story);

        var e = result.Events.Should().ContainSingle().Subject;
        e.Date.Should().BeNull();
        e.Location.Should().BeNull();
    }

    [Fact]
    public async Task OnlyConfiguredProcessesRun()
    {
        var client = new ScriptedLlmClient()
        {
            Extract = _ => new
            {
                facts = new[] { new { statement = "Construction finished in June.", evidence = "Construction finished in June", inferred = false } },
                entities = new[] { new { type = "person", name = "Adrian Dix", aliases = Array.Empty<string>(), roles = Array.Empty<string>(), ambiguous = false } },
                quotes = new[] { new { text = "We are ready to open the hospital in the spring.", speaker = "The minister" } },
            },
        };
        var quotesOnly = new AnalyzerOptions() { ExtractMetadata = false, Summarize = false, SuggestTags = false, ChooseTopic = false, SuggestContributor = false };

        var result = await AnalyzeAsync(client, Story, byline: "By Jane Doe", options: quotesOnly);

        result.Quotes.Should().ContainSingle();
        result.KeyFacts.Should().BeEmpty();
        result.Entities.Should().BeEmpty();
        result.Summary.Should().BeEmpty();
        result.PrimaryTopic.Should().BeNull();
        result.SuggestedContributor.Should().BeNull();
        client.Requests.Should().NotContain(r => r.System == AnalysisPrompts.Summarize, "no configured process needs a summary");
    }

    [Fact]
    public async Task ContributorOnlyNeedsNoModel()
    {
        var client = new ScriptedLlmClient();
        var contributorOnly = new AnalyzerOptions() { ExtractMetadata = false, Summarize = false, ExtractQuotes = false, SuggestTags = false, ChooseTopic = false };

        var result = await AnalyzeAsync(client, Story, byline: "By Jane Doe", options: contributorOnly);

        result.SuggestedContributor.Should().Be("Jane Doe");
        client.Requests.Should().BeEmpty();
    }
}
