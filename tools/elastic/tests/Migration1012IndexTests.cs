using Xunit;
using System.Text.Json.Nodes;

namespace TNO.Elastic.Tests;

/// <summary>
/// Migration 1.0.12 adds the keyword sub-fields, stops indexing 'versions', and must change nothing else.
/// </summary>
public class Migration1012IndexTests
{
    private static JsonNode Read(string version, string file)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "tools/elastic/migration/Migrations"))) directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "tools/elastic/migration/Migrations", version, "indexes", file);
        return JsonNode.Parse(File.ReadAllText(path))!;
    }

    [Theory]
    [InlineData("index-content.json")]
    [InlineData("index-published.json")]
    public void AddsOnlyTheKeywordSubFieldsAndDisablesVersions(string file)
    {
        var previous = Read("1.0.11", file);
        var next = Read("1.0.12", file);
        var properties = next["data"]!["mappings"]!["properties"]!.AsObject();
        foreach (var field in new[] { "byline", "otherSeries" })
        {
            var mapping = properties[field]!;
            Assert.Equal("text", mapping["type"]!.GetValue<string>());
            Assert.Equal("keyword", mapping["fields"]!["keyword"]!["type"]!.GetValue<string>());
            Assert.Equal(256, mapping["fields"]!["keyword"]!["ignore_above"]!.GetValue<int>());
            mapping.AsObject().Remove("fields");
        }

        var versions = properties["versions"]!;
        Assert.Equal("object", versions["type"]!.GetValue<string>());
        Assert.False(versions["enabled"]!.GetValue<bool>());
        Assert.Null(versions["properties"]);
        properties.Remove("versions");

        next["settings"]!["indexName"] = previous["settings"]!["indexName"]!.DeepClone();
        Assert.True(JsonNode.DeepEquals(previous, next));
    }
}
