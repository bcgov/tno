using TNO.Core.Exceptions;

namespace TNO.Core.Reporting;

/// <summary>
/// Shared limits for report preflight, rendering and delivery. Ten million UTF-16 characters
/// leave room for source models, Razor buffers and JSON escaping in a reporting worker.
/// The rendered output is bounded exactly while the template writes it; the preflight estimate
/// is a lower bound of that output, so it only rejects a report that cannot possibly fit.
/// </summary>
public static class ReportSizeLimit
{
    /// <summary>
    /// Maximum estimated input size or rendered output length, in characters.
    /// </summary>
    public const int MaximumCharacters = 10_000_000;

    /// <summary>
    /// Allowance per section entry for template markup, navigation and metadata, kept below what
    /// the smallest story template writes so the estimate never exceeds the real output.
    /// Entries repeated in different sections count separately.
    /// </summary>
    public const int StoryOverheadCharacters = 256;

    /// <summary>
    /// Reject an oversized estimate or output before allocating another report-sized buffer.
    /// </summary>
    /// <param name="characters">Character count, accumulated using 64-bit arithmetic.</param>
    public static void Validate(long characters)
    {
        if (characters > MaximumCharacters) throw new ReportTooLargeException();
    }

    /// <summary>
    /// Replace chart placeholders only if the expanded report fits, including repeated placeholders.
    /// </summary>
    /// <param name="body">Rendered report.</param>
    /// <param name="placeholder">Chart placeholder.</param>
    /// <param name="replacement">Generated chart.</param>
    /// <returns>The expanded report.</returns>
    public static string Replace(string body, string placeholder, string replacement)
    {
        ArgumentException.ThrowIfNullOrEmpty(placeholder);
        long length = body.Length;
        var offset = 0;
        while ((offset = body.IndexOf(placeholder, offset, StringComparison.Ordinal)) >= 0)
        {
            length += (long)replacement.Length - placeholder.Length;
            offset += placeholder.Length;
        }
        Validate(length);
        return body.Replace(placeholder, replacement, StringComparison.Ordinal);
    }
}
