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
        var tokens = search.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var startOfPhrase = -1;
        var endOfPhrase = -1;
        var phrase = new StringBuilder();

        if (isAdvanced)
        {
            var tokenOperators = new string[] { "and", "&&", "or", "||", "not", "!" };

            // Iterate through each token to determine whether it is a keyword that should be marked.
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                var cleanToken = token;
                if (String.IsNullOrEmpty(token)) continue;

                var isRangeStatement = RangeRegex.IsMatch(token);
                if (isRangeStatement) continue;

                var isTokenOperator = tokenOperators.Any(o => token.Equals(o, StringComparison.InvariantCultureIgnoreCase));
                if (isTokenOperator) continue;

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

                startOfPhrase = startOfPhrase == -1 && cleanToken[0] == '"' ? i : startOfPhrase;
                endOfPhrase = cleanToken[^1] == '"' && !cleanToken.EndsWith("\\\"") ? i : -1;

                cleanToken = cleanToken.Replace("\"", "");

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
            var tokenOperators = new string[] { "|", "+", "-" };

            // Iterate through each token to determine whether it is a keyword that should be marked.
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                var cleanToken = token;
                if (String.IsNullOrEmpty(token)) continue;

                var isRangeStatement = RangeRegex.IsMatch(token);
                if (isRangeStatement) continue;

                var isTokenOperator = tokenOperators.Any(o => token.Equals(o, StringComparison.InvariantCultureIgnoreCase));
                if (isTokenOperator) continue;

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

                startOfPhrase = startOfPhrase == -1 && cleanToken[0] == '"' ? i : startOfPhrase;
                endOfPhrase = cleanToken[^1] == '"' && !cleanToken.EndsWith("\\\"") ? i : -1;

                cleanToken = cleanToken.Replace("\"", "");

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
    /// </summary>
    /// <param name="text"></param>
    /// <param name="keywords"></param>
    /// <param name="mark"></param>
    /// <returns></returns>
    public static string MarkKeywords(this string text, IEnumerable<string> keywords, string tagName = "mark")
    {
        var result = new StringBuilder(text);
        var values = String.Join("|", keywords);
        var findAndReplace = Regex.Replace(result.ToString(), $@"\b({values})", match =>
        {
            var values = new List<string>();
            for (var i = 0; i < match.Groups.Count; i++)
            {
                values.Add(match.Groups[i].Value);
            }
            values = values.Distinct().ToList(); // Not clear why each match results in multiple groups even when only a single value is found.
            return $"<{tagName}>{String.Join("", values)}</{tagName}>";
        }, RegexOptions.IgnoreCase);
        result = new StringBuilder(findAndReplace);
        return result.ToString();
    }
    #endregion
}
