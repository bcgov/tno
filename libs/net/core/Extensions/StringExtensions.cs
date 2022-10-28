using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// Replace the first found occurance of the 'search' with 'replace' value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="search"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public static string? ReplaceFirst(this string? text, string search, string replace)
    {
        if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(search)) return text;

        var pos = text.IndexOf(search);
        if (pos < 0) return text;
        return $"{replace}{text[(pos + search.Length)..]}";
    }

    /// <summary>
    /// Ensures the specified path is relative.
    /// Removes ':' character to reduce risk of a drive request.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string MakeRelativePath(this string? path)
    {
        path = path?.Replace('\\', Path.DirectorySeparatorChar).Replace(":", "") ?? "";

        if (path.StartsWith(Path.DirectorySeparatorChar)) return path[1..];

        return path;
    }

    /// <summary>
    /// Extracts the full directory path.
    /// Removes ':' character to reduce risk of a drive request.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string? GetDirectoryPath(this string? path)
    {
        path = path?.Replace('\\', Path.DirectorySeparatorChar).Replace(":", "") ?? "";
        if (path?.EndsWith(Path.DirectorySeparatorChar) == true) return path[..^1];
        return Path.GetDirectoryName($"{path}{Path.DirectorySeparatorChar}");
    }

    /// <summary>
    /// Determines if file exists at the specified 'path'.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static bool FileExists(this string path, bool ignoreCase = false)
    {
        var directory = Path.GetDirectoryName(path) ?? "";
        return directory.DirectoryExists() && File.Exists(path) && Directory.GetFiles(directory).Any(f => String.Compare(f, path, ignoreCase) == 0);
    }

    /// <summary>
    /// Determines if directory exists at the specified 'path'.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static bool DirectoryExists(this string path, bool ignoreCase = false)
    {
        var parent = Directory.GetParent(path.GetDirectoryPath() ?? "")?.FullName ?? "";
        if (String.IsNullOrWhiteSpace(parent)) parent = $"{Path.DirectorySeparatorChar}";
        else if (!parent.StartsWith(Path.DirectorySeparatorChar) && path.StartsWith(Path.DirectorySeparatorChar)) parent = $"{Path.DirectorySeparatorChar}{parent}";
        return Directory.Exists(path) && Directory.GetFileSystemEntries(parent).Any(f => String.Compare(f, path, ignoreCase) == 0);
    }

    /// <summary>
    /// Extract the article text from the HTML document articleContent and remove all tags. This produces a rather
    /// monolithic block of text. This method can be modified to refine the appearance of the output once we have
    /// a subscriber interface to work with.
    /// </summary>
    /// <param name="articleContent">HTML encoded news article</param>
    /// <returns>Article text only with tags removed</returns>
    public static string SanitizeContent(string articleContent)
    {
        int startPos = articleContent.IndexOf("<ARTICLE>", StringComparison.CurrentCultureIgnoreCase);
        int endPos = articleContent.IndexOf("</ARTICLE>", StringComparison.CurrentCultureIgnoreCase);

        const string pEndingTagReplacer = "[|]";
        var articleStr = (startPos > 0 ?
            articleContent.Substring(startPos + 9, endPos - (startPos + 9)) :
            articleContent)
            .Replace("</p>", pEndingTagReplacer, true, null);

        return Regex.Replace(articleStr, @"<[^>]*>", string.Empty)
            .Replace("\r\n", string.Empty, true, null)
            .Replace("\n", " ", true, null)
            .Replace(pEndingTagReplacer, "\n\n")
            .Trim();
    }

    /// <summary>
    /// remove specified tags. 
    /// </summary>
    /// <param name="articleContent">HTML encoded news article</param>
    /// <param name="tagName">html tag that needs to be removed, for example, <p>|</p> for paragraph tags</param>
    /// <param name="replaceString">replacement</param>
    /// <returns>Article text only with specified tags removed</returns>
    public static string SanitizeContent(string articleContent, string tagName, string replaceString = "")
    {
        Regex rgx = new Regex(tagName);
        var res = rgx.Replace(articleContent, replaceString).Trim();
        // remove extra news lines
        res = Regex.Replace(res, @"^\s+$[" + Environment.NewLine +"]*", string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline);
        return res;
    }
}
