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
            ReportSizeLimit.Validate(instance.Body.Length);
        else
            ReportSizeLimit.Validate(service.EstimateReportSize(instance.Id,
                instance.Report?.Sections.Where(s => s.IsEnabled).Select(s => s.Name).ToArray() ?? []));
    }
}
