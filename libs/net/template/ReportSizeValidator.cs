using TNO.Core.Reporting;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine;

/// <summary>
/// Checks the source data before image loading, AI generation or Razor rendering begins.
/// </summary>
public static class ReportSizeValidator
{
    /// <summary>
    /// Conservatively budget the source text and markup for each enabled section entry.
    /// </summary>
    /// <param name="sections">Sections being rendered.</param>
    public static void Validate(IEnumerable<ReportSectionModel> sections)
    {
        long characters = 0;
        foreach (var section in sections.Where(s => s.IsEnabled))
        {
            characters += section.Data?.Length ?? 0;
            foreach (var content in section.Content)
            {
                characters += ReportSizeLimit.StoryOverheadCharacters + (long)content.Headline.Length
                    + content.Body.Length + content.Summary.Length + (content.ImageContent?.Length ?? 0);
                ReportSizeLimit.Validate(characters);
            }
            ReportSizeLimit.Validate(characters);
        }
    }
}
