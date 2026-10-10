using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TNO.DAL;
using TNO.DAL.Config;
using TNO.DAL.Extensions;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.Test.DAL.Scoring;

/// <summary>
/// Tests that content without a topic is given the system "Not Applicable" topic once a rule or
/// its source default scores it, both when its scoring inputs change and when an editor saves it.
/// </summary>
public class TopicScoreSystemTopicTest : IDisposable
{
    #region Variables
    private readonly TNOContext _context = DbContextHelper.Build();
    private readonly ServiceProvider _services = new ServiceCollection().BuildServiceProvider();
    private readonly Source _source;
    private readonly Topic _systemTopic;
    private readonly Topic _topic;
    private readonly TopicScoreRule _timeRule;

    // 09:36 and 11:00 Pacific (daylight time).
    private static readonly DateTime BeforeRule = new(2026, 10, 10, 16, 36, 0, DateTimeKind.Utc);
    private static readonly DateTime InRule = new(2026, 10, 10, 18, 0, 0, DateTimeKind.Utc);
    #endregion

    #region Constructors
    public TopicScoreSystemTopicTest()
    {
        // No default score, so only the time rule scores content.
        _source = new Source("CKNW", "CKNW", 1) { Id = 1, UseInTopics = true };
        _systemTopic = new Topic("Not Applicable") { Id = 1, IsSystem = true };
        _topic = new Topic("Ambulance waits") { Id = 2 };
        _timeRule = new TopicScoreRule(10, _source.Id, null, TimeSpan.FromHours(10), TimeSpan.FromHours(15), 30, 0);
        _context.Sources.Add(_source);
        _context.Topics.AddRange(_systemTopic, _topic);
        _context.TopicScoreRules.Add(_timeRule);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }
    #endregion

    #region Helpers
    private Content AddContent(DateTime publishedOn, int sourceId = 1)
    {
        var content = new Content("uid", "headline", "CKNW", sourceId, ContentType.AudioVideo, 1, 1)
        {
            Body = "<p>Test content.</p>",
            PublishedOn = publishedOn,
        };
        _context.Contents.Add(content);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return content;
    }

    private ContentTopic[] GetTopics(long contentId) => _context.ContentTopics.AsNoTracking().Where(t => t.ContentId == contentId).ToArray();

    private TopicScoreService CreateService() => new(_context, new ClaimsPrincipal(), _services,
        Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _services.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Tests
    [Fact]
    public void ChangingInputsToMatchARuleAddsTheSystemTopic()
    {
        var content = AddContent(BeforeRule);
        GetTopics(content.Id).Should().BeEmpty();

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.PublishedOn = InRule;
        _context.SaveChanges();

        var topic = GetTopics(content.Id).Should().ContainSingle().Subject;
        topic.TopicId.Should().Be(_systemTopic.Id);
        topic.Score.Should().Be(30);
        topic.ScoreRuleId.Should().Be(_timeRule.Id);
        topic.IsScoreOverridden.Should().BeFalse();
    }

    [Fact]
    public void ChangingInputsWithoutAMatchAddsNoTopic()
    {
        var content = AddContent(BeforeRule);

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.PublishedOn = BeforeRule.AddHours(7);
        _context.SaveChanges();

        GetTopics(content.Id).Should().BeEmpty();
    }

    [Fact]
    public void ChangingInputsOfIneligibleContentAddsNoTopic()
    {
        _context.Sources.Add(new Source("Off", "OFF", 1) { Id = 2, UseInTopics = false });
        _context.TopicScoreRules.Add(new TopicScoreRule(11, 2, null, TimeSpan.FromHours(10), TimeSpan.FromHours(15), 30, 0));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        var content = AddContent(BeforeRule, 2);

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.PublishedOn = InRule;
        _context.SaveChanges();

        GetTopics(content.Id).Should().BeEmpty();
    }

    [Fact]
    public void UnrelatedChangesAddNoTopic()
    {
        // Content saved before its rule existed has no topic.
        var content = AddContent(InRule);

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.Headline = "A new headline";
        _context.SaveChanges();

        GetTopics(content.Id).Should().BeEmpty();
    }

    [Fact]
    public void RemovingTheSystemTopicWhileChangingInputsKeepsItRemoved()
    {
        var content = AddContent(InRule);
        _context.ContentTopics.Add(new ContentTopic(content.Id, _systemTopic.Id, 30) { ScoreRuleId = _timeRule.Id });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.PublishedOn = InRule.AddMinutes(30);
        _context.ContentTopics.Remove(_context.ContentTopics.Single(t => t.ContentId == content.Id));
        _context.SaveChanges();

        GetTopics(content.Id).Should().BeEmpty();
    }

    [Fact]
    public void ChangingInputsKeepsAnOverriddenScore()
    {
        var content = AddContent(BeforeRule);
        _context.ContentTopics.Add(new ContentTopic(content.Id, _topic.Id, 50) { IsScoreOverridden = true });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var tracked = _context.Contents.Single(c => c.Id == content.Id);
        tracked.PublishedOn = InRule;
        _context.SaveChanges();

        var topic = GetTopics(content.Id).Should().ContainSingle().Subject;
        topic.TopicId.Should().Be(_topic.Id);
        topic.Score.Should().Be(50);
    }

    [Fact]
    public void EditorSaveOfUnchangedContentAddsTheSystemTopicOnceARuleScoresIt()
    {
        // The content was saved before its rule existed, so it has no topic.
        _context.TopicScoreRules.Remove(_context.TopicScoreRules.Single(r => r.Id == _timeRule.Id));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        var content = AddContent(InRule);
        GetTopics(content.Id).Should().BeEmpty();
        _context.TopicScoreRules.Add(new TopicScoreRule(12, _source.Id, null, TimeSpan.FromHours(10), TimeSpan.FromHours(15), 30, 0));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // The editor saves the story without changing it.
        var saved = _context.Contents.AsNoTracking().Single(c => c.Id == content.Id);
        CreateService().AddSystemTopicWhenScored(saved);
        // Apply the save the way ContentService.UpdateAndSave does.
        var original = _context.Contents.Single(c => c.Id == content.Id);
        _context.UpdateContext(original, saved);
        _context.SaveChanges();

        var topic = GetTopics(content.Id).Should().ContainSingle().Subject;
        topic.TopicId.Should().Be(_systemTopic.Id);
        topic.Score.Should().Be(30);
        topic.ScoreRuleId.Should().Be(12);
    }

    [Fact]
    public void EditorSaveKeepsAnExistingTopic()
    {
        var content = AddContent(InRule);
        var saved = _context.Contents.AsNoTracking().Include(c => c.TopicsManyToMany).Single(c => c.Id == content.Id);
        saved.TopicsManyToMany.Clear();
        saved.TopicsManyToMany.Add(new ContentTopic(content.Id, _topic.Id, 0));

        CreateService().AddSystemTopicWhenScored(saved);

        saved.TopicsManyToMany.Should().ContainSingle(t => t.TopicId == _topic.Id);
    }

    [Fact]
    public void EditorSaveWithoutAMatchAddsNoTopic()
    {
        var content = AddContent(BeforeRule);
        var saved = _context.Contents.AsNoTracking().Single(c => c.Id == content.Id);

        CreateService().AddSystemTopicWhenScored(saved);

        saved.TopicsManyToMany.Should().BeEmpty();
    }
    #endregion
}
