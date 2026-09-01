using System.Text.RegularExpressions;

namespace TNO.Elastic;

/// <summary>
/// StringExtensions static class, provides extensions methods for string objects.
/// </summary>
public static partial class RegexSettings
{
    #region Variables

    [GeneratedRegex(@":([\[\{])")]
    public static partial Regex RangeRegex();

    [GeneratedRegex(@"[\^\~][0-9]*$")]
    public static partial Regex BoostOrProximityRegex();

    [GeneratedRegex(@"[\*\?]")]
    public static partial Regex WildCardRegex();

    [GeneratedRegex("^\"")]
    public static partial Regex StartOfQuoteRegex();

    [GeneratedRegex("\"$")]
    public static partial Regex EndOfQuoteRegex();

    [GeneratedRegex(@"\s?[\+\-\|]\s?")]
    public static partial Regex RemoveSimpleKeywordsRegex();

    [GeneratedRegex(@"(\sAND\s|\sOR\s|\sNOT\s|\s?\&\&\s?|\s?\|\|\s?|\s?[\+\-\!]\s?)", RegexOptions.IgnoreCase)]
    public static partial Regex RemoveAdvancedKeywordsRegex();

    [GeneratedRegex(@"\b\w+:/(\\/|[^/])*/")]
    public static partial Regex RemoveFieldedSearchRegex();

    /// <summary>
    /// An html tag or comment. Keyword marking is applied to the text between these and never
    /// inside one - a mark written into an attribute value corrupts it, and an inline base64
    /// image 'src' is a long enough run of letters that a short keyword lands in one regularly.
    /// </summary>
    [GeneratedRegex(@"(<!--.*?-->|</?[a-zA-Z][^>]*>)", RegexOptions.Singleline)]
    public static partial Regex HtmlTagRegex();
    #endregion
}
