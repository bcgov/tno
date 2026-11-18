using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Test.Core;

namespace TNO.Test.DAL;

/// <summary>
/// The chart data stored with an instance is what later renders read, so replacing part of it must
/// not disturb the rest.
/// </summary>
public class ReportInstanceChartTest : IDisposable
{
    readonly TestHelper helper = new();

    public ReportInstanceChartTest()
    {
        helper.Build((services) =>
        {
            services.AddTNOContext("report-instance-chart");
            services.AddPrincipalForRole("editor");
            services.AddOptions();
            services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));
            services.AddSingleton<IReportInstanceService, ReportInstanceService>();
        });
    }

    private static ReportInstanceChart Row(int sectionId, int chartId, string json) =>
        new(1, sectionId, chartId, $"section-{sectionId}", JsonDocument.Parse(json)) { DocCount = 5, GeneratedOn = DateTime.UtcNow };

    [Fact]
    public void SaveCharts_WithSections_OnlyReplacesThoseSections()
    {
        var service = helper.Provider.GetRequiredService<IReportInstanceService>();
        service.SaveCharts(1, [Row(1, 1, "{\"a\":1}"), Row(2, 1, "{\"b\":1}")]);
        helper.Provider.GetRequiredService<TNOContext>().ChangeTracker.Clear();

        service.SaveCharts(1, [Row(1, 2, "{\"a\":2}")], [1]);
        helper.Provider.GetRequiredService<TNOContext>().ChangeTracker.Clear();

        var rows = service.FindCharts(1).ToArray();
        Assert.Equal(2, rows.Length);
        Assert.Contains(rows, r => r.SectionId == 1 && r.ChartTemplateId == 2);
        Assert.Contains(rows, r => r.SectionId == 2 && r.ChartTemplateId == 1);
    }

    [Fact]
    public void SaveCharts_WithoutSections_ReplacesEveryRow()
    {
        var service = helper.Provider.GetRequiredService<IReportInstanceService>();
        service.SaveCharts(1, [Row(1, 1, "{}"), Row(2, 1, "{}")]);
        helper.Provider.GetRequiredService<TNOContext>().ChangeTracker.Clear();

        service.SaveCharts(1, [Row(1, 1, "{}")]);
        helper.Provider.GetRequiredService<TNOContext>().ChangeTracker.Clear();

        Assert.Single(service.FindCharts(1));
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
