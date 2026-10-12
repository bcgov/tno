using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.DAL;
using TNO.DAL.Helpers;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

public class EventOfDayReportTest : IDisposable
{
    private readonly TestHelper _helper = new();
    private TNOContext Context => _helper.Provider.GetRequiredService<TNOContext>();
    private IReportInstanceService Service => _helper.Provider.GetRequiredService<IReportInstanceService>();

    public EventOfDayReportTest()
    {
        _helper.Build(services =>
        {
            services.AddTNOContext(Guid.NewGuid().ToString());
            services.AddPrincipalForRole("editor");
            services.AddMockSingleton<ILogger<ReportInstanceService>>();
            services.AddSingleton<IReportInstanceService, ReportInstanceService>();
        });
    }

    [Theory]
    [InlineData("2026-10-11", "2026-10-11T07:00:00Z", 24)]
    [InlineData("2026-03-08", "2026-03-08T08:00:00Z", 23)]
    [InlineData("2025-11-02", "2025-11-02T07:00:00Z", 25)]
    public void DayBoundariesUseBcTimeAndDaylightSaving(string date, string expectedStart, int hours)
    {
        var (start, end) = EventOfDayReportQuery.GetDayRange(DateOnly.Parse(date));
        Assert.Equal(DateTime.Parse(expectedStart).ToUniversalTime(), start);
        Assert.Equal(hours, (end - start).TotalHours);
    }

    [Theory]
    [InlineData("2026-10-11", "2026-10-10T17:00:00Z", "2026-10-11T17:00:00Z")]
    [InlineData("2026-03-08", "2026-03-07T18:00:00Z", "2026-03-08T17:00:00Z")]
    [InlineData("2026-10-11", null, "2026-10-12T06:59:59Z")]
    public void GeneratedPublicationTimeFollowsThePriorInstanceOrEndsTheDay(string date, string? template, string expected)
    {
        var result = EventOfDayReportQuery.GetPublishedOn(DateOnly.Parse(date), template == null ? null : DateTime.Parse(template).ToUniversalTime());
        Assert.Equal(DateTime.Parse(expected).ToUniversalTime(), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void GeneratingCreatesTheInstanceAsSentWithItsLinksAndRejectsADuplicateDay()
    {
        var (start, end) = EventOfDayReportQuery.GetDayRange(new DateOnly(2026, 10, 11));
        Context.AddRange(new MediaType("Print") { Id = 1 }, new License("Public", 0) { Id = 1 });
        for (var id = 1; id <= 2; id++)
            Context.Add(new Content(id.ToString(), "Story", "Source", ContentType.PrintContent, 1, 1) { Id = id });
        Context.SaveChanges();

        var saved = Service.AddReportOutput(
            new ReportInstance(1, 7) { PublishedOn = start.AddHours(10), Subject = "subject", Body = "body" }, start, end,
            new[] { new ReportInstanceContent(0, 1, "day"), new ReportInstanceContent(0, 2, "history", 1) });
        Context.ChangeTracker.Clear();

        var instance = Context.ReportInstances.Single(i => i.Id == saved.Id);
        Assert.Equal(start.AddHours(10), instance.PublishedOn);
        Assert.Equal(instance.PublishedOn, instance.SentOn);
        Assert.Equal(ReportStatus.Completed, instance.Status);
        Assert.Equal(7, instance.OwnerId);
        Assert.Equal("body", instance.Body);
        Assert.Equal(new long[] { 1, 2 }, Context.ReportInstanceContents.Where(c => c.InstanceId == saved.Id).OrderBy(c => c.ContentId).Select(c => c.ContentId));

        Assert.Throws<TNO.Core.Exceptions.BadRequestException>(() => Service.AddReportOutput(
            new ReportInstance(1, 7) { PublishedOn = start.AddHours(11), Body = "again" }, start, end, Array.Empty<ReportInstanceContent>()));
        Assert.Throws<TNO.Core.Exceptions.BadRequestException>(() => Service.AddReportOutput(
            new ReportInstance(2, 7) { PublishedOn = end, Body = "outside" }, start, end, Array.Empty<ReportInstanceContent>()));
        Assert.Single(Context.ReportInstances);
    }

    [Fact]
    public void HistoricalQueryKeepsAggregationsAndAnchorsDateMathWithoutMutatingTheFilter()
    {
        using var filter = JsonDocument.Parse("""
            {"size":1024,"_source":["id"],"query":{"bool":{"must":[
              {"range":{"publishedOn":{"gte":"now-28d/d","time_zone":"US/Pacific"}}},
              {"match":{"headline":"now"}}]}},
             "aggs":{"topics":{"terms":{"field":"topics.name","size":5}}}}
            """);
        var query = EventOfDayReportQuery.Create(filter, new DateTime(2026, 10, 11, 17, 0, 0, DateTimeKind.Utc));
        var must = query["query"]!["bool"]!["must"]![0]!["bool"]!["must"]!;
        Assert.Equal("2026-10-11T17:00:00.0000000Z||-28d/d", must[0]!["range"]!["publishedOn"]!["gte"]!.GetValue<string>());
        Assert.Equal("now", must[1]!["match"]!["headline"]!.GetValue<string>());
        Assert.Equal("2026-10-12T07:00:00.0000000Z", query["query"]!["bool"]!["filter"]![0]!["range"]!["publishedOn"]!["lt"]!.GetValue<string>());
        Assert.Equal(5, query["aggs"]!["topics"]!["terms"]!["size"]!.GetValue<int>());
        Assert.Null(query["_source"]);
        Assert.Contains("now-28d/d", filter.RootElement.GetRawText());
    }

    [Fact]
    public void HistoryUsesExclusiveEndAndReturnsAllInstancesInOrder()
    {
        var (start, end) = EventOfDayReportQuery.GetDayRange(new DateOnly(2026, 10, 11));
        Context.AddRange(
            new ReportInstance(1) { Id = 1, PublishedOn = start },
            new ReportInstance(1) { Id = 2, PublishedOn = end.AddTicks(-1) },
            new ReportInstance(1) { Id = 3, PublishedOn = end },
            new ReportInstance(2) { Id = 4, PublishedOn = start });
        Context.SaveChanges();
        Assert.Equal(new long[] { 2, 1 }, Service.FindInstancesInRange(1, start, end).Select(i => i.Id));
    }

    [Fact]
    public void SourceHistoryIncludesExcludedStoriesForReassignmentButNotOtherDatesOrPrivateContent()
    {
        var (start, end) = EventOfDayReportQuery.GetDayRange(new DateOnly(2026, 10, 11));
        var topic = new Topic("Not Applicable") { Id = 1, IsSystem = true };
        Context.AddRange(topic, new MediaType("Print") { Id = 1 }, new License("Public", 0) { Id = 1 });
        for (var id = 1; id <= 4; id++)
        {
            var story = new Content(id.ToString(), "Story", "Source", ContentType.PrintContent, 1, 1)
            {
                Id = id,
                PublishedOn = id == 3 ? end : start,
                IsPrivate = id == 4
            };
            if (id != 2) story.TopicsManyToMany.Add(new ContentTopic(id, topic.Id, 0));
            Context.Add(story);
        }
        Context.SaveChanges();
        Assert.Equal(new long[] { 1 }, Service.FindTopicContentInRange(1, start, end).Select(c => c.Id));
    }

    [Fact]
    public void SavingReplacesOnlyTargetLinksAndHtmlAndPreservesDeliveryAndSourceData()
    {
        var published = new DateTime(2026, 10, 11, 17, 0, 0, DateTimeKind.Utc);
        var instance = new ReportInstance(1)
        {
            Id = 65177,
            PublishedOn = published,
            SentOn = published.AddMinutes(1),
            Status = ReportStatus.Completed,
            Body = "old",
            Subject = "old subject"
        };
        Context.AddRange(instance, new ReportInstance(1) { Id = 65178 });
        for (var id = 1; id <= 3; id++)
            Context.Add(new Content(id.ToString(), "Original story", "Source", ContentType.PrintContent, 1, 1) { Id = id });
        Context.AddRange(new ReportInstanceContent(65177, 1, "day"), new ReportInstanceContent(65177, 2, "history"),
            new ReportInstanceContent(65178, 1, "day"));
        Context.SaveChanges();
        var expectedVersion = instance.Version;
        Service.SaveReportOutput(instance, expectedVersion, "new subject", "new body",
            new[] { new ReportInstanceContent(65177, 2, "history", 1), new ReportInstanceContent(65177, 3, "day") });
        Context.ChangeTracker.Clear();

        var saved = Context.ReportInstances.Single(i => i.Id == 65177);
        Assert.Equal("new body", saved.Body);
        Assert.Equal("new subject", saved.Subject);
        Assert.Equal(published, saved.PublishedOn);
        Assert.Equal(published.AddMinutes(1), saved.SentOn);
        Assert.Equal(ReportStatus.Completed, saved.Status);
        Assert.Equal(new long[] { 2, 3 }, Context.ReportInstanceContents.Where(c => c.InstanceId == 65177).OrderBy(c => c.ContentId).Select(c => c.ContentId));
        Assert.Single(Context.ReportInstanceContents.Where(c => c.InstanceId == 65178));
        Assert.Equal(3, Context.Contents.Count());
        Assert.All(Context.Contents, c => Assert.Equal("Original story", c.Headline));
        Assert.True(saved.Version > expectedVersion);
        Assert.Throws<DbUpdateConcurrencyException>(() => Service.SaveReportOutput(saved, expectedVersion, "stale", "stale", Array.Empty<ReportInstanceContent>()));
        Assert.Equal("new body", saved.Body);
        Assert.Equal(2, Context.ReportInstanceContents.Count(c => c.InstanceId == 65177));
    }

    public void Dispose() => _helper.Dispose();
}
