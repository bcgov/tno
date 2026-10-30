using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TNO.Core.Reporting;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

public class ReportSizeEstimateTest
{
    [Fact]
    public void EstimateCountsSectionOccurrencesWithoutTrackingOrLoadingContent()
    {
        using var helper = new TestHelper();
        helper.Build(services => services.AddTNOContext("report-size-" + Guid.NewGuid()));
        var context = helper.Provider.GetRequiredService<TNOContext>();
        var content = new Content("story", "Headline", "Source", ContentType.PrintContent, 1, 1)
        {
            Id = 1,
            Body = "Story body",
            Summary = "Summary",
        };
        context.Contents.Add(content);
        context.ReportInstanceContents.AddRange(
            new ReportInstanceContent(31, 1, "first"),
            new ReportInstanceContent(31, 1, "second"),
            new ReportInstanceContent(31, 1, "disabled"),
            new ReportInstanceContent(32, 1, "first"));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var service = new ReportInstanceService(context, new ClaimsPrincipal(), helper.Provider, NullLogger<ReportInstanceService>.Instance);

        var expected = 2L * (ReportSizeLimit.StoryOverheadCharacters + content.Body.Length + content.Summary.Length + content.Headline.Length);
        Assert.Equal(expected, service.EstimateReportSize(31, ["first", "second"]));
        Assert.Equal(0, service.EstimateReportSize(31, []));
        Assert.Equal(0, service.EstimateReportSize(99, ["first"]));
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(4, context.ReportInstanceContents.Count());

        // An accumulated report can exceed the budget on markup alone, even with short stories.
        var sections = Enumerable.Range(0, ReportSizeLimit.MaximumCharacters / ReportSizeLimit.StoryOverheadCharacters + 1)
            .Select(i => $"section-{i}").ToArray();
        context.ReportInstanceContents.AddRange(sections.Select(section => new ReportInstanceContent(33, 1, section)));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        Assert.True(service.EstimateReportSize(33, sections) > ReportSizeLimit.MaximumCharacters);
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
