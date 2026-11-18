using System.Text;
using System.Text.RegularExpressions;

namespace TNO.Services.Automation;

/// <summary>
/// ConfirmationMatcher class, matches an action confirmation statement against an LLM response.
/// - The `{field}` token is replaced with the action's selected content field before compiling.
/// - A statement without a `{value}` token is a literal (ordinal) substring match.
/// - A statement with `{value}` is compiled into a regular expression: literals are escaped and
///   `{value}` becomes a capture group that may span multiple lines. Newlines in the statement are
///   matched flexibly (optional surrounding whitespace).
/// </summary>
public class ConfirmationMatcher
{
    private readonly string? _literal;
    private readonly Regex? _pattern;

    /// <summary>
    /// Creates a new instance of a ConfirmationMatcher for the specified statement.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="contentField"></param>
    /// <param name="objective"></param>
    public ConfirmationMatcher(string statement, string? contentField, string? objective = null)
    {
        var resolved = (statement ?? "")
            .Replace("{field}", contentField ?? "")
            .Replace("{objective}", objective ?? "")
            .Replace("\r\n", "\n")
            .Trim();

        if (string.IsNullOrWhiteSpace(resolved))
        {
            _literal = null;
            _pattern = null;
            return;
        }

        if (!resolved.Contains("{value}"))
        {
            _literal = resolved;
            return;
        }

        var parts = resolved.Split("{value}");
        var pattern = new StringBuilder();
        for (var index = 0; index < parts.Length; index++)
        {
            // Escape the literal and allow flexible whitespace around newlines.
            var escaped = Regex.Escape(parts[index]).Replace("\n", @"\s*\n\s*");
            pattern.Append(escaped);
            if (index < parts.Length - 1)
            {
                // Use a lazy multi-line capture when bounded by a trailing literal; greedy otherwise.
                var isTrailing = index == parts.Length - 2 && parts[^1].Length == 0;
                pattern.Append(isTrailing ? @"([\s\S]+)" : @"([\s\S]+?)");
            }
        }

        _pattern = new Regex(pattern.ToString(), RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// get - Whether the statement is valid and can be matched.
    /// </summary>
    public bool IsValid => _literal != null || _pattern != null;

    /// <summary>
    /// Attempt to match the statement against the specified response.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="value">The captured value when the statement contains a `{value}` token.</param>
    /// <returns>True when the response confirms the action.</returns>
    public bool TryMatch(string response, out string? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(response) || !this.IsValid) return false;

        var normalized = response.Replace("\r\n", "\n");
        if (_literal != null)
            return normalized.Contains(_literal, StringComparison.Ordinal);

        try
        {
            var match = _pattern!.Match(normalized);
            if (!match.Success) return false;
            value = match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : null;
            return true;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
