using Microsoft.Extensions.Logging.Abstractions;
using RazorEngineCore;
using TNO.Core.Exceptions;
using TNO.Core.Reporting;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.Test.AI;

public class ReportSizeLimitTest
{
    [Fact]
    public void BoundaryIsInclusiveAndLargeCountsCannotOverflow()
    {
        ReportSizeLimit.Validate(ReportSizeLimit.MaximumCharacters);
        Assert.Throws<ReportTooLargeException>(() => ReportSizeLimit.Validate(ReportSizeLimit.MaximumCharacters + 1L));
        Assert.Throws<ReportTooLargeException>(() => ReportSizeLimit.Validate(long.MaxValue));
    }

    [Fact]
    public void PreflightCountsRepeatedStoriesButIgnoresDisabledSections()
    {
        var content = new ContentModel { Body = new string('x', 6_000_000) };
        var first = new ReportSectionModel { IsEnabled = true, Content = [content] };
        var second = new ReportSectionModel { IsEnabled = false, Content = [content] };
        ReportSizeValidator.Validate([first, second]);
        second.IsEnabled = true;
        Assert.Throws<ReportTooLargeException>(() => ReportSizeValidator.Validate([first, second]));
        Assert.Same(content, first.Content.Single());
        Assert.Equal(6_000_000, content.Body.Length);
    }

    [Fact]
    public async Task NormalRazorMarkupAndAttributesAreUnchanged()
    {
        var engine = new TemplateEngine<ReportEngineContentModel>(new RazorEngine(), NullLogger<TemplateEngine<ReportEngineContentModel>>.Instance);
        var template = engine.GetOrAddTemplateInMemory("small", "<div title=\"@ReportId\">@ReportInstanceId</div>");
        var output = await template.RunAsync(model => { model.ReportId = 12; model.ReportInstanceId = 34; });
        Assert.Equal("<div title=\"12\">34</div>", output);
    }

    [Theory]
    [InlineData("@for (var i = 0; i < 11; i++) { @Model }")]
    [InlineData("@for (var i = 0; i < 11; i++) { <img src=\"@Model\" /> }")]
    public async Task RazorExpansionIsStoppedBeforeBuildingOversizedOutput(string source)
    {
        var engine = new TemplateEngine<ReportEngineContentModel>(new RazorEngine(), NullLogger<TemplateEngine<ReportEngineContentModel>>.Instance);
        var template = engine.GetOrAddTemplateInMemory(source, source);
        await Assert.ThrowsAsync<ReportTooLargeException>(() => template.RunAsync(model => model.Model = new string('x', 1_000_000)));
    }

    [Fact]
    public void LiteralOutputIsBounded()
    {
        var model = new ReportEngineContentModel();
        model.WriteLiteral(new string('x', ReportSizeLimit.MaximumCharacters));
        Assert.Throws<ReportTooLargeException>(() => model.WriteLiteral("x"));
    }

    [Fact]
    public void ChartReplacementChecksAllOccurrencesBeforeAllocating()
    {
        Assert.Equal("image and image", ReportSizeLimit.Replace("[chart] and [chart]", "[chart]", "image"));
        var image = new string('x', 6_000_000);
        Assert.Throws<ReportTooLargeException>(() => ReportSizeLimit.Replace("[chart][chart]", "[chart]", image));
        Assert.Equal("no chart", ReportSizeLimit.Replace("no chart", "[chart]", image));
    }
}
