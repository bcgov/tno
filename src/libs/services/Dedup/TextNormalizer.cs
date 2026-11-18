using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TNO.Core.Extensions;

namespace TNO.Services.Dedup;

/// <summary>
/// TextNormalizer static class, reduces headlines and story text to a comparable form so that
/// copies of a story that differ only by markup, entities, punctuation, case or accents compare equal.
/// </summary>
public static partial class TextNormalizer
{
    #region Variables
    /// <summary>
    /// Words that carry no meaning when comparing headlines.
    /// </summary>
    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "the", "an", "of", "in", "on", "to", "for", "and", "at", "with", "by", "is", "as", "from",
    };

    [GeneratedRegex(@"^(video|photos?|updated?|breaking|watch|live|opinion|letters?|column|editorial)\s*:\s*", RegexOptions.IgnoreCase)]
    private static partial Regex LabelPrefixRegex();

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();
    #endregion

    #region Methods
    /// <summary>
    /// Normalize a headline.
    /// Removes markup and entities, a leading label such as 'UPDATE:', accents, punctuation and case.
    /// </summary>
    /// <param name="headline"></param>
    /// <returns>Lowercase words separated by a single space.</returns>
    public static string NormalizeHeadline(string? headline)
    {
        var text = ToPlainText(headline);
        text = LabelPrefixRegex().Replace(text, "");
        return NormalizeCharacters(text);
    }

    /// <summary>
    /// Normalize story text.
    /// Removes markup and entities, accents, punctuation and case.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Lowercase words separated by a single space.</returns>
    public static string NormalizeText(string? text)
    {
        return NormalizeCharacters(ToPlainText(text));
    }

    /// <summary>
    /// Split a normalized headline into the words that identify it.
    /// Single characters and stop words are dropped.
    /// </summary>
    /// <param name="normalizedHeadline"></param>
    /// <returns></returns>
    public static HashSet<string> HeadlineTokens(string? normalizedHeadline)
    {
        if (String.IsNullOrWhiteSpace(normalizedHeadline)) return new HashSet<string>(StringComparer.Ordinal);
        return normalizedHeadline
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 1 && !StopWords.Contains(token))
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Split story text (HTML or plain) into normalized words.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string[] TextTokens(string? text)
    {
        return NormalizeText(text).Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// The Jaccard similarity of two sets (size of the intersection divided by the size of the union).
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns>A value between 0 and 1.</returns>
    public static double Jaccard(IReadOnlySet<string> first, IReadOnlySet<string> second)
    {
        if (first.Count == 0 || second.Count == 0) return 0;
        var (small, large) = first.Count <= second.Count ? (first, second) : (second, first);
        var intersection = small.Count(large.Contains);
        return (double)intersection / (first.Count + second.Count - intersection);
    }

    /// <summary>
    /// Convert HTML to plain text.
    /// Entities are decoded twice because some publishers double encode them (e.g. '&amp;#8217;').
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string ToPlainText(string? value)
    {
        if (String.IsNullOrWhiteSpace(value)) return "";
        return WebUtility.HtmlDecode(value.HtmlToPlainText()).Trim();
    }

    /// <summary>
    /// Lowercase, remove accents and apostrophes, and replace everything that is not a letter or digit with a single space.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string NormalizeCharacters(string text)
    {
        if (String.IsNullOrWhiteSpace(text)) return "";
        var decomposed = text.Normalize(NormalizationForm.FormKD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            // Apostrophes join words ("doesn’t" => "doesnt") so contractions stay one token.
            if (c is '\'' or '‘' or '’' or 'ʼ') continue;
            builder.Append(Char.ToLowerInvariant(c));
        }
        return NonAlphaNumericRegex().Replace(builder.ToString(), " ").Trim();
    }
    #endregion
}
