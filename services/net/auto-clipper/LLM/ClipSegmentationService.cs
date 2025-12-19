
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.LLM;

public class ClipSegmentationService : IClipSegmentationService
{
    private const string DefaultSystemPrompt = "You are a news segment tool. Analyse timestamped transcripts, choose where new stories begin, and output JSON suitable for ffmpeg clip creation.";

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
- Keep boundaries chronological and avoid duplicates.
- Do not invent timestamps; rely only on the provided lines.

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
            return Array.Empty<ClipDefinition>();

        if (string.IsNullOrWhiteSpace(_options.LlmApiUrl) || string.IsNullOrWhiteSpace(_options.LlmApiKey) || string.IsNullOrWhiteSpace(_options.LlmDeployment))
            return new[] { BuildFallbackClip(transcript) };

        try
        {
            var prompt = BuildPrompt(transcript, settings);
            var systemPrompt = string.IsNullOrWhiteSpace(settings?.SystemPrompt) ? DefaultSystemPrompt : settings!.SystemPrompt!;
            var payload = new
            {
                model = string.IsNullOrWhiteSpace(settings?.ModelOverride) ? _options.LlmModel : settings!.ModelOverride!,
                temperature = _options.LlmTemperature,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                }
            };

            var requestUri = BuildRequestUri();
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("api-key", _options.LlmApiKey);
            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            var clipDefinitions = ParseResponse(body, transcript);
            if (clipDefinitions.Count == 0)
            {
                _logger.LogWarning("LLM segmentation did not return any clips. Falling back to a single clip definition.");
                return new[] { BuildFallbackClip(transcript) };
            }

            return clipDefinitions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to segment transcript with LLM. Falling back to a single clip definition.");
            return new[] { BuildFallbackClip(transcript) };
        }
    }

    private ClipDefinition BuildFallbackClip(IReadOnlyList<TimestampedTranscript> transcript)
    {
        var end = transcript[^1].End;
        return new ClipDefinition("Full Program", "AutoClipper fallback clip", TimeSpan.Zero, end);
    }

    private string BuildPrompt(IReadOnlyList<TimestampedTranscript> transcript, ClipSegmentationSettings? overrides)
    {
        var template = !string.IsNullOrWhiteSpace(overrides?.PromptOverride)
            ? overrides!.PromptOverride!
            : string.IsNullOrWhiteSpace(_options.LlmPrompt) ? DefaultPrompt : _options.LlmPrompt;
        var limit = overrides?.PromptCharacterLimit ?? Math.Max(1000, _options.LlmPromptCharacterLimit);
        var transcriptBody = BuildPromptTranscript(transcript, limit);

        var maxClips = overrides?.MaxStories ?? _options.LlmMaxStories;
        if (maxClips <= 0) maxClips = _options.LlmMaxStories;

        return template
            .Replace("{{max_clips}}", maxClips.ToString(CultureInfo.InvariantCulture))
            .Replace("{{transcript}}", transcriptBody);
    }

    private string BuildPromptTranscript(IReadOnlyList<TimestampedTranscript> transcript, int limit)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < transcript.Count; i++)
        {
            var sentence = transcript[i];
            if (string.IsNullOrWhiteSpace(sentence.Text)) continue;
            var line = $"{i + 1}. {FormatTimestamp(sentence.Start)} --> {FormatTimestamp(sentence.End)} :: {sentence.Text.Trim()}";
            if (builder.Length + line.Length > limit)
                break;
            builder.AppendLine(line);
        }
        return builder.ToString();
    }

    private string BuildRequestUri()
    {
        var baseUrl = _options.LlmApiUrl?.TrimEnd('/') ?? string.Empty;
        var deployment = string.IsNullOrWhiteSpace(_options.LlmDeployment) ? _options.LlmModel : _options.LlmDeployment;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(deployment))
            throw new InvalidOperationException("LLM configuration is missing the Azure OpenAI endpoint or deployment name.");

        var version = string.IsNullOrWhiteSpace(_options.LlmApiVersion) ? "2024-07-18" : _options.LlmApiVersion;
        var path = $"{baseUrl}/openai/deployments/{deployment}/chat/completions";
        return string.IsNullOrWhiteSpace(version) ? path : $"{path}?api-version={version}";
    }

    private IReadOnlyList<ClipDefinition> ParseResponse(string? body, IReadOnlyList<TimestampedTranscript> transcript)
    {
        if (string.IsNullOrWhiteSpace(body)) return Array.Empty<ClipDefinition>();

        try
        {
            body = StripCodeFence(body);
            using var document = JsonDocument.Parse(body);
            JsonElement? root = document.RootElement;
            if (document.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentElement))
                {
                    var contentString = StripCodeFence(contentElement.GetString() ?? "[]");
                    root = JsonDocument.Parse(contentString).RootElement;
                }
            }

            var elements = root?.ValueKind switch
            {
                JsonValueKind.Array => root.Value.EnumerateArray(),
                JsonValueKind.Object when root.Value.TryGetProperty("boundaries", out var boundaries) => boundaries.EnumerateArray(),
                _ => Enumerable.Empty<JsonElement>()
            };

            var candidates = new List<BoundaryCandidate>();
            foreach (var item in elements)
            {
                if (!item.TryGetProperty("index", out var indexElement) || !indexElement.TryGetInt32(out var rawIndex) || rawIndex <= 0)
                    continue;
                var zeroIndex = Math.Clamp(rawIndex - 1, 0, transcript.Count - 1);
                var title = item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? "Clip" : "Clip";
                var summary = item.TryGetProperty("summary", out var summaryElement) ? summaryElement.GetString() ?? string.Empty : string.Empty;
                var score = item.TryGetProperty("score", out var scoreElement) && scoreElement.TryGetDouble(out var rawScore) ? Math.Clamp(rawScore, 0, 1) : 1.0;
                candidates.Add(new BoundaryCandidate(zeroIndex, title, summary, score));
            }

            var threshold = Math.Clamp(_options.LlmBoundaryScoreThreshold, 0, 1);
            return CreateClipDefinitions(transcript, candidates, threshold);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to parse LLM segmentation response. Raw body: {body}", body);
            return Array.Empty<ClipDefinition>();
        }
    }

    private IReadOnlyList<ClipDefinition> CreateClipDefinitions(IReadOnlyList<TimestampedTranscript> transcript, List<BoundaryCandidate> candidates, double threshold)
    {
        if (transcript == null || transcript.Count == 0)
            return Array.Empty<ClipDefinition>();

        var map = new Dictionary<int, BoundaryCandidate>();
        foreach (var candidate in candidates)
        {
            var index = Math.Clamp(candidate.Index, 0, transcript.Count - 1);
            if (!map.TryGetValue(index, out var existing) || candidate.Score > existing.Score)
                map[index] = candidate with { Index = index };
        }

        var ordered = map.Values.OrderBy(c => c.Index).ToList();
        if (ordered.Count == 0)
            ordered.Add(new BoundaryCandidate(0, "Full Program", "AutoClipper fallback clip", 1));

        if (ordered[0].Index != 0)
            ordered.Insert(0, ordered[0] with { Index = 0, Score = 1 });

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
            var end = i + 1 < filtered.Count ? transcript[filtered[i + 1].Index].Start : transcript[^1].End;
            if (end <= start) continue;
            var title = string.IsNullOrWhiteSpace(boundary.Title) ? $"Clip {i + 1}" : boundary.Title;
            var summary = string.IsNullOrWhiteSpace(boundary.Summary) ? string.Empty : boundary.Summary;
            list.Add(new ClipDefinition(title, summary, start, end));
        }

        return FilterOverlaps(list);
    }

    private sealed record BoundaryCandidate(int Index, string Title, string Summary, double Score);

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
