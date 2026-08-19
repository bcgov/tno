using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TNO.API.Areas.Admin.Models.Automation.V2;

/// <summary>
/// V2ConditionResult record, the outcome of a condition evaluation, with a human-readable detail
/// of what was compared so a failed gate is explainable in the run log.
/// </summary>
/// <param name="Passed">Whether the condition passed.</param>
/// <param name="Detail">What was evaluated (field, operator, actual vs compared values).</param>
public record V2ConditionResult(bool Passed, string Detail);

/// <summary>
/// V2ConditionEvaluator class, evaluates declarative property conditions against an item's
/// working copy. Pure - the caller supplies a field resolver and (optionally) an analysis-result
/// resolver for 'from' gates. Field comparisons are case-insensitive; numeric operators coerce
/// both sides with invariant culture.
/// </summary>
public static class V2ConditionEvaluator
{
    /// <summary>
    /// Evaluate the specified condition.
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="field">Resolves a working-copy field to its current string value (null when absent).</param>
    /// <param name="from">Resolves an 'analysisName.key' reference to a boolean analysis result (null when unavailable).</param>
    /// <returns></returns>
    public static V2ConditionResult Evaluate(V2ConditionDefinition condition, Func<string, string?> field, Func<string, bool?>? from = null)
    {
        if (condition.All is { Count: > 0 })
        {
            foreach (var child in condition.All)
            {
                var result = Evaluate(child, field, from);
                if (!result.Passed) return new(false, $"all: failed at [{result.Detail}]");
            }
            return new(true, "all: passed");
        }
        if (condition.Any is { Count: > 0 })
        {
            var details = new List<string>();
            foreach (var child in condition.Any)
            {
                var result = Evaluate(child, field, from);
                if (result.Passed) return new(true, $"any: passed at [{result.Detail}]");
                details.Add(result.Detail);
            }
            return new(false, $"any: none passed ({string.Join("; ", details)})");
        }
        if (condition.Not != null)
        {
            var result = Evaluate(condition.Not, field, from);
            return new(!result.Passed, $"not: [{result.Detail}]");
        }
        if (!string.IsNullOrWhiteSpace(condition.From))
        {
            var answer = from?.Invoke(condition.From);
            return answer == null
                ? new(false, $"from {condition.From}: no boolean result available")
                : new(answer.Value, $"from {condition.From}: {answer.Value}");
        }
        if (string.IsNullOrWhiteSpace(condition.Field) || string.IsNullOrWhiteSpace(condition.Op))
            return new(false, "invalid condition: field and op are required");

        var actual = field(condition.Field!);
        return EvaluateLeaf(condition.Field!, condition.Op!, condition.Value, actual);
    }

    private static V2ConditionResult EvaluateLeaf(string fieldName, string op, JsonElement? value, string? actual)
    {
        var expectedText = DescribeValue(value);
        bool passed;
        switch (op)
        {
            case V2ConditionOps.Exists:
                passed = !string.IsNullOrWhiteSpace(actual);
                break;
            case V2ConditionOps.IsEmpty:
                passed = string.IsNullOrWhiteSpace(actual);
                break;
            case V2ConditionOps.Equals_:
                passed = string.Equals(actual ?? "", AsString(value), StringComparison.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.NotEquals:
                passed = !string.Equals(actual ?? "", AsString(value), StringComparison.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.In:
                passed = AsStrings(value).Contains(actual?.Trim() ?? "", StringComparer.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.NotIn:
                passed = !AsStrings(value).Contains(actual?.Trim() ?? "", StringComparer.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.Contains:
                passed = (actual ?? "").Contains(AsString(value), StringComparison.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.StartsWith:
                passed = (actual ?? "").StartsWith(AsString(value), StringComparison.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.Matches:
                try
                {
                    passed = Regex.IsMatch(actual ?? "", AsString(value), RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                }
                catch (Exception)
                {
                    return new(false, $"{fieldName} matches {expectedText}: invalid pattern or timeout");
                }
                break;
            case V2ConditionOps.LengthLessThan:
                passed = (actual ?? "").Length < AsNumber(value);
                break;
            case V2ConditionOps.LengthGreaterThan:
                passed = (actual ?? "").Length > AsNumber(value);
                break;
            case V2ConditionOps.GreaterThan:
                passed = TryNumber(actual, out var gt) && gt > AsNumber(value);
                break;
            case V2ConditionOps.LessThan:
                passed = TryNumber(actual, out var lt) && lt < AsNumber(value);
                break;
            // Token-list membership: the digest carries 'tags' and 'actions' as comma-separated lists.
            case V2ConditionOps.HasTag:
            case V2ConditionOps.HasAction:
                passed = (actual ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Contains(AsString(value), StringComparer.OrdinalIgnoreCase);
                break;
            case V2ConditionOps.StatusIs:
                passed = string.Equals(actual ?? "", AsString(value), StringComparison.OrdinalIgnoreCase);
                break;
            default:
                return new(false, $"unknown operator '{op}'");
        }

        var actualText = actual == null ? "<null>" : actual.Length > 80 ? $"\"{actual[..80]}…\" ({actual.Length} chars)" : $"\"{actual}\"";
        return new(passed, $"{fieldName} {op} {expectedText} → actual {actualText} → {(passed ? "passed" : "failed")}");
    }

    private static string AsString(JsonElement? value)
    {
        if (value == null) return "";
        return value.Value.ValueKind switch
        {
            JsonValueKind.String => value.Value.GetString() ?? "",
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.Value.GetRawText(),
            _ => value.Value.GetRawText(),
        };
    }

    private static string[] AsStrings(JsonElement? value)
    {
        if (value == null) return Array.Empty<string>();
        if (value.Value.ValueKind == JsonValueKind.Array)
            return value.Value.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.String ? e.GetString() ?? "" : e.GetRawText()).ToArray();
        // A single value behaves as a one-element list.
        return new[] { AsString(value) };
    }

    private static double AsNumber(JsonElement? value)
    {
        if (value == null) return 0;
        if (value.Value.ValueKind == JsonValueKind.Number) return value.Value.GetDouble();
        return TryNumber(AsString(value), out var number) ? number : 0;
    }

    private static bool TryNumber(string? text, out double number)
    {
        return double.TryParse((text ?? "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out number);
    }

    private static string DescribeValue(JsonElement? value)
    {
        if (value == null) return "<none>";
        var text = value.Value.GetRawText();
        return text.Length > 120 ? text[..120] + "…" : text;
    }
}
