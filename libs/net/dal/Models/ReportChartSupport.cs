using System.Text.Json;
using TNO.API.Models.Settings;
using TNO.Entities;

namespace TNO.DAL.Models;

/// <summary>
/// ReportChartSource enum, provides where the stories of a chart come from.
/// </summary>
public enum ReportChartSource
{
    /// <summary>The stories listed in this report's instance.</summary>
    InstanceIds,
    /// <summary>The stories listed in the linked report's current instance.</summary>
    LinkedInstanceIds,
    /// <summary>Everything the section's own filter matches.</summary>
    Filter,
    /// <summary>The stories in the section's own folder.</summary>
    Folder,
}

/// <summary>
/// ReportChartSupport static class, decides how a media analytics section gets its chart data.
/// </summary>
public static class ReportChartSupport
{
    /// <summary>Configuration key, turned on once the 'byline.keyword' index is live in the environment.</summary>
    public const string ByLineAggregationKey = "Reports:ByLineAggregation";

    /// <summary>
    /// Get the source of the section's charts, or null when the section has none (it charts its own stories).
    /// </summary>
    /// <param name="section"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ReportChartSource? GetSource(ReportSection section, JsonSerializerOptions options)
    {
        var settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(section.Settings.RootElement.GetRawText(), options) ?? new();
        return GetSource(settings.UseAllContent, section.LinkedReportId, section.FolderId, section.FilterId);
    }

    /// <summary>
    /// Get the source for the specified data source values. The order matches how the report engine resolves them.
    /// </summary>
    /// <param name="useAllContent"></param>
    /// <param name="linkedReportId"></param>
    /// <param name="folderId"></param>
    /// <param name="filterId"></param>
    /// <returns></returns>
    public static ReportChartSource? GetSource(bool useAllContent, int? linkedReportId, int? folderId, int? filterId)
    {
        if (useAllContent) return ReportChartSource.InstanceIds;
        if (linkedReportId.HasValue) return ReportChartSource.LinkedInstanceIds;
        if (folderId.HasValue) return ReportChartSource.Folder;
        if (filterId.HasValue) return ReportChartSource.Filter;
        return null;
    }

    /// <summary>
    /// Deserialize the chart template settings, or null when the chart template is not loaded.
    /// </summary>
    /// <param name="chart"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ChartTemplateSettingsModel? GetTemplateSettings(ReportSectionChartTemplate chart, JsonSerializerOptions options)
    {
        return chart.ChartTemplate == null ? null : JsonSerializer.Deserialize<ChartTemplateSettingsModel>(chart.ChartTemplate.Settings.RootElement.GetRawText(), options);
    }

    /// <summary>
    /// Whether the built-in converter can produce data for the specified dimensions.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="byLineEnabled"></param>
    /// <returns></returns>
    public static bool SupportsDimensions(ChartSectionSettingsModel settings, bool byLineEnabled)
    {
        return byLineEnabled || (settings.GroupBy != "byline" && settings.Dataset != "byline");
    }

    /// <summary>
    /// Whether the chart's data comes from Elasticsearch aggregations, instead of a list of stories.
    /// </summary>
    /// <param name="chart"></param>
    /// <param name="options"></param>
    /// <param name="byLineEnabled"></param>
    /// <returns></returns>
    public static bool SupportsAggregation(ReportSectionChartTemplate chart, JsonSerializerOptions options, bool byLineEnabled)
    {
        var template = GetTemplateSettings(chart, options);
        if (template == null) return false;
        if (template.Aggregate)
        {
            var settings = JsonSerializer.Deserialize<ChartSectionSettingsModel>(chart.Settings.RootElement.GetRawText(), options) ?? new();
            return SupportsDimensions(settings, byLineEnabled);
        }
        return template.Aggregations != null;
    }

    /// <summary>
    /// Whether the section needs no stories at all: a media analytics section with its own filter
    /// or folder, every chart of which aggregates. A section with a chart that still walks stories
    /// keeps fetching them, and a filter that declares its own aggregations keeps running because
    /// the report template reads the result from the section.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="options"></param>
    /// <param name="byLineEnabled"></param>
    /// <returns></returns>
    public static bool IsAnalyticsOnly(ReportSection section, JsonSerializerOptions options, bool byLineEnabled)
    {
        if (section.SectionType != ReportSectionType.MediaAnalytics) return false;
        if (GetSource(section, options) is not (ReportChartSource.Filter or ReportChartSource.Folder)) return false;
        if (section.ChartTemplatesManyToMany.Count == 0) return false;
        if (section.Filter?.Query.RootElement is { ValueKind: JsonValueKind.Object } query
            && (query.TryGetProperty("aggs", out _) || query.TryGetProperty("aggregations", out _))) return false;
        return section.ChartTemplatesManyToMany.All(chart => SupportsAggregation(chart, options, byLineEnabled));
    }
}
