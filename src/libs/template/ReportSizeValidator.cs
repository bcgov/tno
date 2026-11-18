using TNO.Core.Reporting;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine;

/// <summary>
/// Checks the source data before image loading, AI generation or Razor rendering begins.
/// The estimate is a lower bound of the rendered output: a story's body only counts in a section
/// that shows the full story, and its image only in a section that shows images, because a
/// section that renders neither (an aggregation or headline list) writes a fraction of its source.
/// The output itself is bounded exactly while the template writes it.
/// </summary>
public static class ReportSizeValidator
{
    /// <summary>
    /// Budget the source text each enabled section entry renders, plus a markup allowance.
    /// </summary>
    /// <param name="sections">Sections being rendered.</param>
    public static void Validate(IEnumerable<ReportSectionModel> sections)
    {
        long characters = 0;
        foreach (var section in sections.Where(s => s.IsEnabled))
        {
            characters += section.Data?.Length ?? 0;
            var showFullStory = section.Settings.ShowFullStory;
            var showImage = section.Settings.ShowImage;
            foreach (var content in section.Content)
            {
                characters += ReportSizeLimit.StoryOverheadCharacters + (long)content.Headline.Length + content.Summary.Length;
                if (showFullStory) characters += content.Body.Length;
                if (showImage) characters += content.ImageContent?.Length ?? 0;
                ReportSizeLimit.Validate(characters);
            }
            ReportSizeLimit.Validate(characters);
        }
    }
}
