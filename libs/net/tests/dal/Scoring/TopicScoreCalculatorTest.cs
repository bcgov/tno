using FluentAssertions;
using TNO.DAL.Scoring;
using TNO.Entities;

namespace TNO.Test.DAL.Scoring;

/// <summary>
/// Tests for topic score rule evaluation.
/// </summary>
public class TopicScoreCalculatorTest
{
    #region Variables
    private const int SourceId = 1;
    private static readonly TimeZoneInfo Pacific = TNO.Core.Extensions.DateTimeExtensions.ResolveTimeZone("Pacific Standard Time");
    #endregion

    #region Helpers
    private static TopicScoreRule Rule(int id, int score, int sortOrder, Action<TopicScoreRule>? configure = null)
    {
        var rule = new TopicScoreRule(id, SourceId, null, null, null, null, null, null, score, sortOrder) { Section = null };
        configure?.Invoke(rule);
        return rule;
    }

    private static TopicScoreInput Input(
        string? section = null,
        string? page = null,
        int? seriesId = null,
        bool hasImage = false,
        DateTime? publishedOn = null,
        int characters = 0)
        => new(SourceId, seriesId, section, page, hasImage, publishedOn, characters);

    private static TopicScoreResult Calculate(TopicScoreInput input, int? defaultScore, params TopicScoreRule[] rules)
        => TopicScoreCalculator.Calculate(rules, defaultScore, input, Pacific);
    #endregion

    #region Precedence and defaults
    [Fact]
    public void FirstMatchBySortOrderWins()
    {
        var result = Calculate(Input(), null, Rule(2, 20, 1), Rule(1, 10, 0));

        result.Score.Should().Be(10);
        result.RuleId.Should().Be(1);
    }

    [Fact]
    public void NoMatchUsesSourceDefault()
    {
        var result = Calculate(Input(section: "Sports"), 7, Rule(1, 10, 0, r => r.Section = "News"));

        result.Score.Should().Be(7);
        result.RuleId.Should().BeNull();
        result.IsSourceDefault.Should().BeTrue();
    }

    [Fact]
    public void NoMatchWithoutDefaultScoresZero()
    {
        var result = Calculate(Input(section: "Sports"), null, Rule(1, 10, 0, r => r.Section = "News"));

        result.Score.Should().Be(0);
        result.IsSourceDefault.Should().BeFalse();
    }

    [Fact]
    public void OtherSourcesRulesAreIgnored()
    {
        var other = new TopicScoreRule(9, SourceId + 1, null, null, null, null, null, null, 99, 0);

        Calculate(Input(), null, other).Score.Should().Be(0);
    }

    [Fact]
    public void RuleWithoutConditionsMatchesEverything()
    {
        Calculate(Input(page: "A1", section: "News"), null, Rule(1, 5, 0)).Score.Should().Be(5);
    }

    [Fact]
    public void EvaluateAllExplainsLaterRules()
    {
        var result = TopicScoreCalculator.Calculate(new[] { Rule(1, 5, 0), Rule(2, 3, 1, r => r.Section = "News") }, null, Input(), Pacific, true);

        result.Evaluations.Should().HaveCount(2);
        result.Evaluations[1].IsMatch.Should().BeFalse();
    }
    #endregion

    #region Series and section
    [Fact]
    public void SeriesRuleRequiresSameSeries()
    {
        var rule = Rule(1, 10, 0, r => r.SeriesId = 3);

        Calculate(Input(seriesId: 3), null, rule).Score.Should().Be(10);
        Calculate(Input(seriesId: 4), null, rule).Score.Should().Be(0);
    }

    [Fact]
    public void ContentWithoutSeriesDoesNotMatchSeriesRule()
    {
        var result = Calculate(Input(), null, Rule(1, 10, 0, r => r.SeriesId = 3));

        result.Score.Should().Be(0);
        result.Evaluations[0].FailedCondition.Should().Be(TopicScoreCondition.Series);
    }

    [Fact]
    public void SectionIsTrimmedAndCaseInsensitive()
    {
        Calculate(Input(section: " front "), null, Rule(1, 10, 0, r => r.Section = "Front")).Score.Should().Be(10);
    }

    [Fact]
    public void ContentWithoutSectionDoesNotMatchSectionRule()
    {
        var result = Calculate(Input(section: ""), null, Rule(1, 10, 0, r => r.Section = "Front"));

        result.Evaluations[0].FailedCondition.Should().Be(TopicScoreCondition.Section);
    }
    #endregion

    #region Page
    [Theory]
    [InlineData("A1", 10)]
    [InlineData("a3", 10)]
    [InlineData("A4", 0)]
    [InlineData("B1", 0)]
    [InlineData("1", 0)]
    public void LetteredPageRange(string page, int expected)
    {
        Calculate(Input(page: page), null, Rule(1, 10, 0, r => { r.PageMin = "A1"; r.PageMax = "A3"; })).Score.Should().Be(expected);
    }

    [Fact]
    public void RuleWithoutPageMatchesLetteredPage()
    {
        Calculate(Input(page: "A1"), null, Rule(1, 10, 0, r => r.Section = "News"), Rule(2, 4, 1)).Score.Should().Be(4);
    }

    [Fact]
    public void ContentWithoutPageDoesNotMatchPageRule()
    {
        var result = Calculate(Input(page: ""), null, Rule(1, 10, 0, r => r.PageMin = "1"));

        result.Score.Should().Be(0);
        result.Evaluations[0].FailedCondition.Should().Be(TopicScoreCondition.Page);
    }

    [Fact]
    public void OpenEndedPageRange()
    {
        var rule = Rule(1, 10, 0, r => r.PageMin = "5");

        Calculate(Input(page: "12"), null, rule).Score.Should().Be(10);
        Calculate(Input(page: "4"), null, rule).Score.Should().Be(0);
    }
    #endregion

    #region Image
    [Fact]
    public void ImageRulePairsAreDistinguished()
    {
        var withImage = Rule(1, 10, 0, r => r.HasImage = true);
        var withoutImage = Rule(2, 5, 1, r => r.HasImage = false);

        Calculate(Input(hasImage: true), null, withImage, withoutImage).Score.Should().Be(10);
        Calculate(Input(hasImage: false), null, withImage, withoutImage).Score.Should().Be(5);
    }

    [Fact]
    public void HasImageFromAttachmentOrBody()
    {
        TopicScoreInput.From(SourceId, null, null, null, "<p>text</p>", null, true).HasImage.Should().BeTrue();
        TopicScoreInput.From(SourceId, null, null, null, "<p><img src=\"a.png\"/></p>", null, false).HasImage.Should().BeTrue();
        TopicScoreInput.From(SourceId, null, null, null, "<p>text</p>", null, false).HasImage.Should().BeFalse();
    }
    #endregion

    #region Time
    [Fact]
    public void TimeIsEvaluatedInTheConfiguredTimeZone()
    {
        // 14:30 UTC in July is 07:30 Pacific (daylight time).
        var publishedOn = new DateTime(2026, 7, 1, 14, 30, 0, DateTimeKind.Utc);
        var rule = Rule(1, 10, 0, r => { r.TimeMin = new TimeSpan(7, 0, 0); r.TimeMax = new TimeSpan(8, 0, 0); });

        Calculate(Input(publishedOn: publishedOn), null, rule).Score.Should().Be(10);
    }

    [Fact]
    public void UnspecifiedKindIsTreatedAsUtc()
    {
        var publishedOn = new DateTime(2026, 7, 1, 14, 30, 0, DateTimeKind.Unspecified);
        var rule = Rule(1, 10, 0, r => { r.TimeMin = new TimeSpan(7, 0, 0); r.TimeMax = new TimeSpan(8, 0, 0); });

        Calculate(Input(publishedOn: publishedOn), null, rule).Score.Should().Be(10);
    }

    [Theory]
    [InlineData(23, 30, 10)]
    [InlineData(1, 0, 10)]
    [InlineData(12, 0, 0)]
    public void RangeWrapsMidnight(int hour, int minute, int expected)
    {
        var local = new DateTime(2026, 1, 15, hour, minute, 0, DateTimeKind.Unspecified);
        var publishedOn = TimeZoneInfo.ConvertTimeToUtc(local, Pacific);
        var rule = Rule(1, 10, 0, r => { r.TimeMin = new TimeSpan(22, 0, 0); r.TimeMax = new TimeSpan(2, 0, 0); });

        Calculate(Input(publishedOn: publishedOn), null, rule).Score.Should().Be(expected);
    }

    [Fact]
    public void ContentWithoutPublishTimeDoesNotMatchTimeRule()
    {
        var result = Calculate(Input(), null, Rule(1, 10, 0, r => r.TimeMin = new TimeSpan(1, 0, 0)));

        result.Evaluations[0].FailedCondition.Should().Be(TopicScoreCondition.Time);
    }
    #endregion

    #region Characters
    [Fact]
    public void CharacterCountUsesPlainText()
    {
        var input = TopicScoreInput.From(SourceId, null, null, null, "<p>Hello <b>world</b></p>", null, false);

        input.CharacterCount.Should().Be("Hello world".Length);
    }

    [Theory]
    [InlineData(99, 0)]
    [InlineData(100, 10)]
    [InlineData(500, 10)]
    [InlineData(501, 0)]
    public void CharacterRangeIsInclusive(int characters, int expected)
    {
        Calculate(Input(characters: characters), null, Rule(1, 10, 0, r => { r.CharacterMin = 100; r.CharacterMax = 500; })).Score.Should().Be(expected);
    }
    #endregion

    #region Eligibility and validation
    [Theory]
    [InlineData(ContentType.PrintContent, true, false, true)]
    [InlineData(ContentType.AudioVideo, false, true, true)]
    [InlineData(ContentType.PrintContent, false, false, false)]
    [InlineData(ContentType.Image, true, true, false)]
    public void Eligibility(ContentType contentType, bool sourceUseInTopics, bool seriesUseInTopics, bool expected)
    {
        TopicScoreCalculator.IsEligible(contentType, sourceUseInTopics, seriesUseInTopics).Should().Be(expected);
    }

    [Fact]
    public void ValidationRequiresSharedPagePrefix()
    {
        TopicScoreCalculator.Validate(Rule(1, 1, 0, r => { r.PageMin = "A1"; r.PageMax = "B2"; })).Should().NotBeEmpty();
        TopicScoreCalculator.Validate(Rule(1, 1, 0, r => { r.PageMin = "A1"; r.PageMax = "A12"; })).Should().BeEmpty();
    }

    [Fact]
    public void ValidationRejectsLongPagesNegativeScoresAndReversedRanges()
    {
        TopicScoreCalculator.Validate(Rule(1, 1, 0, r => r.PageMin = "ABCD12")).Should().NotBeEmpty();
        TopicScoreCalculator.Validate(Rule(1, -1, 0)).Should().NotBeEmpty();
        TopicScoreCalculator.Validate(Rule(1, 1, 0, r => { r.CharacterMin = 10; r.CharacterMax = 5; })).Should().NotBeEmpty();
        TopicScoreCalculator.Validate(Rule(1, 1, 0, r => { r.PageMin = "5"; r.PageMax = "2"; })).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidationAllowsOvernightTimes()
    {
        TopicScoreCalculator.Validate(Rule(1, 1, 0, r => { r.TimeMin = new TimeSpan(22, 0, 0); r.TimeMax = new TimeSpan(2, 0, 0); })).Should().BeEmpty();
    }
    #endregion

    #region Rule equality
    [Fact]
    public void RuleEqualityIncludesSeriesAndTimes()
    {
        var a = Rule(1, 1, 0);
        var b = Rule(1, 1, 0, r => r.SeriesId = 2);
        var c = Rule(1, 1, 0, r => r.TimeMin = new TimeSpan(1, 0, 0));

        a.Equals(b).Should().BeFalse();
        a.Equals(c).Should().BeFalse();
        a.Equals(Rule(1, 1, 0)).Should().BeTrue();
    }
    #endregion
}
