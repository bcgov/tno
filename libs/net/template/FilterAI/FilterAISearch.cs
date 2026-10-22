using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAISearch class, builds the bounded search a model may run and reduces the response to
/// what the model needs. The model only supplies a query clause and, optionally, a field to
/// count values of; the body around it (size, source fields, timeout) is fixed here.
/// </summary>
public static partial class FilterAISearch
{
    #region Variables
    /// <summary>The most stories a search returns to the model.</summary>
    public const int MaxSize = 10;
    /// <summary>The stories a search returns when the model does not say.</summary>
    public const int DefaultSize = 5;
    /// <summary>The most values a field count returns.</summary>
    public const int MaxAggregateSize = 50;
    /// <summary>The values a field count returns when the model does not say.</summary>
    public const int DefaultAggregateSize = 20;
    /// <summary>How long Elasticsearch may spend on one search.</summary>
    public const string Timeout = "10s";

    /// <summary>The fields returned for each story.</summary>
    public static readonly string[] SourceFields = ["id", "headline", "source.code", "source.name", "mediaType.name", "series.name", "contentType", "publishedOn"];

    /// <summary>The nested objects; a field inside one is counted through a nested aggregation.</summary>
    public static readonly string[] NestedPaths = ["actions", "tags", "topics", "tonePools", "timeTrackings", "notifications", "userNotifications"];

    /// <summary>Query types and keys a model may not use: scripts, stored queries and wrappers run arbitrary code or hide what runs.</summary>
    public static readonly string[] ForbiddenKeys = ["script", "script_score", "scripted_metric", "wrapper", "percolate", "runtime_mappings", "stored_fields", "terms_lookup", "index", "_index"];

    /// <summary>The keys an extended query may have.</summary>
    public static readonly string[] ExtendedQueryKeys = ["must", "filter", "must_not", "should", "minimum_should_match"];

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)*$")]
    private static partial Regex FieldRegex();
    #endregion

    #region Methods
    /// <summary>
    /// Build the search body for the model's arguments:
    /// { "query": {...}, "size": 0-10, "aggregate": "field", "aggregateSize": 1-50, "collapse": "field" }.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="FilterAIException">The arguments are not usable; the message is for the model.</exception>
    public static JsonObject BuildSearchBody(JsonElement args)
    {
        if (args.ValueKind != JsonValueKind.Object) throw new FilterAIException("search requires an arguments object.");

        JsonNode? query = null;
        if (args.TryGetProperty("query", out var q))
        {
            if (q.ValueKind != JsonValueKind.Object) throw new FilterAIException("search 'query' must be an Elasticsearch query object, i.e. { \"match\": { \"headline\": \"wildfire\" } }.");
            query = JsonNode.Parse(q.GetRawText());
            if (query is JsonObject queryObject && queryObject.ContainsKey("query") && queryObject.Count == 1)
                query = queryObject["query"]?.DeepClone();
            var forbidden = FindForbiddenKey(query);
            if (forbidden != null) throw new FilterAIException($"search 'query' may not use '{forbidden}'.");
        }

        var size = DefaultSize;
        if (args.TryGetProperty("size", out var s) && s.ValueKind == JsonValueKind.Number && s.TryGetInt32(out var requestedSize))
            size = Math.Clamp(requestedSize, 0, MaxSize);

        var body = new JsonObject()
        {
            ["query"] = query ?? new JsonObject() { ["match_all"] = new JsonObject() },
            ["size"] = size,
            ["_source"] = new JsonArray(SourceFields.Select(f => (JsonNode?)JsonValue.Create(f)).ToArray()),
            ["track_total_hits"] = true,
            ["timeout"] = Timeout,
            ["sort"] = new JsonArray(new JsonObject() { ["publishedOn"] = "desc" }),
        };

        if (args.TryGetProperty("collapse", out var c) && c.ValueKind == JsonValueKind.String && !String.IsNullOrWhiteSpace(c.GetString()))
        {
            var field = c.GetString()!.Trim();
            if (!FieldRegex().IsMatch(field)) throw new FilterAIException($"search 'collapse' must be a field name, not '{field}'.");
            body["collapse"] = new JsonObject() { ["field"] = field };
        }

        if (args.TryGetProperty("aggregate", out var a) && a.ValueKind == JsonValueKind.String && !String.IsNullOrWhiteSpace(a.GetString()))
        {
            var field = a.GetString()!.Trim();
            if (!FieldRegex().IsMatch(field)) throw new FilterAIException($"search 'aggregate' must be a field name, not '{field}'.");
            var aggregateSize = DefaultAggregateSize;
            if (args.TryGetProperty("aggregateSize", out var asz) && asz.ValueKind == JsonValueKind.Number && asz.TryGetInt32(out var requestedAggregateSize))
                aggregateSize = Math.Clamp(requestedAggregateSize, 1, MaxAggregateSize);

            var terms = new JsonObject() { ["terms"] = new JsonObject() { ["field"] = field, ["size"] = aggregateSize } };
            var path = NestedPaths.FirstOrDefault(p => field.StartsWith($"{p}.", StringComparison.Ordinal));
            body["aggs"] = new JsonObject()
            {
                ["values"] = path == null
                    ? terms
                    : new JsonObject()
                    {
                        ["nested"] = new JsonObject() { ["path"] = path },
                        ["aggs"] = new JsonObject() { ["values"] = terms },
                    },
            };
        }

        return body;
    }

    /// <summary>
    /// Reduce a search response to the total, the stories and the counted values.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static JsonObject Summarize(string response)
    {
        using var document = JsonDocument.Parse(response);
        var root = document.RootElement;
        var result = new JsonObject();

        if (root.TryGetProperty("hits", out var hits))
        {
            if (hits.TryGetProperty("total", out var total))
            {
                if (total.ValueKind == JsonValueKind.Object)
                {
                    result["total"] = total.TryGetProperty("value", out var value) && value.TryGetInt64(out var count) ? count : 0;
                    if (total.TryGetProperty("relation", out var relation) && relation.GetString() == "gte") result["totalIsLowerBound"] = true;
                }
                else if (total.ValueKind == JsonValueKind.Number) result["total"] = total.GetInt64();
            }
            var stories = new JsonArray();
            if (hits.TryGetProperty("hits", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (!item.TryGetProperty("_source", out var source) || source.ValueKind != JsonValueKind.Object) continue;
                    var story = new JsonObject();
                    CopyValue(source, story, "id", "id");
                    CopyValue(source, story, "headline", "headline");
                    CopyNested(source, story, "source", "name", "source");
                    CopyNested(source, story, "source", "code", "sourceCode");
                    CopyNested(source, story, "mediaType", "name", "mediaType");
                    CopyNested(source, story, "series", "name", "series");
                    CopyValue(source, story, "contentType", "contentType");
                    CopyValue(source, story, "publishedOn", "publishedOn");
                    // A collapsed search returns each story's group value in 'fields'.
                    if (item.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Object)
                    {
                        var group = fields.EnumerateObject().FirstOrDefault();
                        if (group.Value.ValueKind == JsonValueKind.Array && group.Value.GetArrayLength() > 0)
                            story["group"] = JsonNode.Parse(group.Value[0].GetRawText());
                    }
                    stories.Add(story);
                }
            }
            result["stories"] = stories;
        }

        if (root.TryGetProperty("aggregations", out var aggregations) && aggregations.TryGetProperty("values", out var values))
        {
            // A nested field is counted one level down.
            if (!values.TryGetProperty("buckets", out _) && values.TryGetProperty("values", out var inner)) values = inner;
            var counts = new JsonArray();
            if (values.TryGetProperty("buckets", out var buckets) && buckets.ValueKind == JsonValueKind.Array)
            {
                foreach (var bucket in buckets.EnumerateArray())
                {
                    var key = bucket.TryGetProperty("key_as_string", out var keyText) ? keyText.GetString()
                        : bucket.TryGetProperty("key", out var k) ? (k.ValueKind == JsonValueKind.String ? k.GetString() : k.GetRawText())
                        : null;
                    var count = bucket.TryGetProperty("doc_count", out var dc) && dc.TryGetInt64(out var n) ? n : 0;
                    counts.Add(new JsonObject() { ["value"] = key, ["count"] = count });
                }
            }
            result["values"] = counts;
        }

        if (root.TryGetProperty("timed_out", out var timedOut) && timedOut.ValueKind == JsonValueKind.True)
            result["timedOut"] = true;

        return result;
    }

    /// <summary>
    /// Bring an extended query into its stored shape: an object with only bool keys, each clause
    /// list an array. Returns null when it holds no clauses.
    /// </summary>
    /// <param name="extendedQuery"></param>
    /// <returns></returns>
    /// <exception cref="FilterAIException">The extended query is not usable; the message is for the model.</exception>
    public static JsonObject? NormalizeExtendedQuery(JsonNode? extendedQuery)
    {
        if (extendedQuery == null) return null;
        if (extendedQuery is not JsonObject source) throw new FilterAIException("extendedQuery must be an object with bool keys (must, filter, must_not, should, minimum_should_match) or null.");

        // A model may wrap the clauses as { "bool": {...} } or { "query": { "bool": {...} } }.
        if (source.Count == 1 && source["query"] is JsonObject query) source = query;
        if (source.Count == 1 && source["bool"] is JsonObject boolean) source = boolean;

        var unknown = source.Select(p => p.Key).Where(k => !ExtendedQueryKeys.Contains(k)).ToArray();
        if (unknown.Length > 0)
            throw new FilterAIException($"extendedQuery may only have the keys {String.Join(", ", ExtendedQueryKeys)}; it had {String.Join(", ", unknown)}. Put whole queries inside one of those clause lists.");
        var forbidden = FindForbiddenKey(source);
        if (forbidden != null) throw new FilterAIException($"extendedQuery may not use '{forbidden}'.");

        var result = new JsonObject();
        foreach (var key in ExtendedQueryKeys.Where(k => k != "minimum_should_match"))
        {
            var value = source[key];
            if (value == null) continue;
            var clauses = value is JsonArray array ? array.DeepClone().AsArray() : new JsonArray(value.DeepClone());
            if (clauses.Any(c => c is not JsonObject)) throw new FilterAIException($"extendedQuery.{key} must be a list of query objects.");
            if (clauses.Count > 0) result[key] = clauses;
        }
        if (result.Count == 0) return null;
        if (source["minimum_should_match"] is JsonNode minimum) result["minimum_should_match"] = minimum.DeepClone();
        return result;
    }

    /// <summary>
    /// The query body used to validate an extended query, the way the browser merges it.
    /// </summary>
    /// <param name="extendedQuery"></param>
    /// <returns></returns>
    public static JsonObject BuildValidationBody(JsonObject extendedQuery)
    {
        var clause = extendedQuery.DeepClone().AsObject();
        clause["_name"] = "extendedQuery";
        return new JsonObject()
        {
            ["query"] = new JsonObject()
            {
                ["bool"] = new JsonObject()
                {
                    ["filter"] = new JsonArray(new JsonObject() { ["bool"] = clause }),
                },
            },
        };
    }

    /// <summary>
    /// The first forbidden key anywhere in the node, or null.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static string? FindForbiddenKey(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var property in obj)
                {
                    if (ForbiddenKeys.Contains(property.Key)) return property.Key;
                    var found = FindForbiddenKey(property.Value);
                    if (found != null) return found;
                }
                return null;
            case JsonArray array:
                foreach (var item in array)
                {
                    var found = FindForbiddenKey(item);
                    if (found != null) return found;
                }
                return null;
            default:
                return null;
        }
    }

    private static void CopyValue(JsonElement source, JsonObject target, string name, string targetName)
    {
        if (source.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
            target[targetName] = JsonNode.Parse(value.GetRawText());
    }

    private static void CopyNested(JsonElement source, JsonObject target, string parent, string name, string targetName)
    {
        if (source.TryGetProperty(parent, out var obj) && obj.ValueKind == JsonValueKind.Object)
            CopyValue(obj, target, name, targetName);
    }
    #endregion
}
