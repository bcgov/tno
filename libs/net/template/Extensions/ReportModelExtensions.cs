using System.Text.Json;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine.Extensions;

/// <summary>
/// ReportModelExtensions static class, provides extension methods for ReportModel objects.
/// </summary>
public static class ReportModelExtensions
{
    /// <summary>
    /// Parse the report settings for sections.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    private static Dictionary<string, ReportSectionModel> ParseSections(this JsonDocument settings)
    {
        var sections = new Dictionary<string, ReportSectionModel>();
        if (settings.RootElement.TryGetProperty("sections", out JsonElement sectionsElement))
        {
            foreach (var sectionElement in sectionsElement.EnumerateArray())
            {
                var name = sectionElement.GetProperty("name").GetString() ?? "";
                var label = sectionElement.GetProperty("label").GetString() ?? "";
                sections.Add(name, new ReportSectionModel(name, label));
            }
        }
        return sections;
    }

    /// <summary>
    /// Parse the report settings for sections.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public static Dictionary<string, ReportSectionModel> ParseSections(this API.Areas.Services.Models.Report.ReportModel report)
    {
        var settings = JsonDocument.Parse(JsonSerializer.Serialize(report.Settings));
        var sections = settings.ParseSections();
        sections.Add("", new ReportSectionModel("", ""));
        return sections;
    }

    /// <summary>
    /// Parse the report settings for sections.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public static Dictionary<string, ReportSectionModel> ParseSections(this API.Areas.Admin.Models.Report.ReportModel report)
    {
        var settings = JsonDocument.Parse(JsonSerializer.Serialize(report.Settings));
        var sections = settings.ParseSections();
        sections.Add("", new ReportSectionModel("", ""));
        return sections;
    }
}
