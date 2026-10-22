using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIStory static class, describes a listed story to a model: the fields a filter can match,
/// and which parts of the query matched it, read from an Elasticsearch explain response.
/// </summary>
public static partial class FilterAIStory
{
    #region Variables
    /// <summary>The most characters of the summary and body quoted to the model.</summary>
    public const int MaxTextChars = 1_500;

    /// <summary>The most reasons listed from an explanation.</summary>
    public const int MaxReasons = 30;

    /// <summary>The scalar fields copied as they are.</summary>
    private static readonly string[] ScalarFields = ["id", "headline", "byline", "contentType", "status", "publishedOn", "otherSource", "section", "page", "edition", "sourceUrl"];

    /// <summary>Descriptions that are part of the scoring arithmetic rather than a matched condition.</summary>
    [GeneratedRegex(@"^(sum of|max of|min of|product of|match on required clause|# clause|score\(|idf|tf|boost|avgdl|dl|freq|k1|b, |n, |N, |weight\(FunctionScoreQuery|MatchNoDocsQuery|FunctionScoreQuery|queryNorm|fieldNorm|fieldWeight|queryWeight|match filter)", RegexOptions.IgnoreCase)]
    private static partial Regex ArithmeticRegex();

    /// <summary>A term match: weight(field:term in docId) [...]</summary>
    [GeneratedRegex(@"^weight\((?<field>[\w.]+):(?<term>.+?) in \d+\)")]
    private static partial Regex WeightRegex();

    /// <summary>A filter clause: ConstantScore(...)</summary>
    [GeneratedRegex(@"^ConstantScore\((?<clause>.+)\)$")]
    private static partial Regex ConstantScoreRegex();

    /// <summary>A date range written as epoch milliseconds.</summary>
    [GeneratedRegex(@"\[(?<from>-?\d{11,}) TO (?<to>-?\d{11,})\]")]
    private static partial Regex EpochRangeRegex();
    #endregion

    #region Methods
    /// <summary>
    /// Describe a story to the model: its headline, source, media type, dates, tags, actions,
    /// tone and the start of its text.
    /// </summary>
    /// <param name="source">The story's _source JSON.</param>
    /// <param name="maxTextChars">The most characters of the summary and body to keep.</param>
    /// <returns></returns>
    public static JsonObject Summarize(string source, int maxTextChars = MaxTextChars)
    {
        using var document = JsonDocument.Parse(source);
        var root = document.RootElement;
        var story = new JsonObject();
        if (root.ValueKind != JsonValueKind.Object) return story;

        foreach (var field in ScalarFields)
            if (root.TryGetProperty(field, out var value) && value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
                story[field] = JsonNode.Parse(value.GetRawText());

        story["source"] = Reference(root, "source", "code");
        story["mediaType"] = Reference(root, "mediaType", null);
        story["series"] = Reference(root, "series", null);
        story["contributor"] = Reference(root, "contributor", null);
        story["tags"] = Names(root, "tags", "code");
        story["topics"] = Names(root, "topics", "name");
        if (root.TryGetProperty("actions", out var actions) && actions.ValueKind == JsonValueKind.Array)
            story["actions"] = new JsonArray(actions.EnumerateArray()
                .Select(a => $"{Text(a, "name")}: {Text(a, "value")}")
                .Select(a => (JsonNode?)a).ToArray());
        if (root.TryGetProperty("tonePools", out var tones) && tones.ValueKind == JsonValueKind.Array)
        {
            var tone = tones.EnumerateArray().FirstOrDefault();
            if (tone.ValueKind == JsonValueKind.Object && tone.TryGetProperty("value", out var toneValue) && toneValue.ValueKind == JsonValueKind.Number)
                story["tone"] = toneValue.GetDouble();
        }
        if (root.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.String)
            story["summary"] = ReportAIPromptChat.Cap(summary.GetString() ?? "", maxTextChars);
        if (root.TryGetProperty("body", out var body) && body.ValueKind == JsonValueKind.String)
            story["body"] = ReportAIPromptChat.Cap(body.GetString() ?? "", maxTextChars);

        foreach (var key in story.Where(kv => kv.Value == null).Select(kv => kv.Key).ToArray()) story.Remove(key);
        return story;
    }

    /// <summary>
    /// Which parts of the query a story matched, from an Elasticsearch explain response: whether
    /// it matched, and the conditions that did, in plain words where the explanation allows.
    /// </summary>
    /// <param name="response">The explain response JSON.</param>
    /// <param name="maxReasons">The most reasons to keep.</param>
    /// <returns></returns>
    public static JsonObject ExplainReasons(string response, int maxReasons = MaxReasons)
    {
        using var document = JsonDocument.Parse(response);
        var root = document.RootElement;
        var result = new JsonObject
        {
            ["matched"] = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("matched", out var matched) && matched.ValueKind == JsonValueKind.True,
        };
        var reasons = new List<string>();
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("explanation", out var explanation)) Collect(explanation, reasons);
        result["reasons"] = new JsonArray(reasons.Distinct().Take(maxReasons).Select(r => (JsonNode?)r).ToArray());
        if (reasons.Count == 0)
            result["note"] = result["matched"]!.GetValue<bool>()
                ? "The query matched, but the explanation names no condition."
                : "The query does not match this story; it may have changed since the stories were listed.";
        return result;
    }

    /// <summary>
    /// Walk the explanation tree and keep the conditions that contributed.
    /// </summary>
    private static void Collect(JsonElement node, List<string> reasons)
    {
        if (node.ValueKind != JsonValueKind.Object) return;
        var value = node.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;
        if (value <= 0) return;
        var description = node.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() ?? "" : "";
        var reason = Simplify(description);
        if (reason != null) reasons.Add(reason);
        if (node.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
            foreach (var child in details.EnumerateArray()) Collect(child, reasons);
    }

    /// <summary>
    /// A matched condition in plain words, or null for scoring arithmetic.
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static string? Simplify(string description)
    {
        if (String.IsNullOrWhiteSpace(description) || ArithmeticRegex().IsMatch(description)) return null;
        var weight = WeightRegex().Match(description);
        if (weight.Success) return $"{weight.Groups["field"].Value} contains the term '{weight.Groups["term"].Value}'";
        var constant = ConstantScoreRegex().Match(description);
        if (constant.Success) return $"filter matched: {Dates(constant.Groups["clause"].Value)}";
        if (description == "*:*") return "every story matches (match_all)";
        return ReportAIPromptChat.Cap(Dates(description), 200);
    }

    /// <summary>
    /// Epoch milliseconds in a range, written as dates.
    /// </summary>
    private static string Dates(string text) => EpochRangeRegex().Replace(text, m => $"[{Date(m.Groups["from"].Value)} TO {Date(m.Groups["to"].Value)}]");

    private static string Date(string epoch)
    {
        if (!Int64.TryParse(epoch, out var ms)) return epoch;
        if (ms >= DateTimeOffset.MaxValue.ToUnixTimeMilliseconds() || ms <= DateTimeOffset.MinValue.ToUnixTimeMilliseconds()) return ms > 0 ? "the end of time" : "the beginning of time";
        var date = DateTimeOffset.FromUnixTimeMilliseconds(ms);
        return date.Year > 2900 ? "the end of time" : date.Year < 1900 ? "the beginning of time" : date.ToString("yyyy-MM-dd HH:mm 'UTC'");
    }

    private static string? Text(JsonElement element, string name)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText() : null;

    /// <summary>A nested reference as "code - name (id 1)".</summary>
    private static JsonNode? Reference(JsonElement root, string name, string? codeField)
    {
        if (!root.TryGetProperty(name, out var element) || element.ValueKind != JsonValueKind.Object) return null;
        var code = codeField != null ? Text(element, codeField) : null;
        var label = Text(element, "name") ?? "";
        var id = Text(element, "id");
        var text = String.IsNullOrEmpty(code) ? label : $"{code} - {label}";
        return id != null ? $"{text} (id {id})" : text;
    }

    /// <summary>The named field of each item of a nested list.</summary>
    private static JsonNode? Names(JsonElement root, string name, string field)
    {
        if (!root.TryGetProperty(name, out var element) || element.ValueKind != JsonValueKind.Array) return null;
        var values = element.EnumerateArray().Select(e => Text(e, field)).Where(t => !String.IsNullOrEmpty(t)).Select(t => (JsonNode?)t).ToArray();
        return values.Length > 0 ? new JsonArray(values) : null;
    }
    #endregion
}
