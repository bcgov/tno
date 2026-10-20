using FluentAssertions;
using TNO.TemplateEngine.FilterAI;

namespace TNO.Test.AI;

/// <summary>
/// Tests for describing a content index's live mapping to a model.
/// </summary>
public class FilterAISchemaTest
{
    private const string Mapping = """
        {
          "content_v1.0.11": {
            "mappings": {
              "properties": {
                "headline": { "type": "text" },
                "storyType": { "type": "keyword" },
                "source": {
                  "properties": {
                    "name": { "type": "text", "fields": { "keyword": { "type": "keyword" } } },
                    "configuration": { "properties": { "url": { "type": "keyword" } } },
                    "updatedOn": { "type": "date" }
                  }
                },
                "actions": {
                  "type": "nested",
                  "properties": {
                    "name": { "type": "keyword" },
                    "value": { "type": "text" },
                    "createdBy": { "type": "text" },
                    "contentId": { "type": "long" }
                  }
                },
                "versions": { "properties": { "a": { "type": "keyword" } } },
                "empty": { "type": "object" },
                "createdOn": { "type": "date" }
              }
            }
          }
        }
        """;

    [Fact]
    public void DescribesFieldsNestedObjectsAndExactMatchSubfields()
    {
        var fields = FilterAISchema.DescribeMapping(Mapping);

        fields.Should().Be(
            "actions: nested (query with { \"nested\": { \"path\": \"actions\", ... } })\n" +
            "actions.name: keyword\n" +
            "actions.value: text\n" +
            "createdOn: date\n" +
            "headline: text\n" +
            "source: object\n" +
            "source.name: text (exact match: source.name.keyword)\n" +
            "storyType: keyword\n");
    }

    [Fact]
    public void CollapsibleFieldsAreSingleValuedFieldsOutsideNestedObjects()
    {
        var schema = FilterAISchema.Read(Mapping);

        schema.CollapsibleFields.Should().Equal("createdOn", "source.name.keyword", "storyType");
    }

    [Fact]
    public void AnUnexpectedResponseDescribesNothing()
    {
        FilterAISchema.DescribeMapping("{}").Should().BeEmpty();
        FilterAISchema.DescribeMapping("[]").Should().BeEmpty();
    }
}
