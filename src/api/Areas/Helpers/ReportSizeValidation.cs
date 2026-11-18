using System.Text.Json;
using TNO.Core.Exceptions;
using TNO.Core.Reporting;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.API.Helpers;

/// <summary>
/// Reject oversized previews and sends before loading story bodies or queuing a report.
/// </summary>
public static class ReportSizeValidation
{
    /// <summary>
    /// Message returned when a collection-only report is asked to be sent.
    /// </summary>
    public const string CollectionOnlySendMessage = "This report is for content collection only and cannot be sent. Turn off 'Collection only' in the report settings to send it.";

    /// <summary>
    /// Validate stored output, or estimate the enabled sections when rendering is required.
    /// A collection-only report always renders its headline list, so only that list is estimated.
    /// </summary>
    /// <param name="service">Report instance queries.</param>
    /// <param name="instance">Authorized report instance.</param>
    /// <param name="useStoredOutput">Whether this operation will reuse the stored body.</param>
    public static void ValidateReportSize(this IReportInstanceService service, ReportInstance instance, bool useStoredOutput = false)
    {
        var sections = instance.Report?.Sections.Where(s => s.IsEnabled).ToArray() ?? [];
        if (instance.Report.IsCollectionOnly())
        {
            ReportSizeLimit.Validate(service.EstimateHeadlineListSize(instance.Id, sections.Select(s => s.Name).ToArray()));
            return;
        }

        if (useStoredOutput && !string.IsNullOrWhiteSpace(instance.Body))
        {
            ReportSizeLimit.Validate(instance.Body.Length);
            return;
        }

        ReportSizeLimit.Validate(service.EstimateReportSize(instance.Id,
            sections.Select(s => s.Name).ToArray(),
            sections.Where(ShowsFullStory).Select(s => s.Name).ToArray()));
    }

    /// <summary>
    /// Whether the report only collects content for other reports, and is never sent.
    /// </summary>
    /// <param name="report">Report, if loaded.</param>
    public static bool IsCollectionOnly(this Report? report)
        => report?.Settings.RootElement.ValueKind == JsonValueKind.Object
            && report.Settings.RootElement.TryGetProperty("collectionOnly", out var value)
            && value.ValueKind == JsonValueKind.True;

    /// <summary>
    /// Refuse to send or publish a collection-only report.
    /// </summary>
    /// <param name="report">Report, if loaded.</param>
    public static void ThrowIfCollectionOnly(this Report? report)
    {
        if (report.IsCollectionOnly()) throw new BadRequestException(CollectionOnlySendMessage);
    }

    /// <summary>
    /// Whether the section's settings show the full story, which is when its template renders the body.
    /// </summary>
    private static bool ShowsFullStory(ReportSection section)
        => section.Settings.RootElement.ValueKind == JsonValueKind.Object
            && section.Settings.RootElement.TryGetProperty("showFullStory", out var value)
            && value.ValueKind == JsonValueKind.True;
}
