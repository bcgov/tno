
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
    private const string DefaultSystemPrompt = "You are a news segment tool. Read timestamped transcripts, keep clips chronological, and output only JSON suitable for ffmpeg cuts.";

    private const string DefaultPrompt = """
You will receive a transcript where each sentence already has a precise start and end timestamp (HH:MM:SS.mmm). Break the program into at most {{max_clips}} clips by selecting whole sentences only:
- Use the sentence start timestamp for `start` and the same sentence end timestamp for `end`.
- Never merge sentences, invent timestamps, or allow clips to overlap.
- Return ONLY JSON of the form [{"title":string, "summary":string, "start":number, "end":number}] where start/end are expressed in seconds.
- Keep clips in chronological order so downstream ffmpeg calls can cut clean audio segments.

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
        var builder = new StringBuilder();
        foreach (var segment in transcript)
        {
            var line = $"{FormatTimestamp(segment.Start)} --> {FormatTimestamp(segment.End)} :: {segment.Text?.Trim()}";
            if (builder.Length + line.Length > limit)
                break;
            builder.AppendLine(line);
        }

        var maxClips = overrides?.MaxStories ?? _options.LlmMaxStories;
        if (maxClips <= 0) maxClips = _options.LlmMaxStories;

        return template
            .Replace("{{max_clips}}", maxClips.ToString(CultureInfo.InvariantCulture))
            .Replace("{{transcript}}", builder.ToString());
    }

    private string BuildRequestUri()
    {
        var baseUrl = _options.LlmApiUrl?.TrimEnd('/') ?? string.Empty;
        var deployment = string.IsNullOrWhiteSpace(_options.LlmDeployment) ? _options.LlmModel : _options.LlmDeployment;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(deployment))
            throw new InvalidOperationException("LLM configuration is missing the Azure OpenAI endpoint or deployment name.");

        var version = string.IsNullOrWhiteSpace(_options.LlmApiVersion) ? "2024-02-15-preview" : _options.LlmApiVersion;
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
                JsonValueKind.Object when root.Value.TryGetProperty("clips", out var clips) => clips.EnumerateArray(),
                _ => Enumerable.Empty<JsonElement>()
            };

            var list = new List<ClipDefinition>();
            foreach (var item in elements)
            {
                var title = item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? "Clip" : "Clip";
                var summary = item.TryGetProperty("summary", out var summaryElement) ? summaryElement.GetString() ?? string.Empty : string.Empty;
                var start = ReadTime(item, "start");
                var end = ReadTime(item, "end");

                if (end <= start)
                    continue;

                var snapped = SnapToTranscriptBounds(transcript, start, end);
                if (snapped == null)
                    continue;

                var (alignedStart, alignedEnd) = snapped.Value;
                list.Add(new ClipDefinition(title, summary, alignedStart, alignedEnd));
            }

            return FilterOverlaps(list);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to parse LLM segmentation response. Raw body: {body}", body);
            return Array.Empty<ClipDefinition>();
        }
    }

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
