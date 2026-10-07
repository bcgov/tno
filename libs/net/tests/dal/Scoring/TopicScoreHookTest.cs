using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TNO.DAL;
using TNO.Entities;

namespace TNO.Test.DAL.Scoring;

/// <summary>
/// Tests that calculated topic scores follow their inputs through TNOContext.SaveChanges.
/// </summary>
public class TopicScoreHookTest : IDisposable
{
    #region Variables
    private readonly TNOContext _context = DbContextHelper.Build();
    private readonly Source _source;
    private readonly Topic _topic;
    private readonly TopicScoreRule _frontRule;
    #endregion

    #region Constructors
    public TopicScoreHookTest()
    {
        _source = new Source("Daily", "DLY", 1) { Id = 1, UseInTopics = true, TopicDefaultScore = 3 };
        _topic = new Topic("Ambulance waits") { Id = 2 };
        _frontRule = new TopicScoreRule(10, _source.Id, "Front", null, null, null, null, null, 10, 0);
        _context.Sources.Add(_source);
        _context.Topics.Add(_topic);
        _context.TopicScoreRules.Add(_frontRule);
        _context.TopicScoreRules.Add(new TopicScoreRule(11, _source.Id, null, null, null, true, null, null, 7, 1));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }
    #endregion

    #region Helpers
    private Content AddContent(string section, int score = 0, bool overridden = false)
    {
        var content = new Content("uid", "headline", "DLY", _source.Id, ContentType.PrintContent, 1, 1)
        {
            Section = section,
            Body = "<p>body</p>",
        };
        content.TopicsManyToMany.Add(new ContentTopic(0, _topic.Id, score) { IsScoreOverridden = overridden });
        _context.Contents.Add(content);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return content;
    }

    private ContentTopic GetTopic(long contentId) => _context.ContentTopics.AsNoTracking().Single(t => t.ContentId == contentId);

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Tests
    [Fact]
    public void NewContentIsScored()
    {
        var content = AddContent("Front");

        var topic = GetTopic(content.Id);
        topic.Score.Should().Be(10);
        topic.ScoreRuleId.Should().Be(_frontRule.Id);
    }

    [Fact]
    public void ChangingASectionRescores()
    {
        var content = AddContent("Front");

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.Section = "Sports";
        _context.SaveChanges();

        var topic = GetTopic(content.Id);
        topic.Score.Should().Be(3);
        topic.ScoreRuleId.Should().BeNull();
    }

    [Fact]
    public void AttachingAnImageRescores()
    {
        var content = AddContent("Sports");
        GetTopic(content.Id).Score.Should().Be(3);

        _context.FileReferences.Add(new FileReference(content.Id, "image/jpeg", "photo.jpg"));
        _context.SaveChanges();

        GetTopic(content.Id).Score.Should().Be(7);
    }

    [Fact]
    public void OverriddenScoresAreNotRecalculated()
    {
        var content = AddContent("Front", 50, true);

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.Section = "Sports";
        _context.SaveChanges();

        var topic = GetTopic(content.Id);
        topic.Score.Should().Be(50);
        topic.IsScoreOverridden.Should().BeTrue();
    }

    [Fact]
    public void ClearingAnOverrideRecalculates()
    {
        var content = AddContent("Front", 50, true);

        var tracked = _context.ContentTopics.Single(t => t.ContentId == content.Id);
        tracked.IsScoreOverridden = false;
        _context.SaveChanges();

        GetTopic(content.Id).Score.Should().Be(10);
    }

    [Fact]
    public void IneligibleContentIsNotScored()
    {
        var source = _context.Sources.Single(s => s.Id == _source.Id);
        source.UseInTopics = false;
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var content = AddContent("Front", 4);

        GetTopic(content.Id).Score.Should().Be(4);
    }

    [Fact]
    public void UnrelatedChangesDoNotRescore()
    {
        var content = AddContent("Front");
        var rule = _context.TopicScoreRules.Single(r => r.Id == _frontRule.Id);
        rule.Score = 99;
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.Headline = "A new headline";
        _context.SaveChanges();

        GetTopic(content.Id).Score.Should().Be(10);
    }
    #endregion
}
