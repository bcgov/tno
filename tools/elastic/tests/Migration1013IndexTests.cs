using Xunit;
using System.Text.Json.Nodes;
using TNO.Elastic.Migration;

namespace TNO.Elastic.Tests;

/// <summary>
/// Migration 1.0.13 corrects 'status' on the published index and must change nothing else.
/// </summary>
public class Migration1013IndexTests
{
    private static JsonNode Read(string version, string folder)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "tools/elastic/migration/Migrations"))) directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "tools/elastic/migration/Migrations", version, folder, "index-published.json");
        return JsonNode.Parse(File.ReadAllText(path))!;
    }

    [Fact]
    public void KeepsThePublishedMapping()
    {
        var previous = Read("1.0.12", "indexes");
        var next = Read("1.0.13", "indexes");
        var rollback = Read("1.0.13", "rollback");
        Assert.Equal("${PublishedIndex}_v1.0.13", next["settings"]!["indexName"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(previous, rollback));

        next["settings"]!["indexName"] = previous["settings"]!["indexName"]!.DeepClone();
        Assert.True(JsonNode.DeepEquals(previous, next));
    }

    [Fact]
    public void CopiesPublishedStatusAsTheLabelTheIndexerWrites()
    {
        var script = NativeReindex.CreateRequest("source", "destination", true, true, true)["script"]!["source"]!.GetValue<string>();
        Assert.Contains("ctx._source.status = 'Published';", script);
        Assert.DoesNotContain("ctx._source.status = 2", script);
    }
}
