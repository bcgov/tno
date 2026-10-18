using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.DAL;
using TNO.DAL.Analysis;
using TNO.DAL.Scoring;
using TNO.Entities;

namespace TNO.Test.DAL.Scoring;

/// <summary>
/// Tests which content may be given a topic automatically, which topics count in Event of the
/// Day, and that Content-Analysis only gives a topic to content whose source scores topics.
/// </summary>
public class TopicEligibilityTest : IDisposable
{
    #region Variables
    private readonly TNOContext _context = DbContextHelper.Build();
    private readonly Topic _systemTopic = new("Not Applicable") { Id = 1, IsSystem = true };
    private readonly Topic _topic = new("Municipal elections") { Id = 2 };
    private readonly ContentAnalysisSettings _settings = new(null, new HashSet<int>(), new HashSet<int>(), TopicPopulationMode.ExistingOnly);
    #endregion

    #region Constructors
    public TopicEligibilityTest()
    {
        _context.Sources.AddRange(
            new Source("NONE", "Not scored", 1) { Id = 1 },
            new Source("FLAG", "Flag only", 1) { Id = 2, UseInTopics = true },
            new Source("DFLT", "Default score", 1) { Id = 3, UseInTopics = true, TopicDefaultScore = 20 },
            new Source("RULE", "Rule only", 1) { Id = 4 },
            new Source("ZERO", "Default score zero", 1) { Id = 5, UseInTopics = true, TopicDefaultScore = 0 });
        _context.Series.Add(new Series("Program", 4) { Id = 1, UseInTopics = true });
        _context.TopicScoreRules.Add(new TopicScoreRule(4, (int?)null, TimeSpan.Zero, TimeSpan.FromHours(23), 40, 0));
        _context.Topics.AddRange(_systemTopic, _topic);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }
    #endregion

    #region Helpers
    private Content AddContent(int sourceId, ContentTopic? topic = null)
    {
        var content = new Content("uid", "headline", "SRC", sourceId, ContentType.PrintContent, 1, 1)
        {
            Body = "<p>Test content.</p>",
            PublishedOn = new DateTime(2026, 10, 10, 18, 0, 0, DateTimeKind.Utc),
        };
        _context.Contents.Add(content);
        _context.SaveChanges();
        if (topic != null)
        {
            topic.ContentId = content.Id;
            _context.ContentTopics.Add(topic);
            _context.SaveChanges();
        }
        _context.ChangeTracker.Clear();
        return _context.Contents.Include(c => c.TopicsManyToMany).First(c => c.Id == content.Id);
    }

    private void Populate(Content content)
    {
        var analysis = new ContentAnalysis(content.Id, "hash");
        new AnalysisPopulator(_context, _settings, content, analysis).Populate(new AnalysisResultModel
        {
            ContentId = content.Id,
            Processes = AnalysisProcess.Topics,
            PrimaryTopic = _topic.Name,
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Tests
    [Theory]
    [InlineData(1, null, false)] // The source does not use topics.
    [InlineData(2, null, false)] // The source uses topics, but has no default score or rule.
    [InlineData(3, null, true)] // The source uses topics and has a default score.
    [InlineData(4, null, false)] // The source has a rule, but neither it nor the series uses topics.
    [InlineData(4, 1, true)] // The series uses topics and the source has a rule.
    public void IsTopicEligibleRequiresTopicsAndScoring(int sourceId, int? seriesId, bool expected)
    {
        _context.IsTopicEligible(ContentType.PrintContent, sourceId, seriesId).Should().Be(expected);
    }

    [Fact]
    public void ImageContentIsNeverTopicEligible()
    {
        _context.IsTopicEligible(ContentType.Image, 3, null).Should().BeFalse();
    }

    [Fact]
    public void ReportableTopicsAreScoredAndNotTheSystemTopic()
    {
        TopicScoreCalculator.HasReportableTopics(Array.Empty<(bool, int)>()).Should().BeFalse();
        TopicScoreCalculator.HasReportableTopics(new[] { (false, 20) }).Should().BeTrue();
        TopicScoreCalculator.HasReportableTopics(new[] { (false, 0) }).Should().BeFalse();
        TopicScoreCalculator.HasReportableTopics(new[] { (true, 20) }).Should().BeFalse();
        TopicScoreCalculator.HasReportableTopics(new[] { (false, 20), (false, 0) }).Should().BeFalse();
    }

    [Fact]
    public void AnalysisDoesNotGiveATopicToContentFromASourceWithoutScoring()
    {
        var content = AddContent(1);
        Populate(content);
        content.TopicsManyToMany.Should().BeEmpty();
    }

    [Fact]
    public void AnalysisGivesATopicToContentFromAScoredSource()
    {
        var content = AddContent(3);
        Populate(content);
        content.TopicsManyToMany.Should().ContainSingle(t => t.TopicId == _topic.Id);
    }

    [Fact]
    public void AnalysisReplacesTheCalculatedPlaceholder()
    {
        var content = AddContent(3, new ContentTopic(0, _systemTopic.Id, 20));
        Populate(content);
        content.TopicsManyToMany.Should().ContainSingle(t => t.TopicId == _topic.Id);
    }

    [Theory]
    [InlineData(3, 0, true)] // "Mark Not Applicable" saves a zero score, overriding the calculated one.
    [InlineData(3, 20, true)] // An editor overrode the score.
    [InlineData(5, 0, false)] // A zero score that is not overridden.
    public void AnalysisKeepsAnEditorsNotApplicable(int sourceId, int score, bool isOverridden)
    {
        var content = AddContent(sourceId, new ContentTopic(0, _systemTopic.Id, score) { IsScoreOverridden = isOverridden });
        Populate(content);
        content.TopicsManyToMany.Should().ContainSingle(t => t.TopicId == _systemTopic.Id);
    }
    #endregion
}
