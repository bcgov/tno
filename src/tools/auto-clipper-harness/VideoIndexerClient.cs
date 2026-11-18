using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AutoClipperHarness;

/// <summary>
/// Client for Azure Video Indexer API.
/// Handles video upload, processing, and transcript extraction with speaker identification.
/// </summary>
public class VideoIndexerClient
{
    private const string ApiBaseUrl = "https://api.videoindexer.ai";

    private readonly HttpClient _httpClient;
    private readonly string _accountId;
    private readonly string _location;
    private readonly string _apiKey;
    private readonly int _timeoutMinutes;
    private readonly int _pollIntervalSeconds;
    private readonly ILogger? _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public VideoIndexerClient(
        HttpClient httpClient,
        string accountId,
        string location,
        string apiKey,
        int timeoutMinutes = 60,
        int pollIntervalSeconds = 30,
        ILogger? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _accountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
        _location = location ?? throw new ArgumentNullException(nameof(location));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _timeoutMinutes = timeoutMinutes > 0 ? timeoutMinutes : 60;
        _pollIntervalSeconds = pollIntervalSeconds > 0 ? pollIntervalSeconds : 30;
        _logger = logger;
    }

    /// <summary>
    /// Transcribes a media file using Azure Video Indexer.
    /// </summary>
    /// <param name="filePath">Path to the media file (video or audio).</param>
    /// <param name="request">Transcription request options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="rawJsonOutputPath">Optional path to save the raw Video Indexer JSON response for debugging.</param>
    /// <returns>List of transcript segments with optional speaker information.</returns>
    public async Task<IReadOnlyList<TranscriptSegment>> TranscribeAsync(
        string filePath,
        VideoIndexerRequest request,
        CancellationToken cancellationToken = default,
        string? rawJsonOutputPath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Media file not found", filePath);

        _logger?.LogInformation("Starting Video Indexer transcription for {File}", filePath);

        // Step 1: Get access token
        var accessToken = await GetAccessTokenAsync(cancellationToken);
        _logger?.LogDebug("Obtained access token");

        // Step 2: Upload video
        var videoId = await UploadVideoAsync(filePath, accessToken, request, cancellationToken);
        _logger?.LogInformation("Video uploaded with ID: {VideoId}", videoId);

        try
        {
            // Step 3: Wait for processing to complete
            var indexJson = await WaitForProcessingAsync(videoId, accessToken, cancellationToken);
            _logger?.LogInformation("Video processing completed");

            // Save raw JSON for debugging if path is provided
            if (!string.IsNullOrWhiteSpace(rawJsonOutputPath))
            {
                try
                {
                    // Ensure directory exists
                    var dir = Path.GetDirectoryName(rawJsonOutputPath);
                    if (!string.IsNullOrWhiteSpace(dir))
                        Directory.CreateDirectory(dir);

                    // Pretty print the JSON
                    using var doc = JsonDocument.Parse(indexJson);
                    var prettyJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(rawJsonOutputPath, prettyJson, cancellationToken);
                    _logger?.LogInformation("Saved raw Video Indexer response to {Path}", rawJsonOutputPath);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to save raw JSON to {Path}", rawJsonOutputPath);
                }
            }

            // Step 4: Parse transcript
            var segments = ParseTranscript(indexJson, request.IncludeSpeakerLabels);
            _logger?.LogInformation("Parsed {Count} transcript segments", segments.Count);

            return segments;
        }
        finally
        {
            // Clean up: delete the video from Video Indexer
            await TryDeleteVideoAsync(videoId, accessToken);
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var url = $"{ApiBaseUrl}/Auth/{_location}/Accounts/{_accountId}/AccessToken?allowEdit=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to get access token: {response.StatusCode} - {body}");

        // Token is returned as a quoted string
        return body.Trim('"');
    }

    private async Task<string> UploadVideoAsync(
        string filePath,
        string accessToken,
        VideoIndexerRequest request,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(filePath);
        var videoName = $"AutoClipper-{Guid.NewGuid():N}-{fileName}";

        // Build upload URL with query parameters
        var queryParams = new List<string>
        {
            $"accessToken={Uri.EscapeDataString(accessToken)}",
            $"name={Uri.EscapeDataString(videoName)}",
            $"language={Uri.EscapeDataString(request.Language)}",
            "privacy=Private",
            "indexingPreset=AudioOnly"  // We only need audio analysis for transcription
        };

        if (!string.IsNullOrWhiteSpace(request.PersonModelId))
        {
            queryParams.Add($"personModelId={Uri.EscapeDataString(request.PersonModelId)}");
        }

        var url = $"{ApiBaseUrl}/{_location}/Accounts/{_accountId}/Videos?{string.Join("&", queryParams)}";

        _logger?.LogDebug("Uploading video to: {Url}", url.Split('?')[0]);

        using var content = new MultipartFormDataContent();
        await using var fileStream = File.OpenRead(filePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);

        using var response = await _httpClient.PostAsync(url, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to upload video: {response.StatusCode} - {body}");

        using var doc = JsonDocument.Parse(body);
        var videoId = doc.RootElement.GetProperty("id").GetString();

        if (string.IsNullOrWhiteSpace(videoId))
            throw new InvalidOperationException("Video Indexer did not return a video ID");

        return videoId;
    }

    private async Task<string> WaitForProcessingAsync(
        string videoId,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var url = $"{ApiBaseUrl}/{_location}/Accounts/{_accountId}/Videos/{videoId}/Index?accessToken={Uri.EscapeDataString(accessToken)}";
        var timeout = TimeSpan.FromMinutes(_timeoutMinutes);
        var pollInterval = TimeSpan.FromSeconds(_pollIntervalSeconds);
        var startTime = DateTime.UtcNow;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.UtcNow - startTime > timeout)
                throw new TimeoutException($"Video Indexer processing did not complete within {_timeoutMinutes} minutes");

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Failed to get video index: {response.StatusCode} - {body}");

            using var doc = JsonDocument.Parse(body);
            var state = doc.RootElement.GetProperty("state").GetString();

            var elapsed = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            _logger?.LogInformation("Video Indexer status: {State} ({Elapsed}s elapsed)", state, elapsed);

            if (string.Equals(state, "Processed", StringComparison.OrdinalIgnoreCase))
            {
                return body;
            }

            if (string.Equals(state, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                var errorMessage = "Unknown error";
                if (doc.RootElement.TryGetProperty("failureMessage", out var failureMsg))
                    errorMessage = failureMsg.GetString() ?? errorMessage;
                throw new InvalidOperationException($"Video Indexer processing failed: {errorMessage}");
            }

            await Task.Delay(pollInterval, cancellationToken);
        }
    }

    private IReadOnlyList<TranscriptSegment> ParseTranscript(string indexJson, bool includeSpeakerLabels)
    {
        var segments = new List<TranscriptSegment>();

        using var doc = JsonDocument.Parse(indexJson);

        // Navigate to: videos[0].insights.transcript
        if (!doc.RootElement.TryGetProperty("videos", out var videos) ||
            videos.GetArrayLength() == 0)
        {
            _logger?.LogWarning("No videos found in index response");
            return segments;
        }

        var video = videos[0];
        if (!video.TryGetProperty("insights", out var insights))
        {
            _logger?.LogWarning("No insights found in video");
            return segments;
        }

        // Build speaker name map from faces/speakers if available
        var speakerNames = BuildSpeakerNameMap(insights);

        // Parse transcript
        if (!insights.TryGetProperty("transcript", out var transcript) ||
            transcript.ValueKind != JsonValueKind.Array)
        {
            _logger?.LogWarning("No transcript found in insights");
            return segments;
        }

        foreach (var item in transcript.EnumerateArray())
        {
            var text = item.TryGetProperty("text", out var textProp) ? textProp.GetString() : null;
            if (string.IsNullOrWhiteSpace(text))
                continue;

            // Parse timestamps from instances array
            // Video Indexer format: { "instances": [{ "start": "0:00:00.4", "end": "0:00:08.36" }] }
            var start = TimeSpan.Zero;
            var end = TimeSpan.Zero;

            if (item.TryGetProperty("instances", out var instances) &&
                instances.ValueKind == JsonValueKind.Array &&
                instances.GetArrayLength() > 0)
            {
                var firstInstance = instances[0];
                start = ParseTimestamp(firstInstance, "start");
                end = ParseTimestamp(firstInstance, "end");
            }

            if (end <= start)
                end = start + TimeSpan.FromMilliseconds(100);

            // Parse speaker information
            int? speakerId = null;
            string? speakerName = null;

            if (includeSpeakerLabels && item.TryGetProperty("speakerId", out var speakerIdProp))
            {
                speakerId = speakerIdProp.ValueKind == JsonValueKind.Number
                    ? speakerIdProp.GetInt32()
                    : int.TryParse(speakerIdProp.GetString(), out var id) ? id : null;

                if (speakerId.HasValue && speakerNames.TryGetValue(speakerId.Value, out var name))
                {
                    speakerName = name;
                }
            }

            segments.Add(new TranscriptSegment(start, end, text.Trim(), speakerId, speakerName));
        }

        return segments.OrderBy(s => s.Start).ToList();
    }

    private Dictionary<int, string> BuildSpeakerNameMap(JsonElement insights)
    {
        var map = new Dictionary<int, string>();

        // Try to get speaker names from faces (Person Model identification)
        if (insights.TryGetProperty("faces", out var faces) && faces.ValueKind == JsonValueKind.Array)
        {
            foreach (var face in faces.EnumerateArray())
            {
                if (!face.TryGetProperty("id", out var idProp))
                    continue;

                var faceId = idProp.ValueKind == JsonValueKind.Number
                    ? idProp.GetInt32()
                    : int.TryParse(idProp.GetString(), out var id) ? id : (int?)null;

                if (!faceId.HasValue)
                    continue;

                // Get name - could be from Person Model or Unknown
                var name = face.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

                // Skip unknown faces - we'll use speaker ID instead
                if (!string.IsNullOrWhiteSpace(name) &&
                    !name.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
                {
                    map[faceId.Value] = name;
                }
            }
        }

        // Also check speakers section
        if (insights.TryGetProperty("speakers", out var speakers) && speakers.ValueKind == JsonValueKind.Array)
        {
            foreach (var speaker in speakers.EnumerateArray())
            {
                if (!speaker.TryGetProperty("id", out var idProp))
                    continue;

                var speakerId = idProp.ValueKind == JsonValueKind.Number
                    ? idProp.GetInt32()
                    : int.TryParse(idProp.GetString(), out var id) ? id : (int?)null;

                if (!speakerId.HasValue || map.ContainsKey(speakerId.Value))
                    continue;

                var name = speaker.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                if (!string.IsNullOrWhiteSpace(name) &&
                    !name.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase) &&
                    !name.StartsWith("Speaker", StringComparison.OrdinalIgnoreCase))
                {
                    map[speakerId.Value] = name;
                }
            }
        }

        return map;
    }

    private static TimeSpan ParseTimestamp(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var prop))
            return TimeSpan.Zero;

        var value = prop.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return TimeSpan.Zero;

        // Video Indexer uses format like "0:00:05.12" or "00:00:05.12"
        if (TimeSpan.TryParse(value, out var ts))
            return ts;

        // Try parsing as seconds
        if (double.TryParse(value, out var seconds))
            return TimeSpan.FromSeconds(seconds);

        return TimeSpan.Zero;
    }

    private async Task TryDeleteVideoAsync(string videoId, string accessToken)
    {
        try
        {
            var url = $"{ApiBaseUrl}/{_location}/Accounts/{_accountId}/Videos/{videoId}?accessToken={Uri.EscapeDataString(accessToken)}";
            using var response = await _httpClient.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                _logger?.LogDebug("Deleted video {VideoId} from Video Indexer", videoId);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to delete video {VideoId} from Video Indexer", videoId);
        }
    }
}
