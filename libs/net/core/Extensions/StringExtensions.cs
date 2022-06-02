using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TNO.Core.Extensions;

/// <summary>
/// StringExtensions static class, provides extension methods for strings.
/// </summary>
public static class StringExtensions
{
    #region Variables
    private static readonly Regex TimeZone = new("[A-Z]{3}$");
    #endregion

    /// <summary>
    /// Extracts the first letter of each word from the text and returns the value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="uppercase"></param>
    /// <returns></returns>
    public static string GetFirstLetterOfEachWord(this string text, bool uppercase = true)
    {
        var result = new StringBuilder();
        var split = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in split)
        {
            var letter = part[..1];
            result.Append(uppercase ? letter.ToUpper() : letter);
        }
        return result.ToString();
    }

    /// <summary>
    /// Checks if the current hosting environment name is Production.
    /// </summary>
    /// <param name="env">The environment name.</param>
    /// <returns>True if the environment name is Production, otherwise false.</returns>
    public static bool IsProduction(this string env)
    {
        return env != null && env.Equals("Production", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Formats the specified postal value.
    /// </summary>
    /// <param name="postal">The specified postal value</param>
    /// <returns>Postal with format XXX XXX.</returns>
    public static string? FormatAsPostal(this string postal)
    {
        if (postal?.Length == 6 && (!postal?.Contains(' ') ?? false))
            return postal?.ToUpper().Insert(3, " ");
        return postal?.ToUpper();
    }

    /// <summary>
    /// Lowercases the first character of the specified 'word'.
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public static string LowercaseFirstCharacter(this string word)
    {
        if (!String.IsNullOrWhiteSpace(word) && !char.IsUpper(word[0]))
        {
            return char.ToLower(word[0]) + (word.Length > 1 ? word[1..] : null);
        }
        return word;
    }

    /// <summary>
    /// Determine what HTTP method to use.
    /// </summary>
    /// <param name="method"></param>
    /// <return></return>
    public static HttpMethod GetHttpMethod(this string method)
    {
        return (method?.ToLower()) switch
        {
            ("get") => HttpMethod.Get,
            ("delete") => HttpMethod.Delete,
            ("put") => HttpMethod.Put,
            _ => HttpMethod.Post,
        };
    }

    /// <summary>
    /// Convert the specified 'value' from default to UTF8 encoding.
    /// Replace linebreaks with spaces.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="replaceLineBreaks"></param>
    /// <returns></returns>
    public static string? ConvertToUTF8(this string value, bool replaceLineBreaks = true)
    {
        if (value == null) return value;
        var bytes = Encoding.Default.GetBytes(replaceLineBreaks ? value.Replace("\r\n", " ") : value);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Truncate the specified 'value' to the specified 'length'.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>
    /// Remove the beginning and ending of the string of the specified 'remove'.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="remove"></param>
    /// <returns></returns>
    public static string? RemoveStartAndEnd(this string value, string remove)
    {
        var result = value;
        if (value?.StartsWith(remove) == true) result = result?[(remove.Length - 1)..];
        if (result?.EndsWith(remove) == true) result = result?[..^remove.Length];
        return result;
    }

    /// <summary>
    /// Try to parse the string 'value' as a DateTime with the default system format first, then the formats provided.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="cultureInfo"></param>
    /// <param name="dateTimeStyles"></param>
    /// <param name="formats"></param>
    /// <returns></returns>
    public static bool TryParseDateTimeExact(this string? value, CultureInfo cultureInfo, DateTimeStyles dateTimeStyles, out DateTime result, params string[] formats)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            if (DateTime.TryParse(value, out DateTime date))
            {
                result = date;
                return true;
            }

            // If the 'value' has a timezone it will need to be converted into an offset.
            // This isn't a good solution but will work for now.
            var match = TimeZone.Match(value);
            if (match.Success && match.Value != "GMT")
            {
                var abbr = match.Value;
                var timezone = abbr switch
                {
                    "EST" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),
                    "CST" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),
                    "PST" => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),
                    "NST" => TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time"),
                    "AST" => TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time"),
                    "MST" => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),
                    _ => TimeZoneInfo.Utc
                };
                value = value.Replace(abbr, $"{timezone.BaseUtcOffset.Hours}");
            }

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(value, format, cultureInfo, dateTimeStyles, out DateTime dateExact))
                {
                    result = dateExact;
                    return true;
                }
            }
        }

        result = DateTime.MinValue;
        return false;
    }
}
