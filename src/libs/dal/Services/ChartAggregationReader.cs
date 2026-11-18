using System.Text.Json;

namespace TNO.DAL.Services;

/// <summary>
/// ChartCell class, provides the totals for one dataset and label.
/// </summary>
public class ChartCell
{
    /// <summary>get/set - The number of stories.</summary>
    public long Count { get; set; }

    /// <summary>get/set - The sum of the sentiment values.</summary>
    public double SentimentSum { get; set; }

    /// <summary>get/set - The number of sentiment values.</summary>
    public long SentimentCount { get; set; }
}

/// <summary>
/// ChartAggregationReader class, reads Elasticsearch responses to the ChartAggregationQuery into
/// raw-keyed cells. Reading several responses into one reader merges them, which is what chunked
/// id queries and one-query-per-section dimensions need.
/// </summary>
public class ChartAggregationReader
{
    #region Variables
    private readonly IReadOnlyList<string> _levels;
    private readonly Func<string, string, string> _resolveKey;
    #endregion

    #region Properties
    /// <summary>get - The cells by (dataset key, label key).</summary>
    public Dictionary<(string Dataset, string Label), ChartCell> Cells { get; } = new();

    /// <summary>get/set - The number of stories in every response read.</summary>
    public long DocCount { get; private set; }

    /// <summary>get/set - Whether a bucket list was longer than the aggregation returns.</summary>
    public bool Truncated { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartAggregationReader, initializes with specified parameters.
    /// </summary>
    /// <param name="levels">The aggregated dimensions, outermost first.</param>
    /// <param name="resolveKey">Turns a (dimension, bucket key) into the raw key that orders and groups it.</param>
    public ChartAggregationReader(IReadOnlyList<string> levels, Func<string, string, string> resolveKey)
    {
        _levels = levels;
        _resolveKey = resolveKey;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Read the response.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="assign">Places the keys of the levels into a (dataset key, label key).</param>
    public void Read(JsonElement response, Func<string[], (string Dataset, string Label)> assign)
    {
        var total = response.TryGetProperty("hits", out var hits) && hits.TryGetProperty("total", out var totalElement)
            ? (totalElement.ValueKind == JsonValueKind.Object ? totalElement.GetProperty("value").GetInt64() : totalElement.GetInt64())
            : 0;
        DocCount += total;
        if (total == 0) return;

        response.TryGetProperty("aggregations", out var aggregations);
        ReadLevel(aggregations, total, 0, new string[_levels.Count], assign);
    }

    private void ReadLevel(JsonElement parent, long docCount, int level, string[] keys, Func<string[], (string Dataset, string Label)> assign)
    {
        if (level >= _levels.Count)
        {
            if (docCount <= 0) return;
            var position = assign(keys);
            if (!Cells.TryGetValue(position, out var cell)) Cells[position] = cell = new ChartCell();
            cell.Count += docCount;
            if (parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty("tone", out var tone))
            {
                cell.SentimentSum += tone.GetProperty("sum").GetProperty("value").GetDouble();
                cell.SentimentCount += tone.GetProperty("count").GetProperty("value").GetInt64();
            }
            return;
        }

        var dimension = _levels[level];
        var name = $"l{level}";
        if (parent.ValueKind != JsonValueKind.Object) return;

        if (parent.TryGetProperty(name, out var aggregation))
        {
            // A nested aggregation holds its buckets one level down.
            var isNested = aggregation.TryGetProperty("v", out var values);
            ReadBuckets(isNested ? values : aggregation, dimension, isNested, level, keys, assign);
        }
        if (parent.TryGetProperty(name + ChartAggregationQuery.OtherSeriesSuffix, out var other) && other.TryGetProperty("v", out var otherValues))
        {
            // Stories that have no series id are labelled with their free text series.
            ReadBuckets(otherValues, "otherSeries", false, level, keys, assign);
        }
        if (parent.TryGetProperty(name + ChartAggregationQuery.NoneSuffix, out var none))
        {
            keys[level] = "None";
            ReadLevel(none, none.GetProperty("doc_count").GetInt64(), level + 1, keys, assign);
        }
    }

    private void ReadBuckets(JsonElement aggregation, string dimension, bool isNested, int level, string[] keys, Func<string[], (string Dataset, string Label)> assign)
    {
        if (aggregation.TryGetProperty("sum_other_doc_count", out var otherCount) && otherCount.GetInt64() > 0) Truncated = true;
        if (!aggregation.TryGetProperty("buckets", out var buckets)) return;
        foreach (var bucket in buckets.EnumerateArray())
        {
            keys[level] = _resolveKey(dimension, BucketKey(bucket));
            // A nested dimension counts stories, not the nested documents.
            var container = isNested ? bucket.GetProperty("docs") : bucket;
            ReadLevel(container, container.GetProperty("doc_count").GetInt64(), level + 1, keys, assign);
        }
    }

    private static string BucketKey(JsonElement bucket)
    {
        if (bucket.TryGetProperty("key_as_string", out var keyAsString)) return keyAsString.GetString() ?? "";
        var key = bucket.GetProperty("key");
        return key.ValueKind == JsonValueKind.String ? key.GetString() ?? "" : key.GetRawText();
    }
    #endregion
}
