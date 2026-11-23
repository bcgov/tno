using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

public class ReportInstanceContentUpdateTest
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RemovedOrMovedPrivateStoriesRemainAvailableForOtherReports(bool includeContent, bool moveSection)
    {
        using var helper = new TestHelper();
        helper.Build(services => services.AddTNOContext("private-report-content-" + Guid.NewGuid()));
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var story = new Content("private-story", "Private headline", "Source", ContentType.Internet, 1, 1)
        { Id = 1, IsPrivate = true, OwnerId = 1, Status = ContentStatus.Published };
        context.Contents.Add(story);
        context.ReportInstances.AddRange(new ReportInstance(31, 12, 1), new ReportInstance(32, 13, 1));
        context.ReportInstanceContents.Add(new ReportInstanceContent(31, 1, "first", 0));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var service = new ReportInstanceService(context, new ClaimsPrincipal(), helper.Provider, NullLogger<ReportInstanceService>.Instance);

        // Retain a second story so both hydrated and ID-only report payloads exercise removal.
        context.Contents.Add(new Content("other-story", "Other headline", "Source", ContentType.Internet, 1, 1) { Id = 2 });
        context.ReportInstanceContents.Add(new ReportInstanceContent(31, 2, "first", 1));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var retained = new ReportInstanceContent(31, 2, "first", 0)
        {
            Content = includeContent ? context.Contents.Find(2L) : null,
        };
        var update = new ReportInstance(31, 12, 1, [retained]);
        if (moveSection)
            update.ContentManyToMany.Add(new ReportInstanceContent(31, 1, "second", 0)
            { Content = includeContent ? context.Contents.Find(1L) : null });
        context.ChangeTracker.Clear();
        service.UpdateAndSave(update, true);
        context.ChangeTracker.Clear();

        var saved = Assert.Single(context.Contents.Where(c => c.Id == 1));
        Assert.True(saved.IsPrivate);
        Assert.Equal(1, saved.OwnerId);
        Assert.Equal(ContentStatus.Published, saved.Status);
        Assert.Empty(context.TakeIndexRequests());
        Assert.False(context.ReportInstanceContents.Any(c => c.InstanceId == 31 && c.ContentId == 1 && c.SectionName == "first"));
        Assert.Equal(moveSection, context.ReportInstanceContents.Any(c => c.InstanceId == 31 && c.ContentId == 1 && c.SectionName == "second"));

        // The owner can reuse the same article in another report after removing it.
        service.UpdateAndSave(new ReportInstance(32, 13, 1, [new ReportInstanceContent(32, 1, "first", 0)]), true);
        context.ChangeTracker.Clear();
        Assert.True(context.ReportInstanceContents.Any(c => c.InstanceId == 32 && c.ContentId == 1));
        Assert.True(context.Contents.Any(c => c.Id == 1));
    }

    [Fact]
    public void UpdatingStoriesAddsRemovesAndReordersByContentAndSection()
    {
        using var helper = new TestHelper();
        helper.Build(services => services.AddTNOContext("report-instance-update-" + Guid.NewGuid()));
        var context = helper.Provider.GetRequiredService<TNOContext>();
        foreach (var id in new long[] { 1, 2, 3 })
            context.Contents.Add(new Content($"story-{id}", $"Headline {id}", "Source", ContentType.PrintContent, 1, 1) { Id = id });
        context.ReportInstances.Add(new ReportInstance(31, 12, 1));
        context.ReportInstanceContents.AddRange(
            new ReportInstanceContent(31, 1, "first", 0),
            new ReportInstanceContent(31, 2, "first", 1),
            // The same story in a second section.
            new ReportInstanceContent(31, 2, "second", 0));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var service = new ReportInstanceService(context, new ClaimsPrincipal(), helper.Provider, NullLogger<ReportInstanceService>.Instance);

        // Story 1 is removed, story 2 moves in the first section, story 3 is added.
        var update = new ReportInstance(31, 12, 1, [
            new ReportInstanceContent(31, 2, "first", 5),
            new ReportInstanceContent(31, 2, "second", 0),
            new ReportInstanceContent(31, 3, "first", 6),
        ]);
        service.Update(update, true);
        service.CommitTransaction();
        context.ChangeTracker.Clear();

        var saved = context.ReportInstanceContents.Where(c => c.InstanceId == 31)
            .OrderBy(c => c.SectionName).ThenBy(c => c.ContentId)
            .Select(c => new { c.ContentId, c.SectionName, c.SortOrder }).ToArray();
        Assert.Equal([
            new { ContentId = 2L, SectionName = "first", SortOrder = 5 },
            new { ContentId = 3L, SectionName = "first", SortOrder = 6 },
            new { ContentId = 2L, SectionName = "second", SortOrder = 0 },
        ], saved);
    }

    [Fact]
    public void LinkingStoriesWithoutTheirContentSkipsTheOnesThatDoNotExist()
    {
        using var helper = new TestHelper();
        helper.Build(services => services.AddTNOContext("report-instance-link-" + Guid.NewGuid()));
        var context = helper.Provider.GetRequiredService<TNOContext>();
        foreach (var id in new long[] { 1, 2, 3 })
            context.Contents.Add(new Content($"story-{id}", $"Headline {id}", "Source", ContentType.PrintContent, 1, 1) { Id = id });
        context.ReportInstances.Add(new ReportInstance(31, 12, 1));
        context.ReportInstanceContents.Add(new ReportInstanceContent(31, 1, "first", 0));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var service = new ReportInstanceService(context, new ClaimsPrincipal(), helper.Provider, NullLogger<ReportInstanceService>.Instance);

        // Story 2 is added to both sections and story 3 to one. Story 99 was removed after it was found.
        var update = new ReportInstance(31, 12, 1, [
            new ReportInstanceContent(31, 1, "first", 0),
            new ReportInstanceContent(31, 2, "first", 1),
            new ReportInstanceContent(31, 2, "second", 0),
            new ReportInstanceContent(31, 3, "second", 1),
            new ReportInstanceContent(31, 99, "first", 2),
        ]);
        service.Update(update, true);
        service.CommitTransaction();
        context.ChangeTracker.Clear();

        var saved = context.ReportInstanceContents.Where(c => c.InstanceId == 31)
            .OrderBy(c => c.SectionName).ThenBy(c => c.ContentId)
            .Select(c => new { c.ContentId, c.SectionName }).ToArray();
        Assert.Equal([
            new { ContentId = 1L, SectionName = "first" },
            new { ContentId = 2L, SectionName = "first" },
            new { ContentId = 2L, SectionName = "second" },
            new { ContentId = 3L, SectionName = "second" },
        ], saved);
    }
}
