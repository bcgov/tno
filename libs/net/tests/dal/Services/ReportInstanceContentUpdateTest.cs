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
}
