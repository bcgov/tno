using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.API.Models.Settings;
using TNO.Entities;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Config;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Reports;
using ServicesReport = TNO.API.Areas.Services.Models.Report;

namespace TNO.Test.AI;

/// <summary>
/// Tests that AI section results are generated once per manifest and read only their scope.
/// </summary>
public class ReportAISectionGeneratorTest
{
    #region Helpers
    /// <summary>
    /// A store kept in memory, standing in for the database.
    /// </summary>
    private class MemoryStore : IReportAIResultStore
    {
        private long _id;
        public ConcurrentDictionary<string, ReportAIResultModel> Results { get; } = new();

        public Task<ReportAIResultModel?> FindAsync(string hash, CancellationToken cancellationToken = default)
            => Task.FromResult(this.Results.TryGetValue(hash, out var result) ? result : null);

        public Task<ReportAIResultModel?> TryClaimAsync(ReportAIResultClaimModel claim, CancellationToken cancellationToken = default)
        {
            var result = new ReportAIResultModel() { Id = Interlocked.Increment(ref _id), Hash = claim.Hash, Status = ReportAIResultStatus.Pending };
            if (this.Results.TryAdd(claim.Hash, result)) return Task.FromResult<ReportAIResultModel?>(result);
            var existing = this.Results[claim.Hash];
            if (existing.Status == ReportAIResultStatus.Failed)
            {
                this.Results[claim.Hash] = result;
                return Task.FromResult<ReportAIResultModel?>(result);
            }
            return Task.FromResult<ReportAIResultModel?>(null);
        }

        public Task CompleteAsync(long id, ReportAIResultCompletionModel completion, CancellationToken cancellationToken = default)
        {
            var entry = this.Results.Values.First(r => r.Id == id);
            entry.Status = completion.IsSuccess ? ReportAIResultStatus.Completed : ReportAIResultStatus.Failed;
            entry.Output = completion.Output;
            entry.Error = completion.Error;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A store whose result for every manifest is fixed, and that no generator can claim.
    /// </summary>
    private class FixedStore : IReportAIResultStore
    {
        private readonly ReportAIResultModel _result;
        public FixedStore(ReportAIResultModel result) { _result = result; }
        public Task<ReportAIResultModel?> FindAsync(string hash, CancellationToken cancellationToken = default) => Task.FromResult<ReportAIResultModel?>(_result);
        public Task<ReportAIResultModel?> TryClaimAsync(ReportAIResultClaimModel claim, CancellationToken cancellationToken = default) => Task.FromResult<ReportAIResultModel?>(null);
        public Task CompleteAsync(long id, ReportAIResultCompletionModel completion, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private static ContentModel Story(long id, string body) => new() { Id = id, Headline = $"Headline {id}", Body = body };

    private static ReportSectionModel ContentSection(string name, int sortOrder, params ContentModel[] content) => new()
    {
        Name = name,
        SectionType = ReportSectionType.Content,
        SortOrder = sortOrder,
        Settings = new ReportSectionSettingsModel() { Label = name },
        Content = content,
    };

    private static (ServicesReport.ReportModel Report, ServicesReport.ReportSectionModel Section) Report(ReportSectionSettingsModel aiSettings)
    {
        var section = new ServicesReport.ReportSectionModel() { Id = 9, Name = "ai", SectionType = ReportSectionType.AI, IsEnabled = true, Settings = aiSettings };
        var report = new ServicesReport.ReportModel() { Id = 1, Name = "Report", Sections = new[] { section } };
        return (report, section);
    }

    private static API.Areas.Services.Models.LLM.LLMModel Llm(int? contextWindow = 8000) => new()
    {
        Id = 3,
        Name = "gpt",
        DeploymentName = "gpt",
        ApiKey = "key",
        ProjectEndpoint = new Uri("https://example.test/openai/deployments/gpt/chat/completions"),
        ContextWindow = contextWindow,
        MaxOutputTokens = contextWindow.HasValue ? 1000 : null,
    };

    private static ReportAISectionGenerator Generator(IReportAIResultStore store, FakeLlmClient client)
        => new(store, client, new TemplateOptions(), NullLogger.Instance);
    #endregion

    [Fact]
    public async Task UnchangedManifestIsGeneratedOnce()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news."), Story(2, "Transit news.")) };

        var first = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());
        var requests = client.Requests.Count;
        var second = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());

        first.Error.Should().BeNull();
        second.Output.Should().Be(first.Output);
        client.Requests.Count.Should().Be(requests);
        store.Results.Should().ContainSingle();
    }

    [Fact]
    public async Task ChangedStoryInputRegenerates()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });

        await Generator(store, client).GenerateAsync(report, 5, section,
            new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) }, Array.Empty<PreviousReportModel>(), Llm());
        await Generator(store, client).GenerateAsync(report, 5, section,
            new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news, updated.")) }, Array.Empty<PreviousReportModel>(), Llm());

        store.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SectionScopeReadsOnlyItsSections()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel()
        {
            Label = "Summary",
            UserPrompt = "Summarize",
            AIScope = ReportAISectionGenerator.ScopeSections,
            SourceSections = new[] { "health" },
        });
        var sections = new Dictionary<string, ReportSectionModel>
        {
            ["health"] = ContentSection("health", 0, Story(1, "Health news.")),
            ["transit"] = ContentSection("transit", 1, Story(2, "Transit news.")),
        };

        await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());

        var mapped = client.Requests.Where(r => r[0].Content.Contains("You read a batch")).Select(r => r[^1].Content).ToArray();
        String.Join("\n", mapped).Should().Contain("Headline 1").And.NotContain("Headline 2");
    }

    [Fact]
    public async Task SectionScopeWithoutSectionsIsAnError()
    {
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", AIScope = ReportAISectionGenerator.ScopeSections });

        var result = await Generator(new MemoryStore(), new FakeLlmClient(8000)).GenerateAsync(report, 5, section,
            new Dictionary<string, ReportSectionModel>(), Array.Empty<PreviousReportModel>(), Llm());

        result.Error.Should().Contain("Choose the content sections");
    }

    [Fact]
    public async Task LlmWithoutLimitsIsAConfigurationError()
    {
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary" });

        var result = await Generator(new MemoryStore(), new FakeLlmClient(8000)).GenerateAsync(report, 5, section,
            new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "News.")) }, Array.Empty<PreviousReportModel>(), Llm(null));

        result.Error.Should().Contain("context window");
    }

    [Fact]
    public void AnalyzedStoriesAreReadFromTheirEvidenceAndGroupedByTopic()
    {
        var analyzed = Story(1, "A very long article body that synthesis should not read.");
        analyzed.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel()
        {
            ContentId = 1,
            AnalysisId = 42,
            IsApproved = true,
            Topic = "Health care",
            Summary = "The hospital opens in spring.",
            Facts = new[] { "Construction finished in June." },
            Entities = new[] { "Royal Hospital" },
            Quotes = new[] { new API.Areas.Services.Models.Content.AnalysisQuoteSummaryModel() { Statement = "We are ready", Speaker = "The minister" } },
        };
        var unanalyzed = Story(2, "Transit news.");

        var stories = ReportAISectionGenerator.BuildStories(new[] { ContentSection("news", 0, analyzed, unanalyzed) }, null, false);

        stories[0].Text.Should().Contain("The hospital opens in spring.").And.Contain("Construction finished in June.")
            .And.Contain("Royal Hospital").And.Contain("\"We are ready\" (The minister)")
            .And.NotContain("very long article body");
        stories[0].Group.Should().Be("Health care");
        stories[0].AnalysisId.Should().Be(42);
        stories[1].Text.Should().Be("Transit news.");
        stories[1].Group.Should().BeNull();
    }

    [Fact]
    public void AnUnapprovedTranscriptsAnalysisIsNeverUsed()
    {
        var model = new API.Areas.Services.Models.Content.ContentModel()
        {
            Id = 3,
            Headline = "Radio",
            ContentType = ContentType.AudioVideo,
            IsApproved = false,
            Analysis = new API.Areas.Services.Models.Content.ContentAnalysisSummaryModel() { Id = 7, Summary = "From the transcript." },
        };

        new ContentModel(model).Evidence.Should().BeNull();
        model.ToPublishedDocument().Analysis.Should().BeNull();
        model.ToPublishedDocument().Body.Should().BeEmpty();
    }

    [Fact]
    public async Task NewAnalysisRegenerates()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var story = Story(1, "Budget news.");
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, story) };

        await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());
        story.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel() { ContentId = 1, AnalysisId = 8, IsApproved = true, Summary = "The budget adds $1B." };
        await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());

        store.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task PreviewLeavesAMissingSectionPending()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };

        var result = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.NoWait);

        result.Status.Should().Be(AISectionStatus.NotStarted);
        result.Output.Should().BeNull();
        client.Requests.Should().BeEmpty("a preview never generates");
        store.Results.Should().BeEmpty("a preview never claims");
    }

    [Fact]
    public async Task PreviewShowsAStoredResult()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };

        var prepared = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.Prepare);
        var requests = client.Requests.Count;
        var preview = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.NoWait);

        prepared.Status.Should().Be(AISectionStatus.Ready);
        preview.Status.Should().Be(AISectionStatus.Ready);
        preview.Output.Should().Be(prepared.Output);
        client.Requests.Count.Should().Be(requests);
    }

    [Fact]
    public async Task AnotherGeneratorsWorkIsNotWaitedFor()
    {
        var claimExpiresOn = DateTime.UtcNow.AddMinutes(5);
        var store = new FixedStore(new ReportAIResultModel() { Id = 1, Status = ReportAIResultStatus.Pending, ClaimExpiresOn = claimExpiresOn });
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };

        var preview = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.NoWait);
        var prepare = await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.Prepare);

        preview.Status.Should().Be(AISectionStatus.Generating);
        preview.ClaimExpiresOn.Should().Be(claimExpiresOn, "the preview checks again when the claim lapses");
        prepare.Status.Should().Be(AISectionStatus.Generating);
        client.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task PreviewShowsAStoredFailureRatherThanQueueingItAgain()
    {
        var store = new FixedStore(new ReportAIResultModel() { Id = 1, Status = ReportAIResultStatus.Failed, Error = "The model refused." });
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };

        var preview = await Generator(store, new FakeLlmClient(8000)).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.NoWait);

        preview.Status.Should().Be(AISectionStatus.Failed);
        preview.Error.Should().Be("The model refused.");
    }
}
