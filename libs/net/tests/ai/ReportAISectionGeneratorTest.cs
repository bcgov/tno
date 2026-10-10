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
        IsEnabled = true,
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
    public async Task PreviewReusesWorkerResultWhenHistoryArrivesInDifferentOrder()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel()
        {
            Label = "Summary", UserPrompt = "Summarize", IncludePreviousReports = 1,
        });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(10, "Today's news.")) };
        var first = Story(1, "Earlier budget news.");
        first.SortOrder = 2;
        var second = Story(2, "Earlier transit news.");
        second.SortOrder = 1;
        var third = Story(3, "Earlier health news.");
        third.SortOrder = 1;
        var date = DateTime.UtcNow.Date.AddDays(-1);
        var workerHistory = new[] { new PreviousReportModel(4, date,
            new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, second, third, first) }) };
        var previewHistory = new[] { new PreviousReportModel(4, date,
            new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, first, third, second) }) };

        var generated = await Generator(store, client).GenerateAsync(report, 5, section, sections, workerHistory, Llm());
        var requests = client.Requests.Count;
        var preview = await Generator(store, client).GenerateAsync(report, 5, section, sections, previewHistory, Llm(), AISectionWait.NoWait);

        generated.Status.Should().Be(AISectionStatus.Ready);
        preview.Status.Should().Be(AISectionStatus.Ready);
        preview.Output.Should().Be(generated.Output);
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
    public void OnlySelectedMetadataFieldsAreGivenToTheFinalStep()
    {
        var story = Story(1, "News.");
        story.OtherSource = "Globe and Mail";
        story.Byline = "Jane Doe";
        var sections = new[] { ContentSection("news", 0, story) };

        var selected = ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "headline", "source" }).Single();
        var defaults = ReportAISectionGenerator.BuildStories(sections, null, false).Single();

        selected.Fields.Should().BeEquivalentTo(new Dictionary<string, string> { ["source"] = "Globe and Mail" });
        selected.Metadata.Should().Be("source: Globe and Mail");
        // Empty fields are left out rather than sent blank.
        defaults.Fields.Should().BeEquivalentTo(new Dictionary<string, string> { ["source"] = "Globe and Mail", ["byline"] = "Jane Doe" });
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
    public void SelectedFieldsExcludeUnrequestedStoryData()
    {
        var story = Story(1, "Unselected body");
        story.Byline = "Unselected byline";
        story.OtherSource = "Selected source";
        story.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel()
        {
            AnalysisId = 42,
            Summary = "Selected summary",
            Facts = new[] { "Unselected fact" },
            Entities = new[] { "Unselected entity" },
            Quotes = new[] { new API.Areas.Services.Models.Content.AnalysisQuoteSummaryModel() { Statement = "Unselected quote" } },
        };
        var result = ReportAISectionGenerator.BuildStories(new[] { ContentSection("news", 0, story) },
            new Uri("https://mmi.test/view/"), false, new[] { "summary", "source" }).Single();

        result.Headline.Should().BeEmpty();
        result.Metadata.Should().Be("source: Selected source");
        result.Text.Should().Be("Selected summary");
        result.Url.Should().Be("https://mmi.test/view/1");
        result.Anchor.Should().BeNull();
    }

    [Fact]
    public void ArticleTextIsOptInAndTakesPriorityOverSummary()
    {
        var story = Story(1, "Full body");
        story.Summary = "Short summary";
        var sections = new[] { ContentSection("news", 0, story) };
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "headline" }).Single().Text.Should().BeEmpty();
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "summary" }).Single().Text.Should().Be("Short summary");
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "body" }).Single().Text.Should().Be("Full body");
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "summary", "body" }).Single().Text.Should().Be("Full body");
        ReportAISectionGenerator.DefaultInputFields.Should().NotContain("body");
        ReportAISectionGenerator.BuildStories(sections, null, false).Single().Text.Should().Be("Short summary");
        story.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel() { Summary = "Analysis summary", Facts = new[] { "A fact" } };
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "body", "keyFacts" }).Single().Text.Should().Contain("A fact").And.Contain("Full body").And.NotContain("Analysis summary");
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "summary", "body" }).Single().Text.Should().Be("Full body");
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "headline" }).Single().Text.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("<p> </p>")]
    public void EmptyArticleTextUsesSelectedSummary(string body)
    {
        var story = Story(1, body);
        story.Summary = "Story summary";
        var sections = new[] { ContentSection("news", 0, story) };
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "summary", "body" }).Single().Text.Should().Be("Story summary");
        story.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel() { Summary = "Analysis summary" };
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "summary", "body" }).Single().Text.Should().Be("Analysis summary");
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "body" }).Single().Text.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingSummariesUseArticleTextEvenWhenUnselected(string? analysisSummary)
    {
        var story = Story(1, "<p>Fallback article</p>");
        story.Summary = "<p> </p>";
        if (analysisSummary != null)
            story.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel() { Summary = analysisSummary };
        var sections = new[] { ContentSection("news", 0, story) };
        ReportAISectionGenerator.BuildStories(sections, null, false).Single().Text.Should().Contain("Fallback article").And.NotContain("<p>");
        ReportAISectionGenerator.BuildStories(sections, null, false, new[] { "headline" }).Single().Text.Should().Contain("Fallback article");
        story.Body = "";
        ReportAISectionGenerator.BuildStories(sections, null, false).Single().Text.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LegacyTopicSummaryUsesThePromptAndLinksWithinTheReport()
    {
        var client = new FakeLlmClient(8000) { FinalOutput = "<p>Requested format [S1]</p>" };
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Use a paragraph", AIOutputMode = "TopicSummary", AIInputFields = new[] { "body" } });
        var generator = new ReportAISectionGenerator(new MemoryStore(), client,
            new TemplateOptions() { ViewContentUrl = new Uri("https://mmi.test/view/") }, NullLogger.Instance);
        var contentSection = ContentSection("news", 0, Story(1, "Transit news."));
        contentSection.Settings.ShowFullStory = true;

        var result = await generator.GenerateAsync(report, 5, section,
            new Dictionary<string, ReportSectionModel> { ["news"] = contentSection }, Array.Empty<PreviousReportModel>(), Llm());

        result.Error.Should().BeNull();
        result.Output.Should().Contain("Requested format").And.Contain("href=\"#item-1\"").And.NotContain("target=").And.Contain("View story");
        client.Requests.Should().Contain(r => r.Any(m => m.Content.Contains("Use a paragraph")));
        String.Join("\n", client.Requests.SelectMany(r => r.Select(m => m.Content))).Should().NotContain("Headline 1");
    }

    [Fact]
    public void LinksUseVisibleStoryBodiesEvenWhenAnotherSectionFirstSuppliesTheStory()
    {
        var story = Story(1, "Story body");
        var headlines = ContentSection("headlines", 0, story);
        var full = ContentSection("full", 1, story);
        full.Settings.ShowFullStory = true;
        var sections = new[] { headlines, full };
        var url = new Uri("https://mmi.test/view/");
        var result = ReportAISectionGenerator.BuildStories(sections, url, true).Single();
        result.Anchor.Should().Be("#item-1");
        result.Url.Should().Be("https://mmi.test/view/1");
        ReportAISectionGenerator.BuildStories(new[] { headlines }, url, true, anchorSections: sections).Single().Anchor.Should().Be("#item-1");
        full.IsEnabled = false;
        ReportAISectionGenerator.BuildStories(sections, url, true).Single().Anchor.Should().BeNull();
        full.IsEnabled = true;
        ReportAISectionGenerator.BuildStories(sections, url, false).Single().Anchor.Should().BeNull();
    }

    [Fact]
    public async Task EmptyOrUnknownFieldSelectionFailsBeforeCallingTheModel()
    {
        foreach (var fields in new[] { Array.Empty<string>(), new[] { "unknown" } })
        {
            var client = new FakeLlmClient(8000);
            var (report, section) = Report(new ReportSectionSettingsModel() { AIInputFields = fields });
            var result = await Generator(new MemoryStore(), client).GenerateAsync(report, 5, section,
                new Dictionary<string, ReportSectionModel>(), Array.Empty<PreviousReportModel>(), Llm());
            result.Error.Should().Contain("Choose at least one data field");
            client.Requests.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task ChangingFieldSelectionInvalidatesCachedOutput()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", AIInputFields = new[] { "body" } });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Transit news.")) };
        await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());
        section.Settings.AIInputFields = new[] { "body", "headline" };
        await Generator(store, client).GenerateAsync(report, 5, section, sections, Array.Empty<PreviousReportModel>(), Llm());
        store.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task FieldSelectionAlsoFiltersPriorReportContext()
    {
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", AIInputFields = new[] { "summary" } });
        var story = Story(1, "Excluded current body");
        story.Summary = "Current summary";
        var previous = Story(2, "Excluded historical body");
        previous.Summary = "Historical summary";
        var history = new[] { new PreviousReportModel(4, null, new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, previous) }) };
        var result = await Generator(new MemoryStore(), client).GenerateAsync(report, 5, section,
            new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, story) }, history, Llm());
        result.Error.Should().BeNull();
        var sent = String.Join("\n", client.Requests.SelectMany(r => r.Select(m => m.Content)));
        sent.Should().Contain("Historical summary").And.Contain("Current summary")
            .And.NotContain("Excluded").And.NotContain("Headline");
    }

    [Fact]
    public async Task SelectingArticleTextAddsCurrentAndPriorBodiesAndInvalidatesCachedOutput()
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", AIInputFields = new[] { "summary" } });
        var current = Story(1, "Full current article");
        current.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel() { Summary = "Current analysis summary" };
        var previous = Story(2, "Full prior article");
        previous.Evidence = new API.Areas.Services.Models.Content.ContentEvidenceModel() { Summary = "Prior analysis summary" };
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, current) };
        var history = new[] { new PreviousReportModel(4, null, new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, previous) }) };
        var generator = Generator(store, client);
        await generator.GenerateAsync(report, 5, section, sections, history, Llm());
        String.Join("\n", client.Requests.SelectMany(r => r.Select(m => m.Content))).Should().NotContain("Full current article").And.NotContain("Full prior article");

        section.Settings.AIInputFields = new[] { "summary", "body" };
        var previousRequests = client.Requests.Count;
        var result = await generator.GenerateAsync(report, 5, section, sections, history, Llm());
        result.Error.Should().BeNull();
        store.Results.Should().HaveCount(2);
        String.Join("\n", client.Requests.Skip(previousRequests).SelectMany(r => r.Select(m => m.Content))).Should()
            .Contain("Full current article").And.Contain("Full prior article")
            .And.NotContain("Current analysis summary").And.NotContain("Prior analysis summary");
    }

    [Fact]
    public void FieldSelectionSurvivesBothSettingsReaders()
    {
        var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        using var json = System.Text.Json.JsonDocument.Parse("{\"aiInputFields\":[\"summary\",\"source\"]}");
        new ReportSectionSettingsModel(json, options).AIInputFields.Should().Equal("summary", "source");
        var dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json.RootElement.GetRawText(), options)!;
        new ReportSectionSettingsModel(dictionary, options).AIInputFields.Should().Equal("summary", "source");
        using var missing = System.Text.Json.JsonDocument.Parse("{}");
        new ReportSectionSettingsModel(missing, options).AIInputFields.Should().BeNull();
        using var empty = System.Text.Json.JsonDocument.Parse("{\"aiInputFields\":[]}");
        new ReportSectionSettingsModel(empty, options).AIInputFields.Should().BeEmpty();
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

    [Theory]
    [InlineData("http://localhost:40081/view/")]
    [InlineData("")]
    public async Task PreviewAndWorkerShareCacheDespiteDifferentLinkConfiguration(string previewUrl)
    {
        var store = new MemoryStore();
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary", UserPrompt = "Summarize" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };
        var history = new[] { new PreviousReportModel(4, null, sections) };
        var previewOptions = new TemplateOptions() { ViewContentUrl = previewUrl.Length == 0 ? null : new Uri(previewUrl) };
        var workerOptions = new TemplateOptions() { ViewContentUrl = new Uri("https://dev.example.test/view/") };
        var previewGenerator = new ReportAISectionGenerator(store, client, previewOptions, NullLogger.Instance);
        var workerGenerator = new ReportAISectionGenerator(store, client, workerOptions, NullLogger.Instance);
        var pending = await previewGenerator.GenerateAsync(report, 5, section, sections, history, Llm(), AISectionWait.NoWait);
        pending.Status.Should().Be(AISectionStatus.NotStarted);

        // Preserve even an empty URL across the Kafka message: it explicitly disables links.
        var jsonOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        var message = new TNO.Kafka.Models.ReportRequestModel() { PrepareAISections = true, AIViewContentUrl = previewUrl };
        var json = System.Text.Json.JsonSerializer.Serialize(message, jsonOptions);
        var received = System.Text.Json.JsonSerializer.Deserialize<TNO.Kafka.Models.ReportRequestModel>(json, jsonOptions)!;
        var prepared = await workerGenerator.GenerateAsync(report, 5, section, sections, history, Llm(), AISectionWait.Prepare,
            viewContentUrlOverride: received.AIViewContentUrl);
        prepared.Status.Should().Be(AISectionStatus.Ready);
        var requests = client.Requests.Count;

        // Completion notifications and repeated browser refreshes must find the prepared result.
        for (var refresh = 0; refresh < 3; refresh++)
        {
            var preview = await previewGenerator.GenerateAsync(report, 5, section, sections, history, Llm(), AISectionWait.NoWait);
            preview.Status.Should().Be(AISectionStatus.Ready);
            preview.Output.Should().Be(prepared.Output);
        }
        client.Requests.Count.Should().Be(requests);
        store.Results.Should().ContainSingle();
        workerOptions.ViewContentUrl.Should().Be(new Uri("https://dev.example.test/view/"));

        // Older jobs still use the worker configuration, and cannot reuse different links.
        var workerPreview = await workerGenerator.GenerateAsync(report, 5, section, sections, history, Llm(), AISectionWait.NoWait);
        workerPreview.Status.Should().Be(AISectionStatus.NotStarted);
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

    [Theory]
    [InlineData(ReportAIResultStatus.Completed)]
    [InlineData(ReportAIResultStatus.Failed)]
    public async Task SendWaitsForAnotherGeneratorToSucceedOrFail(ReportAIResultStatus completion)
    {
        var stored = new ReportAIResultModel()
        {
            Id = 1,
            Status = ReportAIResultStatus.Pending,
            ClaimExpiresOn = DateTime.UtcNow.AddHours(1),
        };
        var store = new FixedStore(stored);
        var client = new FakeLlmClient(8000);
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var sending = Generator(store, client).GenerateAsync(report, 5, section, sections,
            Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.Wait, cancellation.Token);
        sending.IsCompleted.Should().BeFalse("an unfinished section must hold the report back");

        stored.Output = completion == ReportAIResultStatus.Completed ? "<p>Finished summary</p>" : "";
        stored.Error = completion == ReportAIResultStatus.Failed ? "The model refused." : null;
        stored.Status = completion;
        var result = await sending;

        result.Status.Should().Be(completion == ReportAIResultStatus.Completed ? AISectionStatus.Ready : AISectionStatus.Failed);
        result.Error.Should().Be(stored.Error);
        if (completion == ReportAIResultStatus.Completed) result.Output.Should().Be(stored.Output);
        else result.Output.Should().BeNull();
        client.Requests.Should().BeEmpty("a sender must accept the terminal result of the generation it waited for");
    }

    [Fact]
    public async Task CancellingAWaitingSendDoesNotTurnPendingWorkIntoAnAIFailure()
    {
        var store = new FixedStore(new ReportAIResultModel()
        {
            Id = 1,
            Status = ReportAIResultStatus.Pending,
            ClaimExpiresOn = DateTime.UtcNow.AddHours(1),
        });
        var (report, section) = Report(new ReportSectionSettingsModel() { Label = "Summary" });
        var sections = new Dictionary<string, ReportSectionModel> { ["news"] = ContentSection("news", 0, Story(1, "Budget news.")) };
        using var cancellation = new CancellationTokenSource();
        var sending = Generator(store, new FakeLlmClient(8000)).GenerateAsync(report, 5, section, sections,
            Array.Empty<PreviousReportModel>(), Llm(), AISectionWait.Wait, cancellation.Token);
        cancellation.Cancel();

        var act = async () => await sending;
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
