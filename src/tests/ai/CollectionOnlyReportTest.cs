using TNO.Core.Exceptions;
using TNO.Core.Reporting;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.Test.AI;

public class CollectionOnlyReportTest
{
    private static ContentModel Story(string headline) => new()
    {
        Headline = headline,
        Summary = "SUMMARY-TEXT",
        Body = "BODY-TEXT",
    };

    [Fact]
    public void ListsOnlyHeadlinesOfEnabledSectionsWithStoriesInSectionOrder()
    {
        var html = CollectionOnlyReport.Render("Trade War <Categories>",
        [
            new ReportSectionModel { Name = "b", SortOrder = 2, IsEnabled = true, Settings = new() { Label = "Tariffs" }, Content = [Story("Second & last")] },
            new ReportSectionModel { Name = "a-name", SortOrder = 1, IsEnabled = true, Content = [Story("First"), Story("<b>Bold</b>")] },
            new ReportSectionModel { Name = "off", SortOrder = 0, IsEnabled = false, Settings = new() { Label = "Disabled" }, Content = [Story("Hidden")] },
            new ReportSectionModel { Name = "empty", SortOrder = 3, IsEnabled = true, Settings = new() { Label = "Empty" }, Content = [] },
        ]);

        Assert.StartsWith("<h1>Trade War &lt;Categories&gt;</h1>", html);
        Assert.Contains(CollectionOnlyReport.Notice, html);
        Assert.DoesNotContain("SUMMARY-TEXT", html);
        Assert.DoesNotContain("BODY-TEXT", html);
        Assert.DoesNotContain("Hidden", html);
        Assert.DoesNotContain("Empty", html);
        // The label is used when set; otherwise the section name.
        Assert.Contains("<h2>a-name (2)</h2><ol><li>First</li><li>&lt;b&gt;Bold&lt;/b&gt;</li></ol>", html);
        Assert.Contains("<h2>Tariffs (1)</h2><ol><li>Second &amp; last</li></ol>", html);
        Assert.True(html.IndexOf("a-name", StringComparison.Ordinal) < html.IndexOf("Tariffs", StringComparison.Ordinal));
    }

    [Fact]
    public void LargeCollectionsFitBecauseOnlyHeadlinesCount()
    {
        // About the size of the Trade War Categories collection, each story with a large body.
        var body = new string('x', 5_000);
        var stories = Enumerable.Range(0, 20_000).Select(i => new ContentModel { Headline = $"Headline {i}", Body = body, Summary = body }).ToArray();
        var section = new ReportSectionModel { Name = "all", IsEnabled = true, Settings = new() { ShowFullStory = true }, Content = stories };

        Assert.Throws<ReportTooLargeException>(() => ReportSizeValidator.Validate([section]));
        var html = CollectionOnlyReport.Render("Collection", [section]);
        Assert.Contains("<li>Headline 19999</li>", html);
        Assert.True(html.Length < ReportSizeLimit.MaximumCharacters);
    }

    [Fact]
    public void EstimateNeverFallsBelowTheRenderedList()
    {
        var sections = new[]
        {
            new ReportSectionModel { Name = "a", IsEnabled = true, Settings = new() { Label = "Label" }, Content = Enumerable.Range(0, 50).Select(i => Story($"H{i}")).ToArray() },
        };
        long estimate = CollectionOnlyReport.SectionOverheadCharacters * 2 + "a".Length + "Label".Length
            + sections[0].Content.Sum(c => ReportSizeLimit.HeadlineOverheadCharacters + c.Headline.Length);
        Assert.True(CollectionOnlyReport.Render("", sections).Length <= estimate);
    }

    [Fact]
    public void AHeadlineListOverTheLimitIsStillRejected()
    {
        var headline = new string('h', 1_000);
        var section = new ReportSectionModel
        {
            Name = "all",
            IsEnabled = true,
            Content = Enumerable.Range(0, ReportSizeLimit.MaximumCharacters / 1_000 + 1).Select(_ => new ContentModel { Headline = headline }).ToArray(),
        };
        Assert.Throws<ReportTooLargeException>(() => CollectionOnlyReport.Render("Too many", [section]));
    }
}
