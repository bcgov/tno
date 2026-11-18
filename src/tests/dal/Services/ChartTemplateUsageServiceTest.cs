using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL.Services;

/// <summary>
/// A chart template is used by the reports whose sections link to it.
/// </summary>
public class ChartTemplateUsageServiceTest : IDisposable
{
    readonly TestHelper helper = new();

    public ChartTemplateUsageServiceTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext($"chart-template-usage-{Guid.NewGuid()}");
            services.AddPrincipalForRole("administrator");
            services.AddMockSingleton<ILogger<ChartTemplateService>>();
            services.AddSingleton<IChartTemplateService, ChartTemplateService>();
        });
    }

    private TNOContext Context => helper.Provider.GetRequiredService<TNOContext>();
    private IChartTemplateService Service => helper.Provider.GetRequiredService<IChartTemplateService>();

    [Fact]
    public void FindUsage_ListsEachReportOnceWithItsSections()
    {
        var context = Context;
        context.Users.Add(new User("owner", "owner@test.com") { Id = 7 });
        context.ChartTemplates.AddRange(new ChartTemplate(1, "used", "t"), new ChartTemplate(2, "other", "t"), new ChartTemplate(3, "offered", "t"));
        context.ReportTemplates.Add(new ReportTemplate("template", ReportType.Content, "subject", "body") { Id = 1 });
        context.Reports.AddRange(
            new Report(2, "B report", 1, 7) { IsEnabled = true },
            new Report(1, "A report", 1, null) { IsEnabled = false },
            new Report(3, "Unrelated", 1, 7));
        context.ReportSections.AddRange(
            new ReportSection(1, "guid-1", ReportSectionType.Content, 2) { SortOrder = 2, Settings = JsonDocument.Parse("""{"label":"Second"}""") },
            new ReportSection(2, "guid-2", ReportSectionType.Content, 2) { SortOrder = 1 },
            new ReportSection(3, "guid-3", ReportSectionType.Content, 1),
            new ReportSection(4, "guid-4", ReportSectionType.Content, 3));
        context.ReportSectionChartTemplates.AddRange(
            new ReportSectionChartTemplate(1, 1),
            new ReportSectionChartTemplate(2, 1),
            new ReportSectionChartTemplate(3, 1),
            new ReportSectionChartTemplate(4, 2));
        context.ReportTemplateChartTemplates.Add(new ReportTemplateChartTemplate(1, 3));
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var usage = Service.FindUsage(1).ToArray();

        Assert.Equal(["A report", "B report"], usage.Select(r => r.Name));
        Assert.Equal(["guid-3"], usage[0].Sections);
        Assert.Null(usage[0].Owner);
        Assert.False(usage[0].IsEnabled);
        Assert.Equal(["guid-2", "Second"], usage[1].Sections);
        Assert.Equal("owner", usage[1].Owner);
        Assert.True(usage[1].IsEnabled);
    }

    [Fact]
    public void FindUsage_OnlyOfferedByReportTemplate_IsEmpty()
    {
        var context = Context;
        context.ChartTemplates.Add(new ChartTemplate(3, "offered", "t"));
        context.ReportTemplates.Add(new ReportTemplate("template", ReportType.Content, "subject", "body") { Id = 1 });
        context.ReportTemplateChartTemplates.Add(new ReportTemplateChartTemplate(1, 3));
        context.SaveChanges();
        context.ChangeTracker.Clear();

        Assert.Empty(Service.FindUsage(3));
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        GC.SuppressFinalize(this);
    }
}
