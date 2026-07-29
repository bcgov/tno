using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TNO.Services.Automation;

/// <summary>
/// PromptComposer static class, composes the single LLM prompt for a step execution.
/// Prompts are authored as HTML (Quill); they are converted to text and the runtime tokens are
/// replaced ({content}, {content.&lt;path&gt;}, {results}, {results[i].&lt;path&gt;}, {actions}, {field}).
/// </summary>
public static partial class PromptComposer
{
    [GeneratedRegex(@"<\s*(br|/p|/li|/ul|/ol|/h[1-6]|/pre|/div)\s*/?\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreakTagRegex();

    [GeneratedRegex(@"<\s*li\s*[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ListItemTagRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\{content(?:\.([A-Za-z0-9_.]+))?\}")]
    private static partial Regex ContentTokenRegex();

    [GeneratedRegex(@"\{results(?:\[(\d+)\])?(?:\.([A-Za-z0-9_.]+))?\}")]
    private static partial Regex ResultsTokenRegex();

    /// <summary>
    /// Convert prompt HTML (Quill) to plain text, preserving line structure.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";

        var text = html.Replace("\r\n", "\n");
        text = ListItemTagRegex().Replace(text, "- ");
        text = LineBreakTagRegex().Replace(text, "\n");
        text = TagRegex().Replace(text, "");
        text = WebUtility.HtmlDecode(text);

        // Collapse three or more consecutive newlines into a maximum of two.
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return text.Trim();
    }

    /// <summary>
    /// Compose the full prompt for a step execution.
    /// </summary>
    /// <param name="stepPromptHtml">The step prompt (HTML).</param>
    /// <param name="actionPrompts">The enabled action prompts (HTML) with their optional content field.</param>
    /// <param name="contentJson">The serialized content item JSON, or null when there is no iterated content.</param>
    /// <param name="resultsJson">The serialized step filter results JSON, or null when there are none.</param>
    /// <returns></returns>
    public static string Compose(
        string stepPromptHtml,
        IEnumerable<(string PromptHtml, string? ContentField, string? Objective)> actionPrompts,
        string? contentJson,
        string? resultsJson)
    {
        var stepText = HtmlToText(stepPromptHtml);
        var actionsText = string.Join(
            "\n",
            actionPrompts.Select(
                (action, index) =>
                    $"{index + 1}. {ReplaceObjectiveToken(ReplaceFieldToken(HtmlToText(action.PromptHtml), action.ContentField), action.Objective)}"));

        var text = stepText.Contains("{actions}")
            ? stepText.Replace("{actions}", actionsText)
            : $"{stepText}\n\n## Actions\n{actionsText}";

        text = ReplaceContentTokens(text, contentJson);
        text = ReplaceResultsTokens(text, resultsJson);
        return text;
    }

    /// <summary>
    /// Compose the system prompt for a chat-conversation step: the step prompt with tokens
    /// replaced and no actions section (each action is sent as its own user message).
    /// </summary>
    /// <param name="stepPromptHtml">The step prompt (HTML).</param>
    /// <param name="contentJson">The serialized content item JSON, or null.</param>
    /// <param name="resultsJson">The serialized step filter results JSON, or null.</param>
    /// <returns></returns>
    public static string ComposeSystem(string stepPromptHtml, string? contentJson, string? resultsJson)
    {
        var text = HtmlToText(stepPromptHtml).Replace("{actions}", "");
        text = ReplaceContentTokens(text, contentJson);
        text = ReplaceResultsTokens(text, resultsJson);
        return text.Trim();
    }

    /// <summary>
    /// Compose a single action's prompt (a user message in a chat-conversation step) with all
    /// tokens replaced.
    /// </summary>
    /// <param name="actionPromptHtml">The action prompt (HTML).</param>
    /// <param name="contentField">The action's selected content field.</param>
    /// <param name="objective">The action's objective.</param>
    /// <param name="contentJson">The serialized content item JSON, or null.</param>
    /// <param name="resultsJson">The serialized step filter results JSON, or null.</param>
    /// <returns></returns>
    public static string ComposeAction(
        string actionPromptHtml,
        string? contentField,
        string? objective,
        string? contentJson,
        string? resultsJson)
    {
        var text = ReplaceObjectiveToken(ReplaceFieldToken(HtmlToText(actionPromptHtml), contentField), objective);
        text = ReplaceContentTokens(text, contentJson);
        text = ReplaceResultsTokens(text, resultsJson);
        return text.Trim();
    }

    /// <summary>
    /// Resolve {content} / {content.&lt;path&gt;} tokens in an arbitrary string (e.g. an Extract Data
    /// row value that copies a content property directly).
    /// </summary>
    /// <param name="text"></param>
    /// <param name="contentJson"></param>
    /// <returns></returns>
    public static string ResolveContentTokens(string text, string? contentJson) => ReplaceContentTokens(text, contentJson);

    /// <summary>
    /// Replace the {field} token with the action's selected content field.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="contentField"></param>
    /// <returns></returns>
    public static string ReplaceFieldToken(string text, string? contentField)
    {
        return string.IsNullOrWhiteSpace(contentField) ? text : text.Replace("{field}", contentField);
    }

    /// <summary>
    /// Replace the {objective} token with the action's scoring objective.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="objective"></param>
    /// <returns></returns>
    public static string ReplaceObjectiveToken(string text, string? objective)
    {
        return string.IsNullOrWhiteSpace(objective) ? text : text.Replace("{objective}", objective);
    }

    private static string ReplaceContentTokens(string text, string? contentJson)
    {
        JsonElement? root = TryParse(contentJson);
        return ContentTokenRegex().Replace(text, (match) =>
        {
            var path = match.Groups[1].Value;
            if (string.IsNullOrEmpty(path)) return contentJson ?? "";
            return root != null ? ResolvePath(root.Value, path) : "";
        });
    }

    private static string ReplaceResultsTokens(string text, string? resultsJson)
    {
        JsonElement? root = TryParse(resultsJson);
        return ResultsTokenRegex().Replace(text, (match) =>
        {
            var indexValue = match.Groups[1].Value;
            var path = match.Groups[2].Value;
            if (string.IsNullOrEmpty(indexValue) && string.IsNullOrEmpty(path)) return resultsJson ?? "";
            if (root == null || root.Value.ValueKind != JsonValueKind.Array) return "";

            if (!int.TryParse(indexValue, out var index)) index = 0;
            if (index >= root.Value.GetArrayLength()) return "";
            var item = root.Value[index];
            return string.IsNullOrEmpty(path) ? item.GetRawText() : ResolvePath(item, path);
        });
    }

    private static JsonElement? TryParse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonDocument.Parse(json).RootElement;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Resolve a dot separated property path within the specified JSON element.
    /// Property names are matched exactly, then camelCase.
    /// </summary>
    private static string ResolvePath(JsonElement element, string path)
    {
        var current = element;
        foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object) return "";
            if (!current.TryGetProperty(part, out var next) &&
                !current.TryGetProperty(ToCamelCase(part), out next))
                return "";
            current = next;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString() ?? "",
            JsonValueKind.Null or JsonValueKind.Undefined => "",
            _ => current.GetRawText(),
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0])) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
