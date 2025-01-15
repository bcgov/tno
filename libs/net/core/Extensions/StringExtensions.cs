using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace TNO.Core.Extensions;

/// <summary>
/// StringExtensions static class, provides extension methods for strings.
/// </summary>
public static partial class StringExtensions
{
    #region Variables
    [GeneratedRegex(@"[A-Z]{3}$")]
    private static partial Regex TimeZoneRegex();
    private static readonly Regex TimeZone = TimeZoneRegex();


    [GeneratedRegex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>")]
    private static partial Regex HasHtmlRegex();


    [GeneratedRegex(@"[\?\.\!\;\,\:](?![\n|""|'|\r|\r\n|\s+])")]
    private static partial Regex PunctuationRegex();


    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex LineReturnRegex();
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
    /// Replace line breaks with spaces.
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
    /// Replace the first found occurrence of the 'search' with 'replace' value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="search"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(search)) return text;

        var pos = text.IndexOf(search);
        if (pos < 0) return text;
        return $"{replace}{text[(pos + search.Length)..]}";
    }

    /// <summary>
    /// Replace the last found occurrence of the 'search' with 'replace' value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="search"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public static string ReplaceLast(this string text, string search, string replace)
    {
        int place = text.LastIndexOf(search);
        if (place == -1)
            return text;

        return text.Remove(place, search.Length).Insert(place, replace);
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
    public static string GetDirectoryPath(this string? path)
    {
        path = path?.Replace('\\', Path.DirectorySeparatorChar).Replace(":", "") ?? "";
        if (path?.EndsWith(Path.DirectorySeparatorChar) == true) return path[..^1];
        return Path.GetDirectoryName($"{path}{Path.DirectorySeparatorChar}") ?? "";
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
        var directory = new DirectoryInfo(path.GetDirectoryPath() ?? "");
        if (directory == null || !directory.Exists) return false;

        var parent = directory.Parent?.FullName ?? "";

        return Directory.GetFileSystemEntries(parent).Any(f => String.Compare(f, path, ignoreCase) == 0);
    }

    /// <summary>
    /// uses a regular expression to find all the points in text which are ends of paragraphs
    /// the wraps content in paragraph tags
    /// </summary>
    /// <param name="text"></param>
    /// <param name="paragraphRegex">regular expression that targets paragraph breaks</param>
    /// <returns>formatted body string</returns>
    public static string ConvertTextToParagraphs(string? text, string paragraphRegex)
    {
        string value = string.IsNullOrEmpty(text) ? string.Empty : text;

        if (!string.IsNullOrEmpty(value))
        {
            // pattern for any matching pair of tags
            Regex tagRegex = HasHtmlRegex();
            const string PARAGRAPH_MARKER = "##paragraph_end##`";

            // if the input string is not markup, sanitize it
            if (!tagRegex.IsMatch(value))
            {
                // Add space after the punctuation mark, except there is space/new line/quote after the punctuation mark
                var sanitizedString = PunctuationRegex().Replace(value, "$0 ");

                // replace "carriage return + line feed" OR "carriage return" OR "line feed"
                // with a placeholder first
                string result = Regex.Replace(sanitizedString, paragraphRegex, PARAGRAPH_MARKER);
                if (!result.Equals(sanitizedString, StringComparison.CurrentCultureIgnoreCase))
                {
                    // found at least one paragraph placeholder
                    value = $"<p>{result.Replace(PARAGRAPH_MARKER, "</p><p>")}</p>";
                }
            }
            else
            {
                // this is markup, so remove excess carriage returns and line feeds if found
                string result = LineReturnRegex().Replace(value, PARAGRAPH_MARKER);
                if (!result.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    // found at least one paragraph placeholder
                    value = result.Replace(PARAGRAPH_MARKER, " ");
                }
            }
        }
        return value;
    }

    /// <summary>
    /// Perform a Path.Combine but with the specified directory separator.
    /// This is required if services are running in a different OS than other services.
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <param name="directorySeparatorChar"></param>
    /// <returns></returns>
    public static string CombineWith(this string path1, string path2, char directorySeparatorChar)
    {
        return Path.Combine(path1, path2).Replace(Path.DirectorySeparatorChar, directorySeparatorChar);
    }

    /// <summary>
    /// Perform a Path.Combine but with the specified directory separator.
    /// This is required if services are running in a different OS than other services.
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <param name="path3"></param>
    /// <param name="directorySeparatorChar"></param>
    /// <returns></returns>
    public static string CombineWith(this string path1, string path2, string path3, char directorySeparatorChar)
    {
        return Path.Combine(path1, path2, path3).Replace(Path.DirectorySeparatorChar, directorySeparatorChar);
    }

    /// <summary>
    /// Perform a Path.Combine but with the specified directory separator.
    /// This is required if services are running in a different OS than other services.
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <param name="path3"></param>
    /// <param name="path4"></param>
    /// <param name="directorySeparatorChar"></param>
    /// <returns></returns>
    public static string CombineWith(this string path1, string path2, string path3, string path4, char directorySeparatorChar)
    {
        return Path.Combine(path1, path2, path3, path4).Replace(Path.DirectorySeparatorChar, directorySeparatorChar);
    }

    /// <summary>
    /// Perform a Path.Combine but with the specified directory separator.
    /// This is required if services are running in a different OS than other services.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="directorySeparatorChar"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string CombineWith(this string path, params string[] paths)
    {
        return path.CombineWith(Path.AltDirectorySeparatorChar, paths);
    }

    /// <summary>
    /// Perform a Path.Combine but with the specified directory separator.
    /// This is required if services are running in a different OS than other services.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="directorySeparatorChar"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string CombineWith(this string path, char directorySeparatorChar, params string[] paths)
    {
        var values = new string[paths.Length + 1];
        values[0] = path;
        paths.CopyTo(values, 1);
        return Path.Combine(values).Replace(Path.DirectorySeparatorChar, directorySeparatorChar);
    }

    /// <summary>
    /// Escape all characters in the specified 'value'.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Escape(this string value)
    {
        return Regex.Replace(value, @"[^a-zA-Z\d\s:]", "\\$0");
    }

    /// <summary>
    /// Determines if the email address is valid.
    /// Regrettably the following email "name@name" is valid, but will fail in most cases.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValidEmail(this string value)
    {
        try
        {
            var email = new MailAddress(value);
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Replace invalid characters in text with the specified value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="replaceWith"></param>
    /// <returns></returns>
    public static string RemoveInvalidCharacters(this string text, string? replaceWith = null)
    {
        return RemoveInvalidCharacters(text, replaceWith ?? String.Empty, Encoding.UTF8, Encoding.ASCII);
    }

    /// <summary>
    /// Replace invalid characters in text with the specified value.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="replaceWith"></param>
    /// <param name="originalEncoding"></param>
    /// <param name="outputEncoding"></param>
    /// <returns></returns>
    public static string RemoveInvalidCharacters(this string text, string? replaceWith, Encoding originalEncoding, Encoding outputEncoding)
    {
        var asAscii = outputEncoding.GetString(
            Encoding.Convert(
                originalEncoding,
                Encoding.GetEncoding(
                    outputEncoding.EncodingName,
                    new EncoderReplacementFallback(replaceWith ?? String.Empty),
                    new DecoderExceptionFallback()
                    ),
                originalEncoding.GetBytes(text)
            )
        );
        return asAscii;
    }

    /// <summary>
    /// Remove any invalid UTF-8 characters.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string RemoveInvalidUtf8Characters(this string input)
    {
        // Convert the input string to a byte array
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        // Use a StringBuilder to construct the valid string
        var validString = new StringBuilder();

        // Decode the byte array while ignoring invalid bytes
        Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
        int charCount = utf8Decoder.GetCharCount(bytes, 0, bytes.Length, true);
        char[] chars = new char[charCount];
        utf8Decoder.GetChars(bytes, 0, bytes.Length, chars, 0, true);

        // Construct a string from the valid characters
        validString.Append(chars);
        return validString.ToString();
    }

    /// <summary>
    /// Remove the specified invalid unicode characters.
    /// Default removal of �
    /// </summary>
    /// <param name="text"></param>
    /// <param name="invalidChars"></param>
    /// <returns></returns>
    public static string RemoveInvalidUnicodeCharacters(this string text, params char[] invalidChars)
    {
        if (!invalidChars.Any()) invalidChars = new[] { '\uFFFD' };
        return invalidChars.Aggregate(text, (c1, c2) => c1.Replace(c2.ToString(), String.Empty));
    }
}
