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
    #endregion
}
