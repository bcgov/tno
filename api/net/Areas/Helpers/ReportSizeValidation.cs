using System.Text.Json;
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
    /// Validate stored output, or estimate the enabled sections when rendering is required.
    /// </summary>
    /// <param name="service">Report instance queries.</param>
    /// <param name="instance">Authorized report instance.</param>
    /// <param name="useStoredOutput">Whether this operation will reuse the stored body.</param>
    public static void ValidateReportSize(this IReportInstanceService service, ReportInstance instance, bool useStoredOutput = false)
    {
        if (useStoredOutput && !string.IsNullOrWhiteSpace(instance.Body))
        {
            ReportSizeLimit.Validate(instance.Body.Length);
            return;
        }

        var sections = instance.Report?.Sections.Where(s => s.IsEnabled).ToArray() ?? [];
        ReportSizeLimit.Validate(service.EstimateReportSize(instance.Id,
            sections.Select(s => s.Name).ToArray(),
            sections.Where(ShowsFullStory).Select(s => s.Name).ToArray()));
    }

    /// <summary>
    /// Whether the section's settings show the full story, which is when its template renders the body.
    /// </summary>
    private static bool ShowsFullStory(ReportSection section)
        => section.Settings.RootElement.ValueKind == JsonValueKind.Object
            && section.Settings.RootElement.TryGetProperty("showFullStory", out var value)
            && value.ValueKind == JsonValueKind.True;
}
