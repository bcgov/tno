using System.Net;
using System.Text.RegularExpressions;

namespace TNO.Core.Extensions;

/// <summary>
/// HtmlExtensions static class, provides lightweight HTML-to-text helpers for stored story bodies.
/// </summary>
public static partial class HtmlExtensions
{
    [GeneratedRegex(@"<(script|style)\b[^>]*>.*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptOrStyleRegex();

    [GeneratedRegex(@"<\s*(br|/p|/div|/li|/h[1-6]|/tr|/blockquote)\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBreakRegex();

    [GeneratedRegex(@"<!--.*?-->|<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"[ \t\f\v ]+")]
    private static partial Regex HorizontalSpaceRegex();

    [GeneratedRegex(@"\s*\n\s*(\n\s*)+")]
    private static partial Regex BlankLinesRegex();

    [GeneratedRegex(@"<img\b", RegexOptions.IgnoreCase)]
    private static partial Regex ImageTagRegex();

    /// <summary>
    /// Convert HTML to plain text: scripts and styles removed, block ends as line breaks, tags
    /// stripped, entities decoded, and runs of whitespace collapsed. Plain text passes through
    /// with only its whitespace normalized.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static string HtmlToPlainText(this string? html)
    {
        if (String.IsNullOrEmpty(html)) return "";
        var text = ScriptOrStyleRegex().Replace(html, " ");
        text = BlockBreakRegex().Replace(text, "\n");
        text = TagRegex().Replace(text, " ");
        text = WebUtility.HtmlDecode(text);
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');
        text = HorizontalSpaceRegex().Replace(text, " ");
        text = BlankLinesRegex().Replace(text, "\n\n");
        return String.Join('\n', text.Split('\n').Select(line => line.Trim())).Trim();
    }

    /// <summary>
    /// Whether the HTML contains an image element.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    public static bool ContainsHtmlImage(this string? html)
    {
        return !String.IsNullOrEmpty(html) && ImageTagRegex().IsMatch(html);
    }
}
