using System.Text;
using System.Text.Json.Nodes;
using TNO.TemplateEngine;

namespace TNO.Test.Services.Reporting;

/// <summary>
/// The options sent to the Charts API decide how a chart labels its values.
/// </summary>
public class ChartOptionsTest
{
    const string Formatter = "(value) => value + '%'";

    [Fact]
    public void MergeChartPlugins_SectionWithoutFormatter_KeepsTheTemplateFormatter()
    {
        var template = JsonNode.Parse($$$"""{"legend":{"display":false},"datalabels":{"formatter":"{{{Formatter}}}"}}""");
        var section = JsonNode.Parse("""{"legend":{"display":true},"datalabels":{"color":"#fff"}}""");

        var result = ReportExtensions.MergeChartPlugins(template, section)!;

        Assert.Equal(Formatter, result["datalabels"]!["formatter"]!.GetValue<string>());
        Assert.Equal("#fff", result["datalabels"]!["color"]!.GetValue<string>());
        Assert.True(result["legend"]!["display"]!.GetValue<bool>());
        Assert.False(section!["datalabels"]!.AsObject().ContainsKey("formatter"));
    }

    [Fact]
    public void MergeChartPlugins_SectionWithFormatter_KeepsTheSectionFormatter()
    {
        var template = JsonNode.Parse($$$"""{"datalabels":{"formatter":"{{{Formatter}}}"}}""");
        var section = JsonNode.Parse("""{"datalabels":{"formatter":"(value) => value"}}""");

        Assert.Equal("(value) => value", ReportExtensions.MergeChartPlugins(template, section)!["datalabels"]!["formatter"]!.GetValue<string>());
    }

    [Fact]
    public void MergeChartPlugins_SectionWithoutDataLabels_AddsNoDataLabels()
    {
        var template = JsonNode.Parse($$$"""{"datalabels":{"formatter":"{{{Formatter}}}"}}""");
        var section = JsonNode.Parse("""{"legend":{"display":true}}""");

        Assert.False(ReportExtensions.MergeChartPlugins(template, section)!.AsObject().ContainsKey("datalabels"));
    }

    [Fact]
    public void EncodeChartOptions_SurvivesAQueryString()
    {
        // Three bytes that encode to base64 with a '+' and a '/'.
        var json = "{\"a\":\"~>?~>?~>?\"}";
        var plain = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        Assert.True(plain.Contains('+') || plain.Contains('/'));

        var encoded = ReportExtensions.EncodeChartOptions(json);

        Assert.DoesNotContain("+", encoded);
        Assert.DoesNotContain("/", encoded);
        Assert.Equal(json, Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(encoded))));
    }
}
