using System.Text.Json.Nodes;

namespace TNO.DAL.Services;

/// <summary>
/// ChartAggregationQuery static class, builds the Elasticsearch aggregations for a chart's dimensions.
/// The aggregation for level N is named "lN". A dimension that needs more than one aggregation
/// adds siblings beside it, read back by ChartAggregationReader.
/// </summary>
public static class ChartAggregationQuery
{
    /// <summary>The maximum buckets a terms aggregation returns.</summary>
    public const int TermsSize = 1000;

    /// <summary>The time zone dates are grouped in.</summary>
    public const string TimeZone = "America/Vancouver";

    /// <summary>The bucket for stories without a value.</summary>
    public const string NoneSuffix = "_none";

    /// <summary>The bucket list for stories without a series id.</summary>
    public const string OtherSeriesSuffix = "_other";

    /// <summary>
    /// Whether the dimension produces buckets of its own. 'reportSection' is one query per section instead.
    /// </summary>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static bool IsAggregated(string dimension) => !String.IsNullOrWhiteSpace(dimension) && dimension != "reportSection";

    /// <summary>
    /// Build the 'aggs' block.
    /// </summary>
    /// <param name="levels">The dimensions, outermost first.</param>
    /// <param name="includeSentiment">Whether each leaf needs the sentiment metrics.</param>
    /// <param name="byLineEnabled">Whether the keyword fields exist in the index.</param>
    /// <returns></returns>
    public static JsonObject Build(IReadOnlyList<(string Dimension, string Order)> levels, bool includeSentiment, bool byLineEnabled)
    {
        return BuildLevel(levels, 0, includeSentiment, byLineEnabled) ?? new JsonObject();
    }

    private static JsonObject? BuildLevel(IReadOnlyList<(string Dimension, string Order)> levels, int level, bool includeSentiment, bool byLineEnabled)
    {
        if (level >= levels.Count) return includeSentiment ? SentimentMetrics() : null;

        var (dimension, order) = levels[level];
        JsonObject? Sub() => BuildLevel(levels, level + 1, includeSentiment, byLineEnabled);
        var name = $"l{level}";
        var result = new JsonObject();

        switch (dimension)
        {
            case "dayMonthYear":
            case "monthDay":
                result[name] = WithSub(DateHistogram("day", "yyyyMMdd", order), Sub());
                break;
            case "monthYear":
                result[name] = WithSub(DateHistogram("month", "yyyyMM", order), Sub());
                break;
            case "year":
                result[name] = WithSub(DateHistogram("year", "yyyy", order), Sub());
                break;
            case "source":
            case "otherSource":
                result[name] = WithSub(Terms("otherSource", order), Sub());
                break;
            case "mediaType":
                result[name] = WithSub(Terms("mediaTypeId", order, JsonValue.Create(0)), Sub());
                break;
            case "contentType":
                result[name] = WithSub(Terms("contentType", order), Sub());
                break;
            case "series":
                if (byLineEnabled)
                {
                    result[name] = WithSub(Terms("seriesId", order), Sub());
                    result[name + OtherSeriesSuffix] = new JsonObject
                    {
                        ["filter"] = MustNot(new JsonObject { ["exists"] = new JsonObject { ["field"] = "seriesId" } }),
                        ["aggs"] = new JsonObject { ["v"] = WithSub(Terms("otherSeries.keyword", order, JsonValue.Create("")), Sub()) },
                    };
                }
                else result[name] = WithSub(Terms("seriesId", order, JsonValue.Create(0)), Sub());
                break;
            case "byline":
                if (!byLineEnabled) throw new InvalidOperationException("byline is not available in aggregate mode");
                result[name] = WithSub(Terms("byline.keyword", order, JsonValue.Create("")), Sub());
                break;
            case "sentiment":
                result[name] = Nested("tonePools", Terms("tonePools.value", order), Sub());
                result[name + NoneSuffix] = WithSub(new JsonObject { ["filter"] = NoTonePool() }, Sub());
                break;
            case "sentimentSimple":
                result[name] = Nested("tonePools", SentimentRanges(), Sub());
                result[name + NoneSuffix] = WithSub(new JsonObject { ["filter"] = NoTonePool() }, Sub());
                break;
            case "topicType":
                result[name] = Nested("topics", Terms("topics.topicType", order), Sub());
                break;
            case "topicName":
                result[name] = Nested("topics", Terms("topics.name", order), Sub());
                break;
            default:
                throw new InvalidOperationException($"Chart dimension '{dimension}' is not available in aggregate mode");
        }
        return result;
    }

    private static JsonObject WithSub(JsonObject aggregation, JsonObject? sub)
    {
        if (sub != null) aggregation["aggs"] = sub;
        return aggregation;
    }

    private static JsonObject DateHistogram(string interval, string format, string order) => new()
    {
        ["date_histogram"] = new JsonObject
        {
            ["field"] = "publishedOn",
            ["calendar_interval"] = interval,
            ["format"] = format,
            ["time_zone"] = TimeZone,
            ["min_doc_count"] = 1,
            ["order"] = new JsonObject { ["_key"] = NormalizeOrder(order) },
        },
    };

    private static JsonObject Terms(string field, string order, JsonValue? missing = null)
    {
        var terms = new JsonObject
        {
            ["field"] = field,
            ["size"] = TermsSize,
            ["order"] = new JsonObject { ["_key"] = NormalizeOrder(order) },
        };
        if (missing != null) terms["missing"] = missing;
        return new JsonObject { ["terms"] = terms };
    }

    private static JsonObject SentimentRanges() => new()
    {
        ["range"] = new JsonObject
        {
            ["field"] = "tonePools.value",
            ["ranges"] = new JsonArray(
                new JsonObject { ["key"] = "Negative", ["to"] = 0 },
                new JsonObject { ["key"] = "Neutral", ["from"] = 0, ["to"] = 1 },
                new JsonObject { ["key"] = "Positive", ["from"] = 1 }),
        },
    };

    /// <summary>
    /// A nested dimension counts stories, so each bucket steps back out to the story with 'reverse_nested'.
    /// </summary>
    private static JsonObject Nested(string path, JsonObject values, JsonObject? sub)
    {
        values["aggs"] = new JsonObject { ["docs"] = WithSub(new JsonObject { ["reverse_nested"] = new JsonObject() }, sub) };
        return new JsonObject
        {
            ["nested"] = new JsonObject { ["path"] = path },
            ["aggs"] = new JsonObject { ["v"] = values },
        };
    }

    private static JsonObject NoTonePool() => MustNot(new JsonObject
    {
        ["nested"] = new JsonObject
        {
            ["path"] = "tonePools",
            ["query"] = new JsonObject { ["exists"] = new JsonObject { ["field"] = "tonePools.value" } },
        },
    });

    private static JsonObject MustNot(JsonObject query) => new()
    {
        ["bool"] = new JsonObject { ["must_not"] = new JsonArray(query) },
    };

    /// <summary>
    /// Sum and count rather than an average, so chunked queries can be merged.
    /// </summary>
    private static JsonObject SentimentMetrics() => new()
    {
        ["tone"] = new JsonObject
        {
            ["nested"] = new JsonObject { ["path"] = "tonePools" },
            ["aggs"] = new JsonObject
            {
                ["sum"] = new JsonObject { ["sum"] = new JsonObject { ["field"] = "tonePools.value" } },
                ["count"] = new JsonObject { ["value_count"] = new JsonObject { ["field"] = "tonePools.value" } },
            },
        },
    };

    private static string NormalizeOrder(string order) => order == "desc" ? "desc" : "asc";
}
