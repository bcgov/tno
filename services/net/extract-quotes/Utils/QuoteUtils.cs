using System;

namespace TNO.Services.ExtractQuotes.Utils
{
    /// <summary>
    /// QuoteUtils class, provides utility methods for working with quotes.
    /// </summary>
    public static class QuoteUtils
    {
        /// <summary>
        /// Normalizes a quote by trimming whitespace, removing trailing punctuation, and converting to lowercase.
        /// </summary>
        /// <param name="quote">The original quote text.</param>
        /// <returns>The normalized quote text.</returns>
        public static string NormalizeQuote(string quote)
        {
            if (string.IsNullOrEmpty(quote))
                return string.Empty;

            // Trim whitespace
            var normalized = quote.Trim();

            // Remove trailing punctuation
            normalized = normalized.TrimEnd('.', ',', ';', ':', '!', '?');

            // Remove trailing quotes
            normalized = normalized.TrimEnd('"', '\'');

            // Convert to lowercase for case-insensitive comparison
            return normalized.ToLowerInvariant();
        }
    }
}
