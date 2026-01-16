using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.Config;
using TNO.Services.AutoClipper.LLM.Models;

namespace TNO.Services.AutoClipper.LLM;

/// <summary>
/// ClipSegmentationService class, provides a way to send a transcript to Azure Open AI to identify separate stories within the transcript.
/// </summary>
public class ClipSegmentationService : IClipSegmentationService
{
    private const string DefaultSystemPrompt = "You are a news segment tool. Analyze timestamped transcripts, choose where new stories begin, and output JSON suitable for ffmpeg clip creation.";

    private const int ParagraphSentenceCount = 4;

    private const string DefaultPrompt = """
You will receive a transcript formatted as numbered sentences (index. timestamp range :: sentence).
Identify up to {{max_clips}} places where a new story starts and return ONLY JSON:
{
  \"boundaries\": [
    {\"index\": 12, \"title\": \"slug\", \"summary\": \"recap\", \"score\": 0.82}
  ]
}

Rules:
- `index` is the numbered sentence (1-based) where the new story begins.
- `score` ranges from 0-1; higher means stronger confidence.
- Consider the optional heuristic cues before discarding a boundary.
- Keep boundaries chronological and avoid duplicates.
- Do not invent timestamps; rely only on the provided lines.

Heuristic cues (if provided):
{{heuristic_notes}}

Transcript:
{{transcript}}
""";

    private readonly HttpClient _httpClient;
    private readonly AutoClipperOptions _options;
    private readonly ILogger<ClipSegmentationService> _logger;

    public ClipSegmentationService(HttpClient httpClient, IOptions<AutoClipperOptions> options, ILogger<ClipSegmentationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClipDefinition>> GenerateClipsAsync(IReadOnlyList<TimestampedTranscript> transcript, ClipSegmentationSettings? settings, CancellationToken cancellationToken)
    {
        if (transcript == null || transcript.Count == 0)
            return [];

        if (_options.LlmApiUrl == null || string.IsNullOrWhiteSpace(_options.LlmApiKey))
            throw new InvalidOperationException("LLM configuration is missing the Azure OpenAI endpoint, or API key.");

        try
        {
            var heuristicHits = BuildHeuristicHits(transcript, settings);
            var prompt = BuildPrompt(transcript, settings, heuristicHits);
            var systemPrompt = string.IsNullOrWhiteSpace(settings?.SystemPrompt) ? DefaultSystemPrompt : settings!.SystemPrompt!;
            var payload = new
            {
                model = string.IsNullOrWhiteSpace(settings?.ModelOverride) ? _options.LlmDefaultModel : settings!.ModelOverride!,
                temperature = settings!.TemperatureOverride,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                }
            };

            var requestUri = _options.LlmApiUrl;
            _logger.LogDebug("Sending LLM segmentation request to {RequestUri} with payload: {Payload}", requestUri, JsonSerializer.Serialize(payload));
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("api-key", _options.LlmApiKey);
            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {

                var clipDefinitions = ParseResponse(body, transcript, settings, heuristicHits);
                if (clipDefinitions.Count == 0)
                {
                    _logger.LogWarning("LLM segmentation did not return any clips.");
                    return [];
                }

                return clipDefinitions;
            }
            else
            {
                var responseException = new HttpClientRequestException(response);
                _logger.LogError(responseException, "Failed to segment transcript with LLM. Error: {Details}", body);
                return [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to segment transcript with LLM. Error: {Details}", ex.GetAllMessages());
            return [];
        }
    }

    private string BuildPrompt(IReadOnlyList<TimestampedTranscript> transcript, ClipSegmentationSettings? overrides, IReadOnlyList<HeuristicHit> heuristicHits)
    {
        var template = !string.IsNullOrWhiteSpace(overrides?.PromptOverride)
            ? overrides!.PromptOverride!
            : string.IsNullOrWhiteSpace(_options.LlmPrompt) ? DefaultPrompt : _options.LlmPrompt;
        var includesHeuristicPlaceholder = template.Contains("{{heuristic_notes}}");
        var limit = ResolvePromptLimit(overrides?.PromptCharacterLimit);
        var transcriptBody = BuildPromptTranscript(transcript, limit);
        var heuristicNotes = BuildHeuristicNotes(heuristicHits, transcript);

        var maxClips = overrides?.MaxStories ?? _options.MaxStoriesFromClip;
        if (maxClips <= 0) maxClips = _options.MaxStoriesFromClip;

        var prompt = template
            .Replace("{{max_clips}}", maxClips.ToString(CultureInfo.InvariantCulture))
            .Replace("{{transcript}}", transcriptBody)
            .Replace("{{heuristic_notes}}", heuristicNotes);

        if (!includesHeuristicPlaceholder && !string.IsNullOrWhiteSpace(heuristicNotes))
        {
            prompt += "\n\nHeuristic cues (for reference):\n" + heuristicNotes;
        }

        return prompt;
    }

    private int? ResolvePromptLimit(int? overrideLimit)
    {
        if (overrideLimit.HasValue)
            return overrideLimit.Value > 0 ? overrideLimit.Value : null;

        return _options.LlmPromptCharacterLimit > 0 ? _options.LlmPromptCharacterLimit : null;
    }

    private static string BuildPromptTranscript(IReadOnlyList<TimestampedTranscript> transcript, int? limit)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Sentences:");
        for (var i = 0; i < transcript.Count; i++)
        {
            var sentence = transcript[i];
            if (string.IsNullOrWhiteSpace(sentence.Text)) continue;
            var line = $"{i + 1}. {FormatTimestamp(sentence.Start)} --> {FormatTimestamp(sentence.End)} :: {sentence.Text.Trim()}";
            if (limit.HasValue && builder.Length + line.Length > limit.Value)
                break;
            builder.AppendLine(line);
        }

        builder.AppendLine();
        builder.AppendLine("Paragraphs:");
        var paragraphNumber = 1;
        var index = 0;
        while (index < transcript.Count && (!limit.HasValue || builder.Length < limit.Value))
        {
            var start = index;
            var end = Math.Min(index + ParagraphSentenceCount, transcript.Count);
            var sentences = new List<string>();
            for (var j = start; j < end; j++)
            {
                var sentence = transcript[j];
                if (string.IsNullOrWhiteSpace(sentence.Text)) continue;
                sentences.Add(sentence.Text.Trim());
            }

            if (sentences.Count > 0)
            {
                var line = $"Paragraph {paragraphNumber} (sentences {start + 1}-{end}): {string.Join(" / ", sentences)}";
                if (limit.HasValue && builder.Length + line.Length > limit.Value) break;
                builder.AppendLine(line);
                paragraphNumber++;
            }

            index += ParagraphSentenceCount;
        }

        return builder.ToString();
    }

    private static string BuildHeuristicNotes(IReadOnlyList<HeuristicHit>? hits, IReadOnlyList<TimestampedTranscript> transcript)
    {
        if (hits == null || hits.Count == 0) return "<none>";
        var sb = new StringBuilder();
        foreach (var hit in hits.OrderBy(h => h.Index))
        {
            var sentence = transcript[hit.Index];
            var snippet = string.IsNullOrWhiteSpace(sentence.Text) ? string.Empty : sentence.Text.Trim();
            var meta = BuildHeuristicMeta(hit);
            sb.AppendLine($"Sentence {hit.Index + 1} ({FormatTimestamp(sentence.Start)}): '{hit.Pattern}'{meta} -> {snippet}");
        }
        return sb.ToString().Trim();
    }

    private static string BuildHeuristicMeta(HeuristicHit hit)
    {
        var parts = new List<string> { $"w={hit.Weight:0.00}" };
        if (!string.IsNullOrWhiteSpace(hit.Category)) parts.Add($"cat={hit.Category}");
        if (!string.IsNullOrWhiteSpace(hit.Note)) parts.Add(hit.Note!);
        return parts.Count == 0 ? string.Empty : $" [{string.Join(", ", parts)}]";
    }
    private IReadOnlyList<HeuristicHit> BuildHeuristicHits(IReadOnlyList<TimestampedTranscript> transcript, ClipSegmentationSettings? settings)
    {
        if (transcript == null || transcript.Count == 0) return Array.Empty<HeuristicHit>();
        var patternEntries = settings?.HeuristicPatternEntries;
        var legacyPatterns = settings?.KeywordPatterns;
        var baseWeight = settings?.HeuristicBoundaryWeight ?? 0;
        var hasEntryOverrides = patternEntries != null && patternEntries.Count > 0;
        if (!hasEntryOverrides && (legacyPatterns == null || legacyPatterns.Count == 0)) return Array.Empty<HeuristicHit>();
        if (!hasEntryOverrides && baseWeight <= 0) return Array.Empty<HeuristicHit>();

        var hits = new List<HeuristicHit>();
        var categoryLookup = settings?.KeywordCategories ?? new Dictionary<string, string>();

        void AddMatches(string pattern, double weight, string? category, string? note)
        {
            if (string.IsNullOrWhiteSpace(pattern) || weight <= 0) return;
            Regex regex;
            try
            {
                regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid heuristic pattern: {Pattern}", pattern);
                return;
            }

            for (var i = 0; i < transcript.Count; i++)
            {
                var textValue = transcript[i].Text;
                if (string.IsNullOrWhiteSpace(textValue)) continue;
                if (regex.IsMatch(textValue))
                    hits.Add(new HeuristicHit(i, pattern, weight, category, note));
            }
        }

        if (hasEntryOverrides)
        {
            foreach (var entry in patternEntries!)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Pattern)) continue;
                var weight = entry.Weight ?? baseWeight;
                if (weight <= 0) continue;
                var category = !string.IsNullOrWhiteSpace(entry.Category)
                    ? entry.Category
                    : (categoryLookup.TryGetValue(entry.Pattern, out var mapped) ? mapped : null);
                AddMatches(entry.Pattern, weight, category, entry.Note);
            }
        }
        else if (legacyPatterns != null)
        {
            foreach (var pattern in legacyPatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern)) continue;
                var category = categoryLookup.TryGetValue(pattern, out var mappedCategory) ? mappedCategory : null;
                AddMatches(pattern, baseWeight, category, null);
            }
        }

        return hits;
    }

    private IReadOnlyList<ClipDefinition> ParseResponse(string? body, IReadOnlyList<TimestampedTranscript> transcript, ClipSegmentationSettings? settings, IReadOnlyList<HeuristicHit> heuristicHits)
    {
        if (string.IsNullOrWhiteSpace(body)) return [];

        try
        {
            body = StripCodeFence(body);
            var doc = JsonSerializer.Deserialize<LLMResponse>(body);
            if (doc == null || doc.Choices == null || doc.Choices.Count == 0)
            {
                _logger.LogWarning("LLM response deserialization resulted in null or empty choices.");
                return [];
            }

            var candidates = new List<BoundaryCandidate>();
            foreach (var content in doc.Choices?.Select(c => c.Message?.Content).Where(c => c != null) ?? [])
            {
                if (content == null) continue;

                var boundaries = JsonSerializer.Deserialize<TranscriptBoundaries>(content!);
                if (boundaries == null || boundaries.Boundaries == null) continue;
                foreach (var boundary in boundaries.Boundaries)
                {
                    var rawIndex = boundary.Index;
                    if (rawIndex <= 0)
                        continue;
                    var zeroIndex = Math.Clamp(rawIndex - 1, 0, transcript.Count - 1);
                    var title = string.IsNullOrWhiteSpace(boundary.Title) ? "Clip" : boundary.Title;
                    var summary = string.IsNullOrWhiteSpace(boundary.Summary) ? string.Empty : boundary.Summary;
                    var category = string.IsNullOrWhiteSpace(boundary.Category) ? null : boundary.Category;
                    var score = Math.Clamp(boundary.Score, 0, 1);
                    candidates.Add(new BoundaryCandidate(zeroIndex, title, summary, score, false, category));
                }
            }

            var threshold = Math.Clamp(_options.LlmBoundaryScoreThreshold, 0, 1);
            return CreateClipDefinitions(transcript, candidates, threshold, heuristicHits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to parse LLM segmentation response. Raw body: {body}", body);
            return [];
        }
    }

    private IReadOnlyList<ClipDefinition> CreateClipDefinitions(IReadOnlyList<TimestampedTranscript> transcript, List<BoundaryCandidate> candidates, double threshold, IReadOnlyList<HeuristicHit> heuristicHits)
    {
        if (transcript == null || transcript.Count == 0)
            return [];

        var map = new Dictionary<int, BoundaryCandidate>();
        foreach (var candidate in candidates)
        {
            var index = Math.Clamp(candidate.Index, 0, transcript.Count - 1);
            if (!map.TryGetValue(index, out var existing) || candidate.Score > existing.Score)
                map[index] = candidate with { Index = index };
        }

        if (heuristicHits != null && heuristicHits.Count > 0)
        {
            foreach (var hit in heuristicHits)
            {
                var index = Math.Clamp(hit.Index, 0, transcript.Count - 1);
                var heuristicCandidate = new BoundaryCandidate(index, $"Heuristic boundary ({hit.Pattern})", string.Empty, hit.Weight, true, hit.Category);
                if (!map.TryGetValue(index, out var existing) || heuristicCandidate.Score > existing.Score)
                    map[index] = heuristicCandidate;
            }
        }

        var ordered = map.Values.OrderBy(c => c.Index).ToList();
        if (ordered.Count == 0)
            ordered.Add(new BoundaryCandidate(0, "Full Program", "AutoClipper fallback clip", 1));

        if (ordered[0].Index != 0)
            ordered.Insert(0, ordered[0] with { Index = 0, Score = 1, IsHeuristic = false });

        var filtered = new List<BoundaryCandidate>();
        foreach (var candidate in ordered)
        {
            if (candidate.Index == 0 || candidate.Score >= threshold)
                filtered.Add(candidate);
        }
        if (filtered.Count == 0)
            filtered.Add(new BoundaryCandidate(0, "Full Program", "AutoClipper fallback clip", 1));

        var list = new List<ClipDefinition>();
        for (var i = 0; i < filtered.Count; i++)
        {
            var boundary = filtered[i];
            var start = transcript[boundary.Index].Start;
            var endIndex = i + 1 < filtered.Count ? filtered[i + 1].Index : transcript.Count - 1;
            var end = i + 1 < filtered.Count ? transcript[filtered[i + 1].Index].Start : transcript[^1].End;
            if (end <= start) continue;
            var title = string.IsNullOrWhiteSpace(boundary.Title) ? $"Clip {i + 1}" : boundary.Title;
            var summary = string.IsNullOrWhiteSpace(boundary.Summary) ? string.Empty : boundary.Summary;
            var category = DetermineCategory(boundary, heuristicHits, boundary.Index, endIndex) ?? "News";
            list.Add(new ClipDefinition(title, summary, start, end, category));
            _logger.LogInformation("Boundary {BoundaryIndex}: {Title} ({Start}-{End}) Score={Score:0.00} Heuristic={IsHeuristic} Category={Category}", boundary.Index + 1, title, start, end, boundary.Score, boundary.IsHeuristic, category);
        }

        return FilterOverlaps(list);
    }

    private static string? DetermineCategory(BoundaryCandidate boundary, IReadOnlyList<HeuristicHit>? hits, int startIndex, int endIndex)
    {
        if (!string.IsNullOrWhiteSpace(boundary.Category)) return boundary.Category;
        if (hits == null || hits.Count == 0) return null;
        var best = hits
            .Where(h => h.Index >= startIndex && h.Index <= endIndex)
            .OrderByDescending(h => h.Weight)
            .ThenBy(h => h.Index)
            .FirstOrDefault(h => !string.IsNullOrWhiteSpace(h.Category));
        return best?.Category;
    }

    private sealed record BoundaryCandidate(int Index, string Title, string Summary, double Score, bool IsHeuristic = false, string? Category = null);

    private sealed record HeuristicHit(int Index, string Pattern, double Weight, string? Category, string? Note);

    private static string StripCodeFence(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return body ?? string.Empty;
        var trimmed = body.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var newline = trimmed.IndexOf('\n');
            var closingFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (newline >= 0 && closingFence > newline)
            {
                trimmed = trimmed[(newline + 1)..closingFence].Trim();
            }
        }
        return trimmed;
    }

    private static TimeSpan ReadTime(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var node)) return TimeSpan.Zero;
        if (node.ValueKind == JsonValueKind.Number && node.TryGetDouble(out var seconds)) return TimeSpan.FromSeconds(Math.Max(0, seconds));
        if (node.ValueKind == JsonValueKind.String)
        {
            var value = node.GetString();
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fromString))
                return TimeSpan.FromSeconds(Math.Max(0, fromString));
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var ts)) return ts;
        }
        return TimeSpan.Zero;
    }

    private static string FormatTimestamp(TimeSpan value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", (int)value.TotalHours, value.Minutes, value.Seconds, value.Milliseconds);
    }

    private static (TimeSpan Start, TimeSpan End)? SnapToTranscriptBounds(IReadOnlyList<TimestampedTranscript> transcript, TimeSpan start, TimeSpan end)
    {
        if (transcript == null || transcript.Count == 0) return null;
        var first = transcript.FirstOrDefault(s => s.End > start);
        var last = transcript.LastOrDefault(s => s.Start < end);
        if (first == null || last == null) return null;
        if (last.End > transcript[^1].End) last = transcript[^1];
        if (last.End <= first.Start) return null;
        return (first.Start, last.End);
    }

    private static IReadOnlyList<ClipDefinition> FilterOverlaps(IReadOnlyList<ClipDefinition> clips)
    {
        if (clips == null || clips.Count == 0) return Array.Empty<ClipDefinition>();
        var ordered = clips.OrderBy(c => c.Start).ToArray();
        var result = new List<ClipDefinition>(ordered.Length);
        var lastEnd = TimeSpan.Zero;
        foreach (var clip in ordered)
        {
            if (clip.Start < lastEnd)
                continue;
            result.Add(clip);
            lastEnd = clip.End;
        }
        return result;
    }
}












