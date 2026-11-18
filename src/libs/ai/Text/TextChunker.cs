using System.Text.RegularExpressions;
using TNO.AI.Tokens;

namespace TNO.AI.Text;

/// <summary>
/// TextChunk record, a contiguous span of a source text.
/// </summary>
/// <param name="Index">The chunk's position.</param>
/// <param name="Start">Offset of the first character in the source text.</param>
/// <param name="Length">The number of characters.</param>
/// <param name="Text">The chunk text (the source span, including any overlap).</param>
/// <param name="OverlapLength">Characters at the start repeated from the previous chunk for context.</param>
public record TextChunk(int Index, int Start, int Length, string Text, int OverlapLength = 0);

/// <summary>
/// TextChunker class, splits text into chunks that fit a token budget, on paragraph boundaries
/// first, then sentences, then words. Text is never truncated: every word of the source is in
/// exactly one chunk (only whitespace at chunk boundaries is dropped), and any overlap only repeats
/// earlier text.
/// </summary>
public static partial class TextChunker
{
    [GeneratedRegex(@"\n\s*\n")]
    private static partial Regex ParagraphBreakRegex();

    [GeneratedRegex(@"(?<=[.!?][""'”’)\]]?)\s+(?=\S)")]
    private static partial Regex SentenceBreakRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    /// Split the text into chunks of at most 'maxTokens'.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <param name="maxTokens">The most tokens a chunk may contain, including overlap.</param>
    /// <param name="estimator">Counts tokens.</param>
    /// <param name="overlapTokens">Tokens of trailing context repeated at the start of the next chunk.</param>
    /// <returns></returns>
    public static IReadOnlyList<TextChunk> Split(string? text, int maxTokens, ITokenEstimator estimator, int overlapTokens = 0)
    {
        if (String.IsNullOrEmpty(text)) return Array.Empty<TextChunk>();
        if (maxTokens <= 0) throw new ArgumentOutOfRangeException(nameof(maxTokens), "The chunk budget must be positive.");
        overlapTokens = Math.Clamp(overlapTokens, 0, maxTokens / 2);

        if (estimator.Count(text) <= maxTokens) return new[] { new TextChunk(0, 0, text.Length, text) };

        // Units no larger than the budget less the overlap, so an overlap can always be prepended.
        var unitBudget = Math.Max(1, maxTokens - overlapTokens);
        var units = new List<(int Start, int Length, int Tokens)>();
        foreach (var paragraph in Spans(text, 0, text.Length, ParagraphBreakRegex()))
            AddUnits(text, paragraph.Start, paragraph.Length, unitBudget, estimator, units);

        var chunks = new List<TextChunk>();
        var index = 0;
        while (index < units.Count)
        {
            var first = index;
            var tokens = 0;
            while (index < units.Count && (index == first || tokens + units[index].Tokens + 1 <= unitBudget))
            {
                tokens += units[index].Tokens + 1;
                index++;
            }

            var start = units[first].Start;
            var end = units[index - 1].Start + units[index - 1].Length;

            // Repeat trailing units of the previous chunk as context.
            var overlapStart = start;
            if (overlapTokens > 0 && first > 0)
            {
                var overlap = 0;
                for (var i = first - 1; i >= 0 && overlap + units[i].Tokens <= overlapTokens; i--)
                {
                    overlap += units[i].Tokens;
                    overlapStart = units[i].Start;
                }
            }

            chunks.Add(new TextChunk(chunks.Count, overlapStart, end - overlapStart, text[overlapStart..end], start - overlapStart));
        }

        // The packing sums unit estimates; confirm each chunk and split any that estimation missed.
        var result = new List<TextChunk>();
        foreach (var chunk in chunks)
        {
            if (chunk.Length > 1 && estimator.Count(chunk.Text) > maxTokens)
            {
                var own = chunk.Text[chunk.OverlapLength..];
                foreach (var part in Split(own, Math.Max(1, maxTokens * 3 / 4), estimator))
                    result.Add(new TextChunk(result.Count, chunk.Start + chunk.OverlapLength + part.Start, part.Length, part.Text));
            }
            else result.Add(chunk with { Index = result.Count });
        }
        return result;
    }

    /// <summary>
    /// Add the span as units no larger than the budget: whole, then by sentence, then by word, and
    /// as a last resort by characters.
    /// </summary>
    private static void AddUnits(string text, int start, int length, int budget, ITokenEstimator estimator, List<(int Start, int Length, int Tokens)> units)
    {
        if (length <= 0) return;
        var tokens = estimator.Count(text.Substring(start, length));
        if (tokens <= budget)
        {
            units.Add((start, length, tokens));
            return;
        }

        var sentences = Spans(text, start, length, SentenceBreakRegex()).ToArray();
        if (sentences.Length > 1)
        {
            foreach (var sentence in sentences) AddUnits(text, sentence.Start, sentence.Length, budget, estimator, units);
            return;
        }

        var words = Spans(text, start, length, WhitespaceRegex()).ToArray();
        if (words.Length > 1)
        {
            // Pack words into runs that fit (summing word estimates, since text without sentence
            // breaks, like a raw transcript, can be very long), rather than one unit per word.
            var runs = new List<(int Start, int End)>();
            var runStart = words[0].Start;
            var runEnd = runStart;
            var runTokens = 0;
            foreach (var word in words)
            {
                var wordTokens = estimator.Count(text.Substring(word.Start, word.Length)) + 1;
                if (runEnd > runStart && runTokens + wordTokens > budget)
                {
                    runs.Add((runStart, runEnd));
                    runStart = word.Start;
                    runTokens = 0;
                }
                runEnd = word.Start + word.Length;
                runTokens += wordTokens;
            }
            runs.Add((runStart, runEnd));

            // A single run means the estimates fit but the whole does not; split it by characters.
            if (runs.Count > 1)
            {
                foreach (var run in runs) AddUnits(text, run.Start, run.End - run.Start, budget, estimator, units);
                return;
            }
        }

        // A single run of characters larger than the budget (e.g. an encoded blob): split evenly.
        var parts = (int)Math.Ceiling(tokens / (double)budget);
        var size = (int)Math.Ceiling(length / (double)parts);
        for (var offset = 0; offset < length; offset += size)
        {
            var partLength = Math.Min(size, length - offset);
            units.Add((start + offset, partLength, estimator.Count(text.Substring(start + offset, partLength))));
        }
    }

    /// <summary>
    /// The non-empty spans between separator matches, trimmed of surrounding whitespace.
    /// </summary>
    private static IEnumerable<(int Start, int Length)> Spans(string text, int start, int length, Regex separator)
    {
        var end = start + length;
        var position = start;
        foreach (Match match in separator.Matches(text.Substring(start, length)))
        {
            var matchStart = start + match.Index;
            var span = Trim(text, position, matchStart);
            if (span.Length > 0) yield return span;
            position = matchStart + match.Length;
        }
        var last = Trim(text, position, end);
        if (last.Length > 0) yield return last;
    }

    private static (int Start, int Length) Trim(string text, int start, int end)
    {
        while (start < end && Char.IsWhiteSpace(text[start])) start++;
        while (end > start && Char.IsWhiteSpace(text[end - 1])) end--;
        return (start, end - start);
    }
}
