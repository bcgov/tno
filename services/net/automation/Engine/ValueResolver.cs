using System.Text.Json;
using TNO.API.Areas.Admin.Models.Automation;

namespace TNO.Services.Automation.Engine;

/// <summary>
/// ValueResolver class, resolves an action's value from its fixed source: an analysis result or
/// an earlier action's outcome ('name.key'), a working-copy field ('content.field'), a literal,
/// or a token template. There is no expression language - only these sources.
/// </summary>
public static class ValueResolver
{
    /// <summary>
    /// Resolve the specified value source for the target entry. Arrays join to a comma-separated
    /// list (the shape content.tags consumes); missing analysis keys resolve to null.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="scope">The iteration scope carrying analysis results.</param>
    /// <param name="target">The entry field references and templates read from.</param>
    /// <param name="prompts">Used for template token substitution.</param>
    /// <returns></returns>
    public static string? Resolve(ValueSource? source, ItemScope scope, ContentEntry? target, PromptBuilder prompts)
    {
        if (source == null) return null;
        if (!string.IsNullOrWhiteSpace(source.From))
            return ResolveFrom(source.From!, scope, target);
        if (source.Literal.HasValue)
            return ElementToString(source.Literal.Value);
        if (!string.IsNullOrWhiteSpace(source.Template))
            // The target goes in so '{target.field}' resolves in an action template exactly as
            // '{content.field}' does here - both read the entry the action acts on.
            return prompts.Substitute(source.Template!, null, target) is var text && target != null
                ? SubstituteFields(text, target)
                : text;
        return null;
    }

    /// <summary>
    /// Resolve an 'analysisName.key', '&lt;action name&gt;.key' or 'content.field' reference.
    /// </summary>
    public static string? ResolveFrom(string reference, ItemScope scope, ContentEntry? target)
    {
        if (SplitReference(reference, scope) is not { } split) return null;
        var (name, key) = split;

        if (name.Equals("content", StringComparison.OrdinalIgnoreCase))
            return target?.GetField(key);

        if (scope.Structured.TryGetValue(name, out var document)
            && document.RootElement.ValueKind == JsonValueKind.Object
            && TryGetPropertyIgnoreCase(document.RootElement, key, out var element))
            return ElementToString(element);

        if (scope.Raw.TryGetValue(name, out var raw) && key.Equals("value", StringComparison.OrdinalIgnoreCase))
            return raw;

        return null;
    }

    /// <summary>
    /// Resolve a boolean gate ('analysisName.key' or '&lt;action name&gt;.executed'); null when
    /// unavailable or not boolean.
    /// </summary>
    public static bool? ResolveBool(string reference, ItemScope scope)
    {
        if (SplitReference(reference, scope) is not { } split) return null;
        var (name, key) = split;
        if (!scope.Structured.TryGetValue(name, out var document)
            || document.RootElement.ValueKind != JsonValueKind.Object
            || !TryGetPropertyIgnoreCase(document.RootElement, key, out var element))
            return null;
        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(element.GetString(), out var parsed) ? parsed : null,
            _ => null,
        };
    }

    /// <summary>
    /// Split a 'name.key' reference. A result name can itself contain dots - an action that was
    /// never named publishes under its type ('content.publish.executed') - so the longest name the
    /// scope actually holds wins, and only then does the first dot decide.
    /// </summary>
    private static (string Name, string Key)? SplitReference(string reference, ItemScope scope)
    {
        string? match = null;
        foreach (var name in scope.Structured.Keys.Concat(scope.Raw.Keys))
        {
            if (reference.Length <= name.Length + 1
                || !reference.StartsWith($"{name}.", StringComparison.OrdinalIgnoreCase)) continue;
            if (match == null || name.Length > match.Length) match = name;
        }
        if (match != null) return (match, reference[(match.Length + 1)..]);
        var parts = reference.Split('.', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string? ElementToString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
        JsonValueKind.Array => string.Join(", ", element.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : item.GetRawText())
            .Where(item => !string.IsNullOrWhiteSpace(item))),
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => element.GetRawText(),
    };

    private static string SubstituteFields(string text, ContentEntry target)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"\{content\.(?<field>[a-zA-Z.]+)\}",
            match => target.GetField(match.Groups["field"].Value) ?? "");
    }
}
