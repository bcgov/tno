using FluentAssertions;
using TNO.TemplateEngine.FilterAI;

namespace TNO.Test.AI;

/// <summary>
/// Tests for describing a listed story and why a query matched it.
/// </summary>
public class FilterAIStoryTest
{
    [Fact]
    public void SummarizeKeepsTheFieldsAFilterCanMatch()
    {
        var source =
            "{\"id\":7,\"headline\":\"Fire at the hospital\",\"byline\":\"A. Reporter\",\"contentType\":\"AudioVideo\",\"publishedOn\":\"2026-10-19T00:00:00Z\"," +
            "\"source\":{\"id\":1,\"code\":\"CBC\",\"name\":\"CBC News\"},\"mediaType\":{\"id\":4,\"name\":\"News Radio\"},\"series\":{\"id\":9,\"name\":\"Early Edition\"}," +
            "\"tags\":[{\"code\":\"HLTH\",\"name\":\"Health\"},{\"code\":\"FIN\"}],\"topics\":[{\"name\":\"Health care\"}],\"actions\":[{\"name\":\"Top Story\",\"value\":\"true\"}]," +
            "\"tonePools\":[{\"value\":-2}],\"summary\":\"short\",\"body\":\"0123456789\",\"versions\":{\"1\":{}},\"section\":null}";

        FilterAIStory.Summarize(source).ToJsonString().Should().Be(
            "{\"id\":7,\"headline\":\"Fire at the hospital\",\"byline\":\"A. Reporter\",\"contentType\":\"AudioVideo\",\"publishedOn\":\"2026-10-19T00:00:00Z\"," +
            "\"source\":\"CBC - CBC News (id 1)\",\"mediaType\":\"News Radio (id 4)\",\"series\":\"Early Edition (id 9)\",\"tags\":[\"HLTH\",\"FIN\"],\"topics\":[\"Health care\"]," +
            "\"actions\":[\"Top Story: true\"],\"tone\":-2,\"summary\":\"short\",\"body\":\"0123456789\"}");

        var capped = FilterAIStory.Summarize(source, maxTextChars: 4);
        capped["body"]!.GetValue<string>().Should().StartWith("0123").And.Contain("truncated");
    }

    [Fact]
    public void ExplainReasonsNamesTheMatchedConditionsInPlainWords()
    {
        var reasons = FilterAIStory.ExplainReasons(
            "{\"matched\":true,\"explanation\":{\"value\":3.2,\"description\":\"sum of:\",\"details\":[" +
            "{\"value\":2.0,\"description\":\"max of:\",\"details\":[{\"value\":2.0,\"description\":\"weight(headline:election in 44) [PerFieldSimilarity], result of:\",\"details\":[{\"value\":2.0,\"description\":\"score(freq=1.0), computed as boost * idf * tf from:\",\"details\":[{\"value\":1.1,\"description\":\"idf, computed as log(1 + (N - n + 0.5) / (n + 0.5)) from:\",\"details\":[]}]}]},{\"value\":0,\"description\":\"weight(body:election in 44)\",\"details\":[]}]}," +
            "{\"value\":1.0,\"description\":\"ConstantScore(sourceId:[5 TO 5])\",\"details\":[]}," +
            "{\"value\":0.2,\"description\":\"ConstantScore(publishedOn:[1760000000000 TO 9223372036854775807])\",\"details\":[]}," +
            "{\"value\":0.0,\"description\":\"match on required clause, product of:\",\"details\":[]}," +
            "{\"value\":1.0,\"description\":\"*:*\",\"details\":[]}]}}");

        reasons["matched"]!.GetValue<bool>().Should().BeTrue();
        reasons["reasons"]!.AsArray().Select(r => r!.GetValue<string>()).Should().Equal(
            "headline contains the term 'election'",
            "filter matched: sourceId:[5 TO 5]",
            "filter matched: publishedOn:[2025-10-09 08:53 UTC TO the end of time]",
            "every story matches (match_all)");
        reasons.ContainsKey("note").Should().BeFalse();
    }

    [Fact]
    public void ExplainReasonsSaysWhenTheQueryDoesNotMatch()
    {
        var reasons = FilterAIStory.ExplainReasons("{\"matched\":false,\"explanation\":{\"value\":0,\"description\":\"no match\",\"details\":[]}}");

        reasons["matched"]!.GetValue<bool>().Should().BeFalse();
        reasons["reasons"]!.AsArray().Should().BeEmpty();
        reasons["note"]!.GetValue<string>().Should().Contain("does not match");
    }

    [Theory]
    [InlineData("sum of:", null)]
    [InlineData("weight(FunctionScoreQuery(...))", null)]
    [InlineData("weight(source.name.keyword:CBC in 1) [PerFieldSimilarity]", "source.name.keyword contains the term 'CBC'")]
    [InlineData("ConstantScore(actions.name:Top Story)", "filter matched: actions.name:Top Story")]
    [InlineData("ToParentBlockJoinQuery (+actions.name:Top Story)", "ToParentBlockJoinQuery (+actions.name:Top Story)")]
    public void SimplifyTurnsScoringTermsIntoConditions(string description, string? expected)
    {
        FilterAIStory.Simplify(description).Should().Be(expected);
    }
}
