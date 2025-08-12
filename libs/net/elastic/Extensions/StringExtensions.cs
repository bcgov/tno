using System.Text;
using System.Text.RegularExpressions;

namespace TNO.Elastic;

/// <summary>
/// StringExtensions static class, provides extensions methods for string objects.
/// </summary>
public static class StringExtensions
{
    #region Variables
    static readonly Regex RangeRegex = RegexSettings.RangeRegex();
    static readonly Regex BoostOrProximityRegex = RegexSettings.BoostOrProximityRegex();
    static readonly Regex WildCardRegex = RegexSettings.WildCardRegex();
    static readonly Regex StartOfQuote = RegexSettings.StartOfQuoteRegex();
    static readonly Regex EndOfQuote = RegexSettings.EndOfQuoteRegex();
    static readonly Regex RemoveSimpleKeywords = RegexSettings.RemoveSimpleKeywordsRegex();
    static readonly Regex RemoveAdvancedKeywords = RegexSettings.RemoveAdvancedKeywordsRegex();

    #endregion

    #region Methods
    /// <summary>
    /// Extract keywords and phrases from the Elasticsearch query 'search'.
    /// This does not currently handle escaped characters.
    /// </summary>
    /// <param name="search"></param>
    /// <param name="queryType"></param>
    /// <returns></returns>
    public static IEnumerable<string> ExtractKeywords(this string search, string queryType = "simple-query-string")
    {
        var isAdvanced = queryType == "query-string";
        var keywords = new List<string>();
        var startOfPhrase = -1;
        var endOfPhrase = -1;
        var phrase = new StringBuilder();

        if (isAdvanced)
        {
            var cleanSearch = RemoveAdvancedKeywords.Replace(search, " ");
            var tokens = cleanSearch.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            // Iterate through each token to determine whether it is a keyword that should be marked.
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                var cleanToken = token;
                if (String.IsNullOrEmpty(token)) continue;

                var isRangeStatement = RangeRegex.IsMatch(token);
                if (isRangeStatement) continue;

                // TODO: No need to include tokens that are related to a NOT
                var isNot = token[0] == '!';
                if (isNot) continue;

                var isBoostOrProximity = BoostOrProximityRegex.IsMatch(token);
                if (isBoostOrProximity)
                    cleanToken = BoostOrProximityRegex.Replace(token, "");

                var isWildcard = WildCardRegex.IsMatch(cleanToken);
                if (isWildcard && startOfPhrase == -1)
                    cleanToken = cleanToken.Replace("*", ".*").Replace("?", ".?");
                else if (isWildcard)
                    cleanToken = cleanToken.Replace("*", "").Replace("?", "");

                // TODO: This doesn't handle escaped characters.
                cleanToken = cleanToken.Replace("(", "").Replace(")", "");

                startOfPhrase = startOfPhrase == -1 && StartOfQuote.IsMatch(cleanToken) ? i : startOfPhrase;
                endOfPhrase = EndOfQuote.IsMatch(cleanToken) && !cleanToken.EndsWith("\\\"") ? i : -1;

                cleanToken = cleanToken.Replace("\"", "").Trim();
                if (String.IsNullOrWhiteSpace(cleanToken)) continue;

                if (startOfPhrase == -1)
                {
                    // Not part of a phrase, add it as a keyword if it isn't a token operator.
                    keywords.Add(cleanToken);
                }
                else if (endOfPhrase > -1)
                {
                    // Reset and add these words to a phrase.
                    phrase.Append($" {cleanToken}");
                    keywords.Add(phrase.ToString().Trim());
                    phrase.Clear();

                    startOfPhrase = -1;
                    endOfPhrase = -1;
                }
                else
                {
                    phrase.Append($" {cleanToken}");
                }
            }
        }
        else
        {
            var cleanSearch = RemoveSimpleKeywords.Replace(search, " ");
            var tokens = cleanSearch.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            // Iterate through each token to determine whether it is a keyword that should be marked.
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                var cleanToken = token;
                if (String.IsNullOrEmpty(token)) continue;

                var isRangeStatement = RangeRegex.IsMatch(token);
                if (isRangeStatement) continue;

                // TODO: No need to include tokens that are related to a NOT
                var isNot = token[0] == '-';
                if (isNot) continue;

                var isBoostOrProximity = BoostOrProximityRegex.IsMatch(token);
                if (isBoostOrProximity)
                    cleanToken = BoostOrProximityRegex.Replace(token, "");

                var isWildcard = WildCardRegex.IsMatch(cleanToken);
                if (isWildcard && startOfPhrase == -1)
                    cleanToken = cleanToken.Replace("*", ".*").Replace("?", ".?");
                else if (isWildcard)
                    cleanToken = cleanToken.Replace("*", "").Replace("?", "");

                // TODO: This doesn't handle escaped characters.
                cleanToken = cleanToken.Replace("(", "").Replace(")", "").Replace("|", "").Replace("+", "");

                endOfPhrase = startOfPhrase != -1 && EndOfQuote.IsMatch(cleanToken) && !cleanToken.EndsWith("\\\"") ? i : -1;
                startOfPhrase = startOfPhrase == -1 && StartOfQuote.IsMatch(cleanToken) ? i : startOfPhrase;

                cleanToken = cleanToken.Replace("\"", "").Trim();
                if (String.IsNullOrWhiteSpace(cleanToken)) continue;

                if (startOfPhrase == -1)
                {
                    // Not part of a phrase, add it as a keyword if it isn't a token operator.
                    keywords.Add(cleanToken);
                }
                else if (endOfPhrase > -1)
                {
                    // Reset and add these words to a phrase.
                    phrase.Append($" {cleanToken}");
                    keywords.Add(phrase.ToString().Trim());
                    phrase.Clear();

                    startOfPhrase = -1;
                    endOfPhrase = -1;
                }
                else
                {
                    phrase.Append($" {cleanToken}");
                }
            }
        }

        return keywords.ToArray();
    }

    /// <summary>
    /// Add html mark tags to each keyword.
    /// If markRawTag is false, we will use span with style. this is useful for email clients like Outlook.
    /// If markRawTag is true, we will use the raw tag name.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="keywords"></param>
    /// <param name="tagName"></param>
    /// <param name="markRawTag"></param>
    /// <returns></returns>
    public static string MarkKeywords(this string text, IEnumerable<string> keywords, string tagName = "mark", bool markRawTag = false)
    {
        const string highlightStyle = "background-color:#fff59e;color:inherit;";
        var values = String.Join("|", keywords.Where(v => !String.IsNullOrWhiteSpace(v)));
        return Regex.Replace(text, $@"\b({values})", match =>
        {
            var values = new List<string>();
            for (var i = 0; i < match.Groups.Count; i++)
            {
                values.Add(match.Groups[i].Value);
            }
            // Not clear why each match results in multiple groups even when only a single value is found.
            values = values.Distinct().Where(v => !String.IsNullOrWhiteSpace(v)).ToList();
            var result = String.Join("", values);
            if (values.Count != 0)
            {
                if (!String.IsNullOrWhiteSpace(tagName) && !markRawTag && tagName.Equals("mark", StringComparison.OrdinalIgnoreCase))
                    return $"<span style=\"{highlightStyle}\">{result}</span>";
                return $"<{tagName}>{result}</{tagName}>";
            }
            return result;
        }, RegexOptions.IgnoreCase);
    }
    #endregion
}
