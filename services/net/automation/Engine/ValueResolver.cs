using System.Text.Json;
using TNO.API.Areas.Admin.Models.Automation;

namespace TNO.Services.Automation.Engine;

/// <summary>
/// ValueResolver class, resolves an action's value from its fixed source: an analysis result
/// ('name.key'), a working-copy field ('content.field'), a literal, or a token template.
/// There is no expression language - only these sources.
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
    /// Resolve an 'analysisName.key' or 'content.field' reference.
    /// </summary>
    public static string? ResolveFrom(string reference, ItemScope scope, ContentEntry? target)
    {
        var parts = reference.Split('.', 2);
        if (parts.Length != 2) return null;
        var (name, key) = (parts[0], parts[1]);

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
    /// Resolve a boolean analysis gate ('analysisName.key'); null when unavailable or not boolean.
    /// </summary>
    public static bool? ResolveBool(string reference, ItemScope scope)
    {
        var parts = reference.Split('.', 2);
        if (parts.Length != 2) return null;
        if (!scope.Structured.TryGetValue(parts[0], out var document)
            || document.RootElement.ValueKind != JsonValueKind.Object
            || !TryGetPropertyIgnoreCase(document.RootElement, parts[1], out var element))
            return null;
        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(element.GetString(), out var parsed) ? parsed : null,
            _ => null,
        };
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
