using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using TNO.TemplateEngine.FilterAI;

namespace TNO.Test.AI;

/// <summary>
/// Tests for the bounded search a filter AI model may run, and for the extended query shape.
/// </summary>
public class FilterAISearchTest
{
    #region Helpers
    private static JsonElement Args(string json) => JsonDocument.Parse(json).RootElement;
    #endregion

    [Fact]
    public void SearchBodyIsBoundedAroundTheModelsQuery()
    {
        var body = FilterAISearch.BuildSearchBody(Args("{\"query\":{\"match\":{\"headline\":\"fire\"}},\"size\":500}"));

        body["query"]!.ToJsonString().Should().Be("{\"match\":{\"headline\":\"fire\"}}");
        body["size"]!.GetValue<int>().Should().Be(FilterAISearch.MaxSize);
        body["timeout"]!.GetValue<string>().Should().Be(FilterAISearch.Timeout);
        body["track_total_hits"]!.GetValue<bool>().Should().BeTrue();
        body["_source"]!.AsArray().Select(v => v!.GetValue<string>()).Should().Equal(FilterAISearch.SourceFields);
        body.ContainsKey("aggs").Should().BeFalse();
    }

    [Fact]
    public void SearchBodyUnwrapsAQueryAndDefaultsToMatchAll()
    {
        FilterAISearch.BuildSearchBody(Args("{\"query\":{\"query\":{\"term\":{\"id\":1}}}}"))["query"]!.ToJsonString()
            .Should().Be("{\"term\":{\"id\":1}}");
        FilterAISearch.BuildSearchBody(Args("{}"))["query"]!.ToJsonString().Should().Be("{\"match_all\":{}}");
    }

    [Theory]
    [InlineData("{\"query\":{\"script\":{\"script\":\"1\"}}}", "script")]
    [InlineData("{\"query\":{\"bool\":{\"must\":[{\"wrapper\":{\"query\":\"e30=\"}}]}}}", "wrapper")]
    [InlineData("{\"query\":{\"terms\":{\"id\":{\"index\":\"users\",\"id\":\"1\",\"path\":\"x\"}}}}", "index")]
    public void SearchRejectsForbiddenKeys(string args, string key)
    {
        var act = () => FilterAISearch.BuildSearchBody(Args(args));

        act.Should().Throw<FilterAIException>().WithMessage($"*'{key}'*");
    }

    [Fact]
    public void AggregatesUseNestedPathsAndCapTheSize()
    {
        var flat = FilterAISearch.BuildSearchBody(Args("{\"aggregate\":\"source.code\",\"aggregateSize\":900}"));
        flat["aggs"]!.ToJsonString().Should().Be("{\"values\":{\"terms\":{\"field\":\"source.code\",\"size\":50}}}");

        var nested = FilterAISearch.BuildSearchBody(Args("{\"aggregate\":\"tags.code\"}"));
        nested["aggs"]!.ToJsonString().Should().Be("{\"values\":{\"nested\":{\"path\":\"tags\"},\"aggs\":{\"values\":{\"terms\":{\"field\":\"tags.code\",\"size\":20}}}}}");

        var act = () => FilterAISearch.BuildSearchBody(Args("{\"aggregate\":\"doc['x'].value\"}"));
        act.Should().Throw<FilterAIException>();
    }

    [Fact]
    public void SummarizeKeepsTheTotalStoriesAndValues()
    {
        var response = "{\"hits\":{\"total\":{\"value\":10000,\"relation\":\"gte\"},\"hits\":[{\"_source\":{\"id\":5,\"headline\":\"H\",\"source\":{\"name\":\"CBC\",\"code\":\"CBC\"},\"mediaType\":{\"name\":\"Online\"},\"publishedOn\":\"2026-10-19\",\"body\":\"long\"}}]}," +
            "\"aggregations\":{\"values\":{\"doc_count\":4,\"values\":{\"buckets\":[{\"key\":\"HLTH\",\"doc_count\":3},{\"key\":7,\"doc_count\":1}]}}}}";

        var summary = FilterAISearch.Summarize(response);

        summary["total"]!.GetValue<long>().Should().Be(10000);
        summary["totalIsLowerBound"]!.GetValue<bool>().Should().BeTrue();
        summary["stories"]!.ToJsonString().Should().Be("[{\"id\":5,\"headline\":\"H\",\"source\":\"CBC\",\"sourceCode\":\"CBC\",\"mediaType\":\"Online\",\"publishedOn\":\"2026-10-19\"}]");
        summary["values"]!.ToJsonString().Should().Be("[{\"value\":\"HLTH\",\"count\":3},{\"value\":\"7\",\"count\":1}]");
    }

    [Fact]
    public void ExtendedQueryIsNormalized()
    {
        FilterAISearch.NormalizeExtendedQuery(null).Should().BeNull();
        FilterAISearch.NormalizeExtendedQuery(JsonNode.Parse("{\"must\":[]}")).Should().BeNull();

        var normalized = FilterAISearch.NormalizeExtendedQuery(JsonNode.Parse("{\"query\":{\"bool\":{\"must_not\":{\"term\":{\"id\":1}},\"minimum_should_match\":1}}}"));
        normalized!.ToJsonString().Should().Be("{\"must_not\":[{\"term\":{\"id\":1}}],\"minimum_should_match\":1}");
    }

    [Fact]
    public void SearchDateRangesAreInPacificTime()
    {
        var body = FilterAISearch.BuildSearchBody(Args("{\"query\":{\"bool\":{\"filter\":[{\"range\":{\"publishedOn\":{\"gte\":\"now/d\"}}},{\"range\":{\"postedOn\":{\"gte\":\"now-1d/d\",\"time_zone\":\"UTC\"}}},{\"range\":{\"sentiment\":{\"gte\":1}}}]}}}"));

        body["query"]!.ToJsonString().Should().Be("{\"bool\":{\"filter\":[{\"range\":{\"publishedOn\":{\"gte\":\"now/d\",\"time_zone\":\"US/Pacific\"}}},{\"range\":{\"postedOn\":{\"gte\":\"now-1d/d\",\"time_zone\":\"UTC\"}}},{\"range\":{\"sentiment\":{\"gte\":1}}}]}}");
    }

    [Fact]
    public void ExtendedQueryDateRangesAreInPacificTime()
    {
        var normalized = FilterAISearch.NormalizeExtendedQuery(JsonNode.Parse("{\"filter\":[{\"range\":{\"publishedOn\":{\"gte\":\"2026-10-22\"}}}]}"));

        normalized!.ToJsonString().Should().Be("{\"filter\":[{\"range\":{\"publishedOn\":{\"gte\":\"2026-10-22\",\"time_zone\":\"US/Pacific\"}}}]}");
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("{\"match\":{\"headline\":\"x\"}}")]
    [InlineData("{\"must\":[\"text\"]}")]
    [InlineData("{\"filter\":[{\"script\":{}}]}")]
    public void ExtendedQueryRejectsOtherShapes(string json)
    {
        var act = () => FilterAISearch.NormalizeExtendedQuery(JsonNode.Parse(json));

        act.Should().Throw<FilterAIException>();
    }

    [Fact]
    public void ValidationBodyMatchesHowTheBrowserMergesIt()
    {
        var body = FilterAISearch.BuildValidationBody(JsonNode.Parse("{\"must\":[{\"match_all\":{}}]}")!.AsObject());

        body.ToJsonString().Should().Be("{\"query\":{\"bool\":{\"filter\":[{\"bool\":{\"must\":[{\"match_all\":{}}],\"_name\":\"extendedQuery\"}}]}}}");
    }

    [Fact]
    public void SearchCollapsesAndSummarizeKeepsTheGroup()
    {
        var body = FilterAISearch.BuildSearchBody(Args("{\"collapse\":\"analysis.primaryTopic\"}"));
        body["collapse"]!.ToJsonString().Should().Be("{\"field\":\"analysis.primaryTopic\"}");

        var act = () => FilterAISearch.BuildSearchBody(Args("{\"collapse\":\"a b\"}"));
        act.Should().Throw<FilterAIException>();

        var summary = FilterAISearch.Summarize("{\"hits\":{\"total\":{\"value\":4},\"hits\":[{\"_source\":{\"id\":1,\"headline\":\"H\"},\"fields\":{\"analysis.primaryTopic\":[\"Housing\"]}}]}}");
        summary["stories"]!.ToJsonString().Should().Be("[{\"id\":1,\"headline\":\"H\",\"group\":\"Housing\"}]");
    }
}
