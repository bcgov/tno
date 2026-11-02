using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RazorEngineCore;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.Test.Services.Reporting;

/// <summary>
/// The expectations below are the records CsvHelper 33's GetRecords&lt;dynamic&gt;() produced for the same input,
/// so report templates keep seeing the same data after the CsvHelper 34 upgrade.
/// </summary>
public class CsvDynamicReaderTest
{
    private static List<IDictionary<string, object?>> Read(string csv, List<string>? malformed = null)
    {
        using var reader = new StringReader(csv);
        return CsvDynamicReader.ReadRecords(reader, malformed == null ? null : malformed.Add)
            .Select(r => (IDictionary<string, object?>)r)
            .ToList();
    }

    [Fact]
    public void ReadRecords_KeysByHeaderWithStringValues()
    {
        var records = Read("name,count,date\nAlice,1,2026-01-01\nBob,2,2026-01-02\n");

        records.Should().HaveCount(2);
        records[0].Should().Equal(new Dictionary<string, object?> { ["name"] = "Alice", ["count"] = "1", ["date"] = "2026-01-01" });
        records[1]["name"].Should().Be("Bob");
    }

    [Fact]
    public void ReadRecords_SupportsDynamicMemberAccess()
    {
        using var reader = new StringReader("name,count\nAlice,1\n");
        dynamic record = CsvDynamicReader.ReadRecords(reader)[0];

        string name = record.name;
        name.Should().Be("Alice");
    }

    [Fact]
    public void ReadRecords_ParsesQuotedSeparatorsQuotesAndLineBreaks()
    {
        var records = Read("id,payload,note\n1,\"{\"\"a\"\":1}\",\"x, y\"\n2,\"line one\nline two\",\"He said \"\"hi\"\"\"\n");

        records[0]["payload"].Should().Be("{\"a\":1}");
        records[0]["note"].Should().Be("x, y");
        records[1]["payload"].Should().Be("line one\nline two");
        records[1]["note"].Should().Be("He said \"hi\"");
    }

    [Fact]
    public void ReadRecords_ShortRowYieldsNullForMissingColumns()
    {
        var records = Read("a,b,c\n1,2\n");

        records[0].Should().Equal(new Dictionary<string, object?> { ["a"] = "1", ["b"] = "2", ["c"] = null });
    }

    [Fact]
    public void ReadRecords_IgnoresFieldsBeyondTheHeader()
    {
        var records = Read("a,b\n1,2,3,4\n");

        records[0].Should().Equal(new Dictionary<string, object?> { ["a"] = "1", ["b"] = "2" });
    }

    [Fact]
    public void ReadRecords_DuplicateHeaderKeepsTheLastValue()
    {
        var records = Read("a,a,b\n1,2,3\n");

        records[0].Should().Equal(new Dictionary<string, object?> { ["a"] = "2", ["b"] = "3" });
        records[0].Keys.Should().ContainInOrder("a", "b");
    }

    [Fact]
    public void ReadRecords_SkipsBlankLinesAndKeepsEmptyFields()
    {
        var records = Read("a,b\n\n,\n\n3,4\n");

        records.Should().HaveCount(2);
        records[0].Should().Equal(new Dictionary<string, object?> { ["a"] = "", ["b"] = "" });
        records[1]["a"].Should().Be("3");
    }

    [Fact]
    public void ReadRecords_DoesNotTrimOrSplitOnOtherSeparators()
    {
        Read("a , b\n 1 ,2\n")[0].Should().Equal(new Dictionary<string, object?> { ["a "] = " 1 ", [" b"] = "2" });
        Read("a;b\n1;2\n")[0].Should().Equal(new Dictionary<string, object?> { ["a;b"] = "1;2" });
    }

    [Fact]
    public void ReadRecords_HeaderOnlyOrEmptyReturnsNoRecords()
    {
        Read("a,b,c\n").Should().BeEmpty();
        Read("").Should().BeEmpty();
    }

    [Fact]
    public void ReadRecords_ReportsMalformedRecordsAndStillReadsThem()
    {
        var malformed = new List<string>();
        var records = Read("a,b\nab\"c,2\n\"x\"y,3\n", malformed);

        malformed.Should().HaveCount(2);
        records.Select(r => r["a"]).Should().Equal("ab\"c", "xy");
    }

    [Fact]
    public async Task ReadRecords_CanBeRenderedByARazorDataTemplate()
    {
        // ReportEngine passes the records to the section's data template as Model (a ReportEngineDataModel) and Data.
        using var reader = new StringReader("name,count\nAlice,1\nBob,\n");
        var records = CsvDynamicReader.ReadRecords(reader);
        var engine = new TemplateEngine<ReportEngineDataModel<dynamic>>(new RazorEngine(), NullLogger<TemplateEngine<ReportEngineDataModel<dynamic>>>.Instance);
        var template = engine.GetOrAddTemplateInMemory("csv-data-test", "@foreach (var row in Model.Data) {<li>@row.name=@row.count</li>}");

        var html = await template.RunAsync(instance =>
        {
            instance.Model = new ReportEngineDataModel<dynamic>(records);
            instance.Data = records;
        });

        html.Should().Be("<li>Alice=1</li><li>Bob=</li>");
    }
}
