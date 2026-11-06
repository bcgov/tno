using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TNO.API.Models.Settings;
using TNO.DAL;
using TNO.DAL.Models;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL.Integration;

/// <summary>
/// Runs only when ELASTIC_IT_URL points at a disposable Elasticsearch.
/// </summary>
public sealed class ChartElasticFactAttribute : FactAttribute
{
    public ChartElasticFactAttribute([System.Runtime.CompilerServices.CallerFilePath] string? sourceFilePath = null, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELASTIC_IT_URL"))) Skip = "Set ELASTIC_IT_URL to run against Elasticsearch.";
    }
}

/// <summary>
/// The queries must be accepted by Elasticsearch, against the mapping migration 1.0.12 creates,
/// and count what the 'Custom' chart template counts for the same stories.
/// </summary>
public class ReportChartElasticIntegrationTest : IAsyncLifetime
{
    private const string Index = "it_report_charts";
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http = new() { BaseAddress = new Uri((Environment.GetEnvironmentVariable("ELASTIC_IT_URL") ?? "http://localhost:9200").TrimEnd('/') + "/") };
    private readonly TestHelper _helper = new();
    private readonly Mock<ITNOElasticClient> _elastic = new();
    private readonly Mock<IMediaTypeService> _mediaTypes = new();
    private readonly Mock<ISeriesService> _series = new();
    private readonly Dictionary<string, string> _config = new();

    public ReportChartElasticIntegrationTest()
    {
        _elastic.Setup(e => e.SearchRawAsync(It.IsAny<string>(), It.IsAny<JsonDocument>()))
            .Returns(async (string index, JsonDocument query) =>
            {
                var response = await _http.PostAsync($"{index}/_search", new StringContent(query.RootElement.GetRawText(), System.Text.Encoding.UTF8, "application/json"));
                var body = await response.Content.ReadAsStringAsync();
                Assert.True(response.IsSuccessStatusCode, body);
                return JsonDocument.Parse(body);
            });
        _mediaTypes.Setup(m => m.FindAll()).Returns([new MediaType("Print") { Id = 1 }, new MediaType("Radio") { Id = 2 }]);
        _series.Setup(s => s.FindAll()).Returns([new Series("Morning") { Id = 3 }]);
    }

    private IReportChartDataService Build(bool byLine)
    {
        _helper.Build(services =>
        {
            services.AddTNOContext("report-chart-elastic");
            services.AddPrincipalForRole("editor");
            services.AddOptions();
            services.Configure<ElasticOptions>(o => { o.ContentIndex = Index; o.PublishedIndex = Index; });
            services.Configure<JsonSerializerOptions>(o => o.PropertyNameCaseInsensitive = true);
            services.AddSingleton(_elastic.Object);
            services.AddSingleton(Mock.Of<IReportService>());
            services.AddSingleton(_mediaTypes.Object);
            services.AddSingleton(_series.Object);
            services.AddSingleton(Mock.Of<ILogger<ReportChartDataService>>());
            services.AddSingleton<IConfiguration>(
                new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { ReportChartSupport.ByLineAggregationKey, byLine.ToString() } }).Build());
            services.AddSingleton<IReportChartDataService, ReportChartDataService>();
        });
        return _helper.Provider.GetRequiredService<IReportChartDataService>();
    }

    private static async Task<JsonNode> Mapping()
    {
        var path = Path.Combine(FindRoot(), "tools/elastic/migration/Migrations/1.0.12/indexes/index-content.json");
        var data = JsonNode.Parse(await File.ReadAllTextAsync(path))!["data"]!.DeepClone();
        data["settings"]!["index"]!["number_of_replicas"] = 0;
        return data;
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "tools/elastic"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found");
    }

    public async ValueTask InitializeAsync()
    {
        if (String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELASTIC_IT_URL"))) return;
        await _http.DeleteAsync(Index);
        (await _http.PutAsync(Index, new StringContent((await Mapping()).ToJsonString(), System.Text.Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
        object Doc(long id, string published, string source, int media, int? series, string contentType, int? tone, string? byline, string? otherSeries, params (string Type, string Name)[] topics) => new
        {
            id,
            publishedOn = published,
            otherSource = source,
            mediaTypeId = media,
            seriesId = series,
            contentType,
            byline,
            otherSeries,
            tonePools = tone.HasValue ? new[] { new { id = 1, name = "Default", ownerId = 1, value = tone.Value } } : [],
            topics = topics.Select(t => new { id = 1, name = t.Name, topicType = t.Type, score = 1, isSystem = false }),
        };
        var docs = new[]
        {
            // 01:00 UTC on the 2nd is still the 1st in British Columbia.
            Doc(1, "2026-01-02T01:00:00Z", "Globe", 1, 3, "PrintContent", 2, "Ann <mark>Lee</mark>", null, ("Issues", "Health"), ("Issues", "Budget")),
            Doc(2, "2026-01-02T20:00:00Z", "Globe", 1, null, "PrintContent", -1, "", "Evening Extra", ("Proactive", "Health")),
            Doc(3, "2026-02-03T20:00:00Z", "Sun", 2, null, "AudioVideo", null, null, null),
        };
        foreach (var doc in docs)
            (await _http.PostAsJsonAsync($"{Index}/_doc?refresh=true", doc)).EnsureSuccessStatusCode();
    }

    public ValueTask DisposeAsync()
    {
        _helper.Dispose();
        _http.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private static ReportChartDataRequest Request(ChartSectionSettingsModel settings)
    {
        var chart = new ReportSectionChartTemplate(1, 1, 0) { Settings = JsonSerializer.SerializeToDocument(settings, Options) };
        return new ReportChartDataRequest
        {
            Report = new Report(1, "report", 1, 1),
            Section = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1),
            Chart = chart,
            Source = ReportChartSource.InstanceIds,
            SectionIds = new Dictionary<string, long[]> { { "stories", new long[] { 1, 2, 3 } } },
            SettingsOverride = settings,
        };
    }

    private async Task<Dictionary<string, double?>> GroupAsync(string groupBy, bool byLine = false, string value = "count")
    {
        var result = await Build(byLine).GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = groupBy, Dataset = "", DatasetValue = value }));
        Assert.Equal(3, result.DocCount);
        return result.Data.Labels.Zip(result.Data.Datasets.Single().Data).ToDictionary(p => p.First, p => p.Second);
    }

    [ChartElasticFact]
    public async Task Dates_GroupInBritishColumbiaTime()
    {
        var days = await GroupAsync("dayMonthYear");
        Assert.Equal(1, days["01-Jan-2026"]);
        Assert.Equal(1, days["02-Jan-2026"]);
        Assert.Equal(1, days["03-Feb-2026"]);
        var months = await GroupAsync("monthYear");
        Assert.Equal(2, months["Jan-2026"]);
        Assert.Equal(3, (await GroupAsync("year"))["2026"]);
    }

    [ChartElasticFact]
    public async Task Terms_CountAndName()
    {
        Assert.Equal(2, (await GroupAsync("source"))["Globe"]);
        Assert.Equal(1, (await GroupAsync("mediaType"))["Radio"]);
        Assert.Equal(1, (await GroupAsync("contentType"))["AudioVideo"]);
        var series = await GroupAsync("series");
        Assert.Equal(1, series["Morning"]);
        Assert.Equal(2, series["None"]);
    }

    [ChartElasticFact]
    public async Task Sentiment_HasANoneBucket()
    {
        var values = await GroupAsync("sentiment");
        Assert.Equal(1, values["2"]);
        Assert.Equal(1, values["-1"]);
        Assert.Equal(1, values["None"]);
        var simple = await GroupAsync("sentimentSimple");
        Assert.Equal(1, simple["Positive"]);
        Assert.Equal(1, simple["Negative"]);
        Assert.Equal(1, simple["None"]);
        Assert.False(simple.ContainsKey("Neutral") && simple["Neutral"] > 0);
    }

    [ChartElasticFact]
    public async Task Topics_CountStoriesOncePerBucket()
    {
        var types = await GroupAsync("topicType");
        Assert.Equal(1, types["Issues"]); // Story 1 has two topics of this type.
        Assert.Equal(1, types["Proactive"]);
        Assert.Equal(2, (await GroupAsync("topicName"))["Health"]);
    }

    [ChartElasticFact]
    public async Task AverageSentiment_RoundsAwayFromZero()
    {
        var result = await Build(false).GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "source", DatasetValue = "sentiment" }));
        var values = result.Data.Labels.Zip(result.Data.Datasets.Single().Data).ToDictionary(p => p.First, p => p.Second);
        Assert.Equal(1, values["Globe"]); // (2 + -1) / 2 = 0.5.
        Assert.Null(values["Sun"]);
    }

    [ChartElasticFact]
    public async Task TwoDimensions_WithANestedOuter()
    {
        var data = (await Build(false).GetChartDataAsync(Request(new ChartSectionSettingsModel { Dataset = "sentimentSimple", GroupBy = "source" }))).Data;
        Assert.Equal(new[] { "Globe", "Sun" }, data.Labels);
        var none = data.Datasets.Single(d => d.Label == "None");
        Assert.Equal(new double?[] { 0, 1 }, none.Data);
        var positive = data.Datasets.Single(d => d.Label == "Positive");
        Assert.Equal(new double?[] { 1, 0 }, positive.Data);
    }

    [ChartElasticFact]
    public async Task ByLine_UsesTheKeyword_AndCleansLabels()
    {
        var values = await GroupAsync("byline", byLine: true);
        Assert.Equal(1, values["Ann Lee"]);
        Assert.Equal(2, values["Unknown"]);
    }

    [ChartElasticFact]
    public async Task Series_FallsBackToOtherSeries()
    {
        var values = await GroupAsync("series", byLine: true);
        Assert.Equal(1, values["Morning"]);
        Assert.Equal(1, values["Evening Extra"]);
        Assert.Equal(1, values["None"]);
    }
}
