using System.Net;
using System.Text;
using TNO.Core.Reporting;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine;

/// <summary>
/// Output for a report that only collects content for other reports (e.g. media analytics).
/// Its template is never run: the preview lists each section's headlines, without summaries,
/// bodies, images, charts or AI output, so even a very large collection stays small.
/// </summary>
public static class CollectionOnlyReport
{
    /// <summary>
    /// Allowance for the page heading, notice and each section heading.
    /// </summary>
    public const int SectionOverheadCharacters = 256;

    /// <summary>
    /// Message shown on the preview, and the reason given when sending is refused.
    /// </summary>
    public const string Notice = "This report is for content collection only. It is not sent, and its preview lists headlines only.";

    /// <summary>
    /// Reject a headline list that would exceed the shared report size limit.
    /// </summary>
    /// <param name="sections">Sections being listed.</param>
    public static void Validate(IEnumerable<ReportSectionModel> sections)
    {
        long characters = SectionOverheadCharacters;
        foreach (var section in Listed(sections))
        {
            characters += SectionOverheadCharacters + section.Name.Length + section.Settings.Label.Length;
            foreach (var content in section.Content)
                characters += ReportSizeLimit.HeadlineOverheadCharacters + content.Headline.Length;
            ReportSizeLimit.Validate(characters);
        }
    }

    /// <summary>
    /// List the headlines of each enabled section that holds stories, in section order.
    /// </summary>
    /// <param name="reportName">Report name used as the page heading.</param>
    /// <param name="sections">Sections and their stories.</param>
    /// <returns>HTML.</returns>
    public static string Render(string reportName, IEnumerable<ReportSectionModel> sections)
    {
        Validate(sections);
        var html = new StringBuilder();
        html.Append("<h1>").Append(WebUtility.HtmlEncode(reportName)).Append("</h1>");
        html.Append("<p><em>").Append(WebUtility.HtmlEncode(Notice)).Append("</em></p>");
        foreach (var section in Listed(sections))
        {
            var label = String.IsNullOrWhiteSpace(section.Settings.Label) ? section.Name : section.Settings.Label;
            var content = section.Content.ToArray();
            html.Append("<h2>").Append(WebUtility.HtmlEncode(label))
                .Append(" (").Append(content.Length.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)).Append(")</h2>");
            html.Append("<ol>");
            foreach (var story in content)
                html.Append("<li>").Append(WebUtility.HtmlEncode(story.Headline)).Append("</li>");
            html.Append("</ol>");
        }
        return html.ToString();
    }

    private static IEnumerable<ReportSectionModel> Listed(IEnumerable<ReportSectionModel> sections)
        => sections.Where(s => s.IsEnabled && s.Content.Any()).OrderBy(s => s.SortOrder);
}
