using System.Text.Json;
using System.Text.Json.Nodes;
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

namespace TNO.Test.DAL;

/// <summary>
/// Chart data must match what the 'Custom' chart template produces from the same stories,
/// without fetching any of them.
/// </summary>
public class ReportChartDataServiceTest : IDisposable
{
    #region Variables
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    readonly TestHelper helper = new();
    readonly Mock<ITNOElasticClient> elastic = new();
    readonly Mock<IReportService> reportService = new();
    readonly Mock<IMediaTypeService> mediaTypes = new();
    readonly Mock<ISeriesService> series = new();
    readonly List<JsonDocument> queries = new();
    #endregion

    #region Constructor
    public ReportChartDataServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("report-chart-data");
            services.AddPrincipalForRole("editor");
            services.AddOptions();
            services.Configure<ElasticOptions>(o => { o.ContentIndex = "unpublished_content"; o.PublishedIndex = "content"; });
            services.Configure<JsonSerializerOptions>(o => o.PropertyNameCaseInsensitive = true);
            services.AddSingleton(elastic.Object);
            services.AddSingleton(reportService.Object);
            services.AddSingleton(mediaTypes.Object);
            services.AddSingleton(series.Object);
            services.AddSingleton(Mock.Of<ILogger<ReportChartDataService>>());
            services.AddSingleton<IReportChartDataService, ReportChartDataService>();
        });
    }
    #endregion

    #region Helpers
    private IReportChartDataService Service => helper.Provider.GetRequiredService<IReportChartDataService>();

    private void Respond(params string[] responses)
    {
        var queue = new Queue<string>(responses);
        elastic.Setup(e => e.SearchRawAsync(It.IsAny<string>(), It.IsAny<JsonDocument>()))
            .ReturnsAsync((string index, JsonDocument query) =>
            {
                queries.Add(query);
                return JsonDocument.Parse(queue.Count > 1 ? queue.Dequeue() : queue.Peek());
            });
    }

    private static string Total(long total, string aggregations = "{}") => "{\"hits\":{\"total\":{\"value\":" + total + "}},\"aggregations\":" + aggregations + "}";

    private static ReportChartDataRequest Request(ChartSectionSettingsModel settings, Dictionary<string, long[]>? ids = null)
    {
        var report = new Report(1, "report", 1, 1);
        var section = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1);
        var chart = new ReportSectionChartTemplate(1, 1, 0) { Settings = JsonSerializer.SerializeToDocument(settings, Options) };
        return new ReportChartDataRequest
        {
            Report = report,
            Section = section,
            Chart = chart,
            Source = ReportChartSource.InstanceIds,
            SectionIds = ids ?? new Dictionary<string, long[]> { { "stories", new long[] { 1, 2, 3 } } },
            SettingsOverride = settings,
        };
    }

    private static JsonObject Aggs(string dimension, string order = "asc", bool sentiment = false, bool byLine = false)
    {
        return ChartAggregationQuery.Build(new[] { (dimension, order) }, sentiment, byLine);
    }
    #endregion

    #region Query
    [Theory]
    [InlineData("dayMonthYear", "day", "yyyyMMdd")]
    [InlineData("monthDay", "day", "yyyyMMdd")]
    [InlineData("monthYear", "month", "yyyyMM")]
    [InlineData("year", "year", "yyyy")]
    public void Dates_GroupInBritishColumbiaTime(string dimension, string interval, string format)
    {
        var histogram = Aggs(dimension, "desc")["l0"]!["date_histogram"]!;
        Assert.Equal("publishedOn", histogram["field"]!.GetValue<string>());
        Assert.Equal(interval, histogram["calendar_interval"]!.GetValue<string>());
        Assert.Equal(format, histogram["format"]!.GetValue<string>());
        Assert.Equal("America/Vancouver", histogram["time_zone"]!.GetValue<string>());
        Assert.Equal(1, histogram["min_doc_count"]!.GetValue<int>());
        Assert.Equal("desc", histogram["order"]!["_key"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("source", "otherSource")]
    [InlineData("otherSource", "otherSource")]
    [InlineData("mediaType", "mediaTypeId")]
    [InlineData("series", "seriesId")]
    [InlineData("contentType", "contentType")]
    public void Terms_UseTheKeywordField(string dimension, string field)
    {
        var terms = Aggs(dimension)["l0"]!["terms"]!;
        Assert.Equal(field, terms["field"]!.GetValue<string>());
        Assert.Equal(1000, terms["size"]!.GetValue<int>());
        Assert.Equal("asc", terms["order"]!["_key"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("topicType", "topics", "topics.topicType")]
    [InlineData("topicName", "topics", "topics.name")]
    [InlineData("sentiment", "tonePools", "tonePools.value")]
    public void Nested_CountsStoriesWithReverseNested(string dimension, string path, string field)
    {
        var nested = Aggs(dimension)["l0"]!;
        Assert.Equal(path, nested["nested"]!["path"]!.GetValue<string>());
        var values = nested["aggs"]!["v"]!;
        Assert.Equal(field, values["terms"]!["field"]!.GetValue<string>());
        Assert.NotNull(values["aggs"]!["docs"]!["reverse_nested"]);
    }

    [Fact]
    public void SentimentSimple_UsesNamedRanges()
    {
        var ranges = Aggs("sentimentSimple")["l0"]!["aggs"]!["v"]!["range"]!["ranges"]!.AsArray();
        Assert.Equal(new[] { "Negative", "Neutral", "Positive" }, ranges.Select(r => r!["key"]!.GetValue<string>()));
        Assert.Equal(0, ranges[0]!["to"]!.GetValue<int>());
        Assert.Equal(1, ranges[1]!["to"]!.GetValue<int>());
        Assert.Equal(1, ranges[2]!["from"]!.GetValue<int>());
    }

    [Fact]
    public void Sentiment_UsesSumAndCount_SoChunksMerge()
    {
        var tone = Aggs("source", sentiment: true)["l0"]!["aggs"]!["tone"]!["aggs"]!;
        Assert.Equal("tonePools.value", tone["sum"]!["sum"]!["field"]!.GetValue<string>());
        Assert.Equal("tonePools.value", tone["count"]!["value_count"]!["field"]!.GetValue<string>());
    }

    [Fact]
    public void TwoDimensions_NestedOuter_SubAggregatesInsideReverseNested()
    {
        var aggs = ChartAggregationQuery.Build(new[] { ("topicType", "asc"), ("source", "asc") }, false, false);
        var inner = aggs["l0"]!["aggs"]!["v"]!["aggs"]!["docs"]!["aggs"]!["l1"]!;
        Assert.Equal("otherSource", inner["terms"]!["field"]!.GetValue<string>());
    }

    [Fact]
    public void TwoDimensions_NestedInner_IsItsOwnNestedBlock()
    {
        var aggs = ChartAggregationQuery.Build(new[] { ("source", "asc"), ("topicName", "asc") }, false, false);
        Assert.Equal("topics", aggs["l0"]!["aggs"]!["l1"]!["nested"]!["path"]!.GetValue<string>());
    }

    [Fact]
    public void Byline_RequiresTheKeywordField()
    {
        Assert.Throws<InvalidOperationException>(() => Aggs("byline"));
        Assert.Equal("byline.keyword", Aggs("byline", byLine: true)["l0"]!["terms"]!["field"]!.GetValue<string>());
    }

    [Fact]
    public void Series_FallsBackToOtherSeries_OnlyWhenTheKeywordExists()
    {
        Assert.Null(Aggs("series")["l0_other"]);
        Assert.Equal("otherSeries.keyword", Aggs("series", byLine: true)["l0_other"]!["aggs"]!["v"]!["terms"]!["field"]!.GetValue<string>());
    }

    [Fact]
    public async Task Query_ReturnsNoHits_AndChunksIdsAtTheLimit()
    {
        Respond(Total(1));
        var ids = Enumerable.Range(1, ReportChartDataService.MaxIdsPerQuery + 5).Select(i => (long)i).ToArray();

        await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "source" }, new() { { "stories", ids } }));

        Assert.Equal(2, queries.Count);
        var first = queries[0].RootElement;
        Assert.Equal(0, first.GetProperty("size").GetInt32());
        Assert.True(first.GetProperty("track_total_hits").GetBoolean());
        Assert.Equal(ReportChartDataService.MaxIdsPerQuery, first.GetProperty("query").GetProperty("terms").GetProperty("id").GetArrayLength());
        Assert.Equal(5, queries[1].RootElement.GetProperty("query").GetProperty("terms").GetProperty("id").GetArrayLength());
    }

    [Fact]
    public async Task Filter_StripsWhatWouldReturnHits_AndKeepsTheQuery()
    {
        Respond(Total(250000));
        var filterQuery = JsonDocument.Parse("""{"size":10000,"from":5,"sort":[{"publishedOn":"desc"}],"_source":["id"],"highlight":{},"collapse":{"field":"id"},"query":{"term":{"status":2}}}""");
        reportService.Setup(s => s.BuildSectionQueryContext(It.IsAny<Report>(), It.IsAny<long?>(), It.IsAny<int?>()))
            .Returns(new SectionQueryContext([], [], [], null, new ReportSettingsModel(), 1));
        reportService.Setup(s => s.BuildSectionQuery(It.IsAny<Report>(), It.IsAny<ReportSection>(), It.IsAny<SectionQueryContext>()))
            .Returns(("content", filterQuery));
        var request = Request(new ChartSectionSettingsModel { GroupBy = "" });
        request.Source = ReportChartSource.Filter;

        var result = await Service.GetChartDataAsync(request);

        var sent = queries.Single().RootElement;
        Assert.Equal(0, sent.GetProperty("size").GetInt32());
        foreach (var removed in new[] { "from", "sort", "_source", "highlight", "collapse" })
            Assert.False(sent.TryGetProperty(removed, out _), removed);
        Assert.Equal(2, sent.GetProperty("query").GetProperty("term").GetProperty("status").GetInt32());
        Assert.Equal(250000, result.DocCount);
        Assert.Equal(new double?[] { 250000 }, result.Data.Datasets.Single().Data);
    }
    #endregion

    #region Conversion
    [Fact]
    public async Task NoDimension_IsOneBucket()
    {
        Respond(Total(7));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "", Dataset = "" }))).Data;
        Assert.Equal(new[] { "Story Count" }, data.Labels);
        Assert.Equal("Story Count", data.Datasets.Single().Label);
        Assert.Equal(new double?[] { 7 }, data.Datasets.Single().Data);
    }

    [Fact]
    public async Task NoStories_ProducesNoLabelsOrDatasets()
    {
        Respond(Total(0));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "", Dataset = "" }))).Data;
        Assert.Empty(data.Labels);
        Assert.Empty(data.Datasets);
    }

    [Theory]
    [InlineData("dayMonthYear", "20260104", "04-Jan-2026")]
    [InlineData("monthDay", "20260104", "Jan-04")]
    [InlineData("monthYear", "202601", "Jan-2026")]
    [InlineData("year", "2026", "2026")]
    public async Task Dates_AreFormattedFromTheKey(string groupBy, string key, string label)
    {
        Respond(Total(2, "{\"l0\":{\"buckets\":[{\"key\":1,\"key_as_string\":\"" + key + "\",\"doc_count\":2}]}}"));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = groupBy }))).Data;
        Assert.Equal(new[] { label }, data.Labels);
    }

    [Fact]
    public async Task MediaTypeAndSeries_AreNamed_WithFallbacks()
    {
        mediaTypes.Setup(m => m.FindAll()).Returns(new[] { new MediaType("Print") { Id = 1 } });
        series.Setup(s => s.FindAll()).Returns(new[] { new Series("Morning") { Id = 3 } });

        Respond(Total(3, """{"l0":{"buckets":[{"key":1,"doc_count":2},{"key":9,"doc_count":1}]}}"""));
        var media = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "mediaType" }))).Data;
        Assert.Equal(new[] { "Other", "Print" }, media.Labels);
        Assert.Equal(new double?[] { 1, 2 }, media.Datasets.Single().Data);

        Respond(Total(3, """{"l0":{"buckets":[{"key":0,"doc_count":1},{"key":3,"doc_count":2}]}}"""));
        var seriesData = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "series" }))).Data;
        Assert.Equal(new[] { "Morning", "None" }, seriesData.Labels);
    }

    [Fact]
    public async Task Sentiment_StoriesWithoutAPool_AreTheNoneBucket()
    {
        Respond(Total(5, """
            {"l0":{"doc_count":4,"v":{"buckets":[
                {"key":-1,"doc_count":1,"docs":{"doc_count":1}},
                {"key":2,"doc_count":3,"docs":{"doc_count":3}}]}},
             "l0_none":{"doc_count":1}}
            """));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "sentiment" }))).Data;
        Assert.Equal(new[] { "None", "-1", "2" }.OrderBy(k => k, StringComparer.InvariantCulture), data.Labels);
        var values = data.Labels.Zip(data.Datasets.Single().Data).ToDictionary(p => p.First, p => p.Second);
        Assert.Equal(1, values["None"]);
        Assert.Equal(1, values["-1"]);
        Assert.Equal(3, values["2"]);
    }

    [Fact]
    public async Task Nested_CountsStories_NotNestedDocuments()
    {
        Respond(Total(1, """{"l0":{"doc_count":2,"v":{"buckets":[{"key":"Issues","doc_count":2,"docs":{"doc_count":1}}]}}}"""));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "topicType" }))).Data;
        Assert.Equal(new double?[] { 1 }, data.Datasets.Single().Data);
    }

    [Fact]
    public async Task SentimentValue_RoundsAwayFromZero_AndIsNullWithoutValues()
    {
        Respond(Total(3, """
            {"l0":{"buckets":[
                {"key":"A","doc_count":2,"tone":{"doc_count":2,"sum":{"value":5},"count":{"value":2}}},
                {"key":"B","doc_count":1,"tone":{"doc_count":0,"sum":{"value":0},"count":{"value":0}}}]}}
            """));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "source", DatasetValue = "sentiment" }))).Data;
        Assert.Equal(new double?[] { 3, null }, data.Datasets.Single().Data);
    }

    [Fact]
    public async Task Chunks_MergeCounts_AndRecomputeSentimentFromSums()
    {
        Respond(
            Total(1, """{"l0":{"buckets":[{"key":"A","doc_count":1,"tone":{"sum":{"value":1},"count":{"value":1}}}]}}"""),
            Total(1, """{"l0":{"buckets":[{"key":"A","doc_count":1,"tone":{"sum":{"value":2},"count":{"value":1}}}]}}"""));
        var ids = Enumerable.Range(1, ReportChartDataService.MaxIdsPerQuery + 1).Select(i => (long)i).ToArray();

        var result = await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "source", DatasetValue = "sentiment" }, new() { { "stories", ids } }));

        Assert.Equal(new double?[] { 2 }, result.Data.Datasets.Single().Data); // 3 / 2 = 1.5, rounded away from zero.
        Assert.Equal(2, result.DocCount);
    }

    [Fact]
    public async Task TwoDimensions_AlignEachDatasetWithTheLabels()
    {
        Respond(Total(3, """
            {"l0":{"buckets":[
                {"key":"X","doc_count":2,"l1":{"buckets":[{"key":"A","doc_count":2}]}},
                {"key":"Y","doc_count":1,"l1":{"buckets":[{"key":"B","doc_count":1}]}}]}}
            """));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { Dataset = "contentType", GroupBy = "source" }))).Data;
        Assert.Equal(new[] { "A", "B" }, data.Labels);
        Assert.Equal(new double?[] { 2, 0 }, data.Datasets[0].Data);
        Assert.Equal(new double?[] { 0, 1 }, data.Datasets[1].Data);
    }

    [Fact]
    public async Task ReportSection_EachSectionIsABucket_InSortOrder()
    {
        Respond(Total(2), Total(1));
        var request = Request(new ChartSectionSettingsModel { GroupBy = "reportSection" }, new() { { "zebra", new long[] { 1, 2 } }, { "apple", new long[] { 3 } } });
        request.SectionLabels = new Dictionary<string, string?> { { "zebra", "Zebra" }, { "apple", null } };

        var data = (await Service.GetChartDataAsync(request)).Data;

        Assert.Equal(new[] { "Zebra", "Other" }, data.Labels);
        Assert.Equal(new double?[] { 2, 1 }, data.Datasets.Single().Data);
        Assert.False(queries[0].RootElement.TryGetProperty("aggs", out _));
    }

    [Theory]
    [InlineData("series", """{"l0":{"buckets":[{"key":0,"doc_count":1},{"key":3,"doc_count":2}]}}""", "Morning")]
    [InlineData("sentimentSimple", """{"l0":{"v":{"buckets":[{"key":"Positive","doc_count":2,"docs":{"doc_count":2}}]}},"l0_none":{"doc_count":1}}""", "Positive")]
    public async Task ExcludeEmptyValues_RemovesTheNoneBucket(string groupBy, string aggregations, string remaining)
    {
        series.Setup(s => s.FindAll()).Returns(new[] { new Series("Morning") { Id = 3 } });
        Respond(Total(3, aggregations));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = groupBy, ExcludeEmptyValues = true }))).Data;
        Assert.Equal(new[] { remaining }, data.Labels);
    }

    [Fact]
    public async Task ExcludeEmptyValues_WithSentiment_RemovesBucketsWithoutValues()
    {
        Respond(Total(2, """
            {"l0":{"buckets":[
                {"key":"A","doc_count":1,"tone":{"sum":{"value":1},"count":{"value":1}}},
                {"key":"B","doc_count":1,"tone":{"sum":{"value":0},"count":{"value":0}}}]}}
            """));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "source", DatasetValue = "sentiment", ExcludeEmptyValues = true }))).Data;
        Assert.Equal(new[] { "A" }, data.Labels);
    }

    [Fact]
    public async Task Truncated_WhenTermsOverflow()
    {
        Respond(Total(2, """{"l0":{"sum_other_doc_count":5,"buckets":[{"key":"A","doc_count":2}]}}"""));
        var result = await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "source" }));
        Assert.True(result.Truncated);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public async Task Byline_IsRejected_UntilTheFlagIsOn()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "byline" })));
    }

    [Fact]
    public async Task Colours_KeepTheSentimentScriptAsAString()
    {
        Respond(Total(1));
        var data = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { GroupBy = "", Dataset = "", DatasetValue = "sentiment" }))).Data;
        var json = JsonSerializer.SerializeToElement(data);
        var dataset = json.GetProperty("datasets")[0];
        Assert.StartsWith("(ctx,options) =>", dataset.GetProperty("backgroundColor").GetString());
        Assert.Equal("Average Sentiment", dataset.GetProperty("label").GetString());
        Assert.True(dataset.GetProperty("spanGaps").GetBoolean());
        Assert.Equal(2, dataset.GetProperty("borderWidth").GetInt32());
    }

    [Fact]
    public async Task Colours_PickByDatasetIndex_OrEveryColourPerValue()
    {
        Respond(Total(1, """{"l0":{"buckets":[{"key":"A","doc_count":1}]}}"""));
        var colors = new[] { "#111", "#222" };
        var perDataset = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { Dataset = "source", GroupBy = "", DatasetColors = colors }))).Data;
        Assert.Equal("""["#111"]""", JsonSerializer.Serialize(perDataset.Datasets[0].BackgroundColor));

        var perValue = (await Service.GetChartDataAsync(Request(new ChartSectionSettingsModel { Dataset = "source", GroupBy = "", DatasetColors = colors, ApplyColorToValue = true }))).Data;
        Assert.Equal("""["#111","#222"]""", JsonSerializer.Serialize(perValue.Datasets[0].BackgroundColor));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Colours_AnEmptyList_IsNoColour(bool applyColorToValue)
    {
        // Choosing a dataset type in the chart editor leaves both lists empty. Chart.js draws a
        // colour it cannot find in black, so the chart is given none and keeps its own.
        Respond(Total(1, """{"l0":{"buckets":[{"key":"A","doc_count":1}]}}"""));
        var settings = new ChartSectionSettingsModel { Dataset = "source", GroupBy = "", DatasetColors = [], DatasetBorderColors = [], ApplyColorToValue = applyColorToValue };

        var dataset = (await Service.GetChartDataAsync(Request(settings))).Data.Datasets[0];

        Assert.Equal("null", JsonSerializer.Serialize(dataset.BackgroundColor));
        Assert.Equal("null", JsonSerializer.Serialize(dataset.BorderColor));
    }

    [Fact]
    public async Task Colours_AnEmptyList_KeepsTheSentimentColours()
    {
        Respond(Total(1, """{"l0":{"v":{"buckets":[{"key":"Positive","doc_count":1,"docs":{"doc_count":1}}]}},"l0_none":{"doc_count":0}}"""));
        var settings = new ChartSectionSettingsModel { Dataset = "sentimentSimple", GroupBy = "", DatasetColors = [], DatasetBorderColors = [] };

        var dataset = (await Service.GetChartDataAsync(Request(settings))).Data.Datasets.First(d => d.Label == "Positive");

        Assert.Equal("""["green"]""", JsonSerializer.Serialize(dataset.BackgroundColor));
    }
    #endregion

    #region Support
    private static ReportSection AnalyticsSection(bool aggregate, string groupBy = "source", int? filterId = 5, string query = """{"query":{}}""")
    {
        var section = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1, filterId, null, null) { Settings = JsonDocument.Parse("{}") };
        if (filterId.HasValue) section.Filter = new Filter(5, "filter", 1) { Query = JsonDocument.Parse(query) };
        section.ChartTemplatesManyToMany.Add(new ReportSectionChartTemplate(1, 1, 0)
        {
            Settings = JsonSerializer.SerializeToDocument(new ChartSectionSettingsModel { GroupBy = groupBy }, Options),
            ChartTemplate = new ChartTemplate("Custom", "") { Settings = JsonDocument.Parse(aggregate ? """{"aggregate":true}""" : "{}") },
        });
        return section;
    }

    [Fact]
    public void AnalyticsOnly_RequiresEveryChartToAggregate()
    {
        Assert.True(ReportChartSupport.IsAnalyticsOnly(AnalyticsSection(true), Options, false));
        Assert.False(ReportChartSupport.IsAnalyticsOnly(AnalyticsSection(false), Options, false));
    }

    [Fact]
    public void AnalyticsOnly_KeepsFetchingByline_UntilTheFlagIsOn()
    {
        Assert.False(ReportChartSupport.IsAnalyticsOnly(AnalyticsSection(true, "byline"), Options, false));
        Assert.True(ReportChartSupport.IsAnalyticsOnly(AnalyticsSection(true, "byline"), Options, true));
    }

    [Fact]
    public void AnalyticsOnly_KeepsRunningAFilterWithItsOwnAggregations()
    {
        Assert.False(ReportChartSupport.IsAnalyticsOnly(AnalyticsSection(true, query: """{"aggs":{"a":{}}}"""), Options, false));
    }

    [Fact]
    public void AnalyticsOnly_NeedsItsOwnSource()
    {
        Assert.False(ReportChartSupport.IsAnalyticsOnly(AnalyticsSection(true, filterId: null), Options, false));
        var section = AnalyticsSection(true);
        section.Settings = JsonDocument.Parse("""{"useAllContent":true}""");
        Assert.False(ReportChartSupport.IsAnalyticsOnly(section, Options, false));
    }

    [Fact]
    public void BucketMode_NeedsNoStories()
    {
        var section = AnalyticsSection(false);
        section.ChartTemplatesManyToMany[0].ChartTemplate!.Settings = JsonDocument.Parse("""{"aggregations":{"topics":{"nested":{"path":"topics"}}}}""");
        Assert.True(ReportChartSupport.IsAnalyticsOnly(section, Options, false));
    }
    #endregion

    #region Stories changed
    private const int OwnerId = 7;
    /// <summary>The report's own instance, which a section linked to another report never reads.</summary>
    private const long OwnInstanceId = 900;

    private TNOContext Context => helper.Provider.GetRequiredService<TNOContext>();

    private static (Report Report, ReportSection Section) LinkedSection(int linkedReportId, int? ownerId = OwnerId)
    {
        var report = new Report(1, "report", 1, ownerId);
        return (report, new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1, null, null, linkedReportId));
    }

    private void AddInstance(long instanceId, int reportId, int? ownerId = OwnerId, params long[] contentIds)
    {
        Context.ReportInstances.Add(new ReportInstance(instanceId, reportId, ownerId));
        AddStories(instanceId, contentIds);
    }

    /// <summary>
    /// The way 'Add to report' does it: the links are inserted and the instance is left as it was.
    /// </summary>
    private void AddStories(long instanceId, params long[] contentIds)
    {
        Context.ReportInstanceContents.AddRange(contentIds.Select(id => new ReportInstanceContent(instanceId, id, "stories", 0)));
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    private static ReportSection AllContentSection(int? linkedReportId = null) =>
        new(1, "chart", ReportSectionType.MediaAnalytics, 1, null, null, linkedReportId) { Settings = JsonDocument.Parse("""{"useAllContent":true}""") };

    private static ReportInstanceChart StoredChart(DateTime generatedOn) =>
        new(1, 1, 1, "chart", JsonDocument.Parse("{}")) { GeneratedOn = generatedOn };

    [Fact]
    public void StoriesChangedOn_IsNull_WhenTheSectionFetchesItsOwnStories()
    {
        // A filter or folder is run when the section is generated, so its chart is a snapshot of that run.
        AddInstance(1001, 1, OwnerId, 1);
        var report = new Report(1, "report", 1, OwnerId);
        var filtered = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1, 8, null, null);
        var folder = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1, null, 8, null);

        Assert.Null(Service.GetStoriesChangedOn(report, filtered, 1001));
        Assert.Null(Service.GetStoriesChangedOn(report, folder, 1001));
    }

    [Fact]
    public void AllContent_IsNull_WithoutAnInstance()
    {
        Assert.Null(Service.GetStoriesChangedOn(new Report(1, "report", 1, OwnerId), AllContentSection(), 1010));
    }

    [Fact]
    public void AllContent_Moves_WhenAStoryIsAddedToTheReport()
    {
        var report = new Report(1, "report", 1, OwnerId);
        AddInstance(1011, 1, OwnerId, 1, 2);
        var before = Service.GetStoriesChangedOn(report, AllContentSection(), 1011);

        AddStories(1011, 3);
        var after = Service.GetStoriesChangedOn(report, AllContentSection(), 1011);

        Assert.NotNull(before);
        Assert.True(after > before);
        // A chart generated between the two no longer matches the report.
        Assert.False(ReportChartSupport.IsCurrent(StoredChart(before!.Value.AddTicks(1)), new ReportSectionChartTemplate(1, 1, 0), AllContentSection(), after));
    }

    [Fact]
    public void AllContent_ReadsTheReportsOwnInstance_NotALinkedReports()
    {
        var report = new Report(1, "report", 1, OwnerId);
        AddInstance(1012, 1, OwnerId, 1);
        AddInstance(1013, 112, OwnerId, 2);
        var before = Service.GetStoriesChangedOn(report, AllContentSection(112), 1012);

        // 'Use all content' wins over a linked report, as it does when the chart is computed.
        AddStories(1013, 3);
        Assert.Equal(before, Service.GetStoriesChangedOn(report, AllContentSection(112), 1012));

        AddStories(1012, 4);
        Assert.True(Service.GetStoriesChangedOn(report, AllContentSection(112), 1012) > before);
    }

    [Fact]
    public void LinkedReport_IsNull_UntilItHasAnInstance()
    {
        var (report, section) = LinkedSection(102);
        Assert.Null(Service.GetStoriesChangedOn(report, section, OwnInstanceId));
    }

    [Fact]
    public void LinkedReport_Moves_WhenAStoryIsAdded()
    {
        var (report, section) = LinkedSection(103);
        AddInstance(1003, 103, OwnerId, 1, 2);
        var before = Service.GetStoriesChangedOn(report, section, OwnInstanceId);

        AddStories(1003, 3);
        var after = Service.GetStoriesChangedOn(report, section, OwnInstanceId);

        Assert.NotNull(before);
        Assert.True(after > before);
        // A chart generated between the two no longer matches the linked report.
        Assert.False(ReportChartSupport.IsCurrent(StoredChart(before!.Value.AddTicks(1)), new ReportSectionChartTemplate(1, 1, 0), section, after));
    }

    [Fact]
    public void LinkedReport_Moves_WhenAStoryIsRemoved()
    {
        var (report, section) = LinkedSection(104);
        AddInstance(1004, 104, OwnerId, 1, 2);
        var before = Service.GetStoriesChangedOn(report, section, OwnInstanceId);

        // Removing a story saves the instance with the links that remain.
        var instance = Context.ReportInstances.Single(i => i.Id == 1004);
        Context.ReportInstanceContents.Remove(Context.ReportInstanceContents.Single(c => c.InstanceId == 1004 && c.ContentId == 2));
        Context.Update(instance);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        Assert.True(Service.GetStoriesChangedOn(report, section, OwnInstanceId) > before);
    }

    [Fact]
    public void LinkedReport_Moves_WhenItStartsANewInstance()
    {
        var (report, section) = LinkedSection(105);
        AddInstance(1005, 105, OwnerId, 1, 2);
        var before = Service.GetStoriesChangedOn(report, section, OwnInstanceId);

        AddInstance(1006, 105);

        Assert.True(Service.GetStoriesChangedOn(report, section, OwnInstanceId) > before);
    }

    [Fact]
    public void LinkedReport_ReadsTheInstanceTheChartReads_TheReportOwnersNewest()
    {
        var (report, section) = LinkedSection(107);
        AddInstance(1007, 107, OwnerId, 1);
        var mine = Service.GetStoriesChangedOn(report, section, OwnInstanceId);

        // Another user's newer instance of the linked report is not where this chart's stories come from.
        AddInstance(1008, 107, 99, 2, 3);

        Assert.Equal(mine, Service.GetStoriesChangedOn(report, section, OwnInstanceId));
    }

    [Fact]
    public void StoredChart_IsCurrent_UntilSomethingItShowsChanges()
    {
        var generatedOn = new DateTime(2026, 11, 12, 13, 10, 0, DateTimeKind.Utc);
        var section = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1) { UpdatedOn = generatedOn.AddMinutes(-5) };
        var chart = new ReportSectionChartTemplate(1, 1, 0) { UpdatedOn = generatedOn.AddMinutes(-5) };
        var row = StoredChart(generatedOn);

        Assert.True(ReportChartSupport.IsCurrent(row, chart, section, null));
        Assert.True(ReportChartSupport.IsCurrent(row, chart, section, generatedOn.AddMinutes(-1)));
        // Stories added after the chart was generated.
        Assert.False(ReportChartSupport.IsCurrent(row, chart, section, generatedOn.AddMinutes(24)));
        // A story stamped at the same moment may not have been counted.
        Assert.False(ReportChartSupport.IsCurrent(row, chart, section, generatedOn));
    }

    [Fact]
    public void StoredChart_IsStale_WhenItsChartOrSectionWasEdited()
    {
        var generatedOn = new DateTime(2026, 11, 12, 13, 10, 0, DateTimeKind.Utc);
        var section = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1) { UpdatedOn = generatedOn.AddMinutes(-5) };
        var chart = new ReportSectionChartTemplate(1, 1, 0) { UpdatedOn = generatedOn.AddMinutes(-5) };
        var row = StoredChart(generatedOn);

        chart.UpdatedOn = generatedOn.AddMinutes(1);
        Assert.False(ReportChartSupport.IsCurrent(row, chart, section, null));

        chart.UpdatedOn = generatedOn.AddMinutes(-5);
        section.UpdatedOn = generatedOn.AddMinutes(1);
        Assert.False(ReportChartSupport.IsCurrent(row, chart, section, null));
    }

    [Fact]
    public async Task ComputedChart_IsStampedBeforeItsStoriesAreRead()
    {
        // A story added while the chart is being computed must count as newer than the chart.
        var readOn = DateTime.MinValue;
        var report = new Report(1, "report", 1, OwnerId);
        var section = new ReportSection(1, "chart", ReportSectionType.MediaAnalytics, 1, null, null, 108);
        section.ChartTemplatesManyToMany.Add(new ReportSectionChartTemplate(1, 1, 0)
        {
            ChartTemplate = new ChartTemplate("Custom", "") { Settings = JsonDocument.Parse("""{"aggregate":true}""") },
            Settings = JsonSerializer.SerializeToDocument(new ChartSectionSettingsModel { GroupBy = "reportSection" }, Options),
        });
        report.Sections.Add(section);
        reportService.Setup(r => r.GetCurrentReportInstance(108, OwnerId, false)).Returns(() =>
        {
            Thread.Sleep(20);
            readOn = DateTime.UtcNow;
            return (ReportInstance?)null;
        });

        var row = Assert.Single(await Service.ComputeChartsAsync(report, 1, null, OwnerId));

        Assert.True(row.GeneratedOn < readOn);
    }
    #endregion

    #region Convert
    [Fact]
    public void Convert_SmallNegativeSentiment_IsNotNegativeZero()
    {
        var reader = new ChartAggregationReader(["sentiment"], (_, key) => key);
        reader.Cells[("", "a")] = new ChartCell { Count = 5, SentimentSum = -1, SentimentCount = 5 };

        var value = ReportChartDataService.Convert(new ChartSectionSettingsModel { DatasetValue = "sentiment" }, reader, [], new Dictionary<string, string?>()).Datasets[0].Data[0];

        Assert.False(double.IsNegative(value!.Value));
        Assert.Equal(0, value);
    }

    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
