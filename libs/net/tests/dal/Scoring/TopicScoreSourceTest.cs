using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.Test.DAL.Scoring;

public class TopicScoreSourceTest
{
    [Fact]
    public void ExistingRulesRemainVisibleAfterReopeningWithoutEnablingAutomaticScoring()
    {
        var database = Guid.NewGuid().ToString();
        using var services = new ServiceCollection().BuildServiceProvider();
        using (var context = DbContextHelper.Build(database))
        {
            context.Sources.AddRange(
                new Source("Existing rules", "OLD", 1) { Id = 1, UseInTopics = false },
                new Source("New test source", "NEW", 1) { Id = 2, UseInTopics = true },
                new Source("Unused", "OFF", 1) { Id = 3, UseInTopics = false },
                new Source("Series only", "SER", 1) { Id = 4, UseInTopics = false });
            context.Series.Add(new Series("Morning", 4) { Id = 1, UseInTopics = true });
            context.TopicScoreRules.Add(new TopicScoreRule(10, 1, null, null, null, null, null, null, 20, 0));
            context.SaveChanges();
        }

        // Each visit creates a new request/context, just like leaving and returning to the page.
        for (var visit = 0; visit < 2; visit++)
        {
            using var context = DbContextHelper.Build(database);
            var service = new TopicScoreService(context, new ClaimsPrincipal(), services,
                Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);
            var sources = service.FindSourceSummaries().ToArray();
            sources.Select(s => s.Id).Should().BeEquivalentTo(new[] { 1, 2, 4 });
            sources.Single(s => s.Id == 1).RuleCount.Should().Be(1);
            sources.Single(s => s.Id == 1).UseInTopics.Should().BeFalse();
            context.TopicScoreRules.Single().Score.Should().Be(20);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitRemovalSurvivesReopeningAndReaddingRestoresRules(bool enableSeries)
    {
        var database = Guid.NewGuid().ToString();
        using var services = new ServiceCollection().BuildServiceProvider();
        using (var context = DbContextHelper.Build(database))
        {
            context.Sources.Add(new Source("Existing rules", "OLD", 1)
            {
                Id = 1,
                UseInTopics = false,
                TopicDefaultScore = 7,
                Configuration = JsonDocument.Parse("{\"otherSetting\":{\"value\":42}}"),
            });
            context.Series.Add(new Series("Morning", 1) { Id = 1, UseInTopics = false });
            context.TopicScoreRules.Add(new TopicScoreRule(10, 1, null, null, null, null, null, null, 20, 0));
            context.SaveChanges();
            var service = new TopicScoreService(context, new ClaimsPrincipal(), services,
                Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);
            service.FindSourceSummaries().Should().ContainSingle();
            service.RemoveSource(1);
        }

        using (var context = DbContextHelper.Build(database))
        {
            var service = new TopicScoreService(context, new ClaimsPrincipal(), services,
                Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);
            service.FindSourceSummaries().Should().BeEmpty();
            context.TopicScoreRules.Single().Score.Should().Be(20);
            var source = context.Sources.Single();
            source.TopicDefaultScore.Should().Be(7);
            source.Configuration.RootElement.GetProperty("otherSetting").GetProperty("value").GetInt32().Should().Be(42);
            context.Cache.Select(c => c.Key).Should().Contain(new[] { "sources", "series", "lookups" });
            if (enableSeries) context.Series.Single().UseInTopics = true;
            else source.UseInTopics = true;
            context.SaveChanges();
        }

        using (var context = DbContextHelper.Build(database))
        {
            var service = new TopicScoreService(context, new ClaimsPrincipal(), services,
                Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);
            service.FindSourceSummaries().Single().RuleCount.Should().Be(1);
            service.RemoveSource(1);
            service.FindSourceSummaries().Should().BeEmpty();
        }
    }

    [Fact]
    public void RemovalDisablesSourceAndSeriesButPreservesRulesAndCanBeReversed()
    {
        using var context = DbContextHelper.Build();
        using var services = new ServiceCollection().BuildServiceProvider();
        var service = new TopicScoreService(context, new ClaimsPrincipal(), services,
            Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);
        var source = new Source("Daily", "DLY", 1) { Id = 1, UseInTopics = true, TopicDefaultScore = 7 };
        var other = new Source("Other", "OTH", 1) { Id = 2, UseInTopics = true };
        context.Sources.AddRange(source, other);
        context.Series.AddRange(new Series("Morning", 1) { Id = 1, UseInTopics = true },
            new Series("Other program", 2) { Id = 2, UseInTopics = true });
        context.TopicScoreRules.Add(new TopicScoreRule(10, 1, null, null, null, null, null, null, 20, 0));
        context.SaveChanges();

        service.RemoveSource(1);
        service.FindSourceSummaries().Select(s => s.Id).Should().Equal(2);
        context.Sources.Single(s => s.Id == 1).UseInTopics.Should().BeFalse();
        context.Series.Single(s => s.Id == 1).UseInTopics.Should().BeFalse();
        context.Series.Single(s => s.Id == 2).UseInTopics.Should().BeTrue();
        context.TopicScoreRules.Single().Score.Should().Be(20);
        context.Sources.Single(s => s.Id == 1).TopicDefaultScore.Should().Be(7);

        service.RemoveSource(1); // Retrying is safe.
        source.UseInTopics = true;
        context.SaveChanges();
        service.FindSourceSummaries().Single(s => s.Id == 1).RuleCount.Should().Be(1);
    }

    [Fact]
    public void MissingSourceCannotChangeOtherSources()
    {
        using var context = DbContextHelper.Build();
        using var services = new ServiceCollection().BuildServiceProvider();
        var service = new TopicScoreService(context, new ClaimsPrincipal(), services,
            Options.Create(new TopicScoreOptions()), NullLogger<TopicScoreService>.Instance);
        var act = () => service.RemoveSource(999);
        act.Should().Throw<NoContentException>();
    }
}
