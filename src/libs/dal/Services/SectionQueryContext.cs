using TNO.API.Models.Settings;

namespace TNO.DAL.Services;

/// <summary>
/// SectionQueryContext record, provides what is computed once per report to build the Elasticsearch query of a section.
/// </summary>
/// <param name="ExcludeHistoricalContentIds">Content in previous instances.</param>
/// <param name="ExcludeReportContentIds">Content in the reports the settings exclude.</param>
/// <param name="ExcludeAboveSectionContentIds">Content in the sections above. The list grows as the sections are worked through.</param>
/// <param name="PreviousPublishedOn">When the previous instance was published.</param>
/// <param name="ReportSettings">The report settings.</param>
/// <param name="OwnerId">The user whose source and media type exclusions apply.</param>
public record SectionQueryContext(
    long[] ExcludeHistoricalContentIds,
    long[] ExcludeReportContentIds,
    IEnumerable<long> ExcludeAboveSectionContentIds,
    DateTime? PreviousPublishedOn,
    ReportSettingsModel ReportSettings,
    int? OwnerId);
