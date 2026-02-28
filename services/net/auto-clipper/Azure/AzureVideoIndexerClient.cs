using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Azure;

/// <summary>
/// Client for Azure Video Indexer API.
/// Handles video upload, processing, and transcript extraction with speaker identification.
/// Supports both ARM Authentication (Service Principal) and classic API Key authentication.
/// </summary>
public class AzureVideoIndexerClient : IAzureVideoIndexerClient
{
    private const string ApiBaseUrl = "https://api.videoindexer.ai";

    private readonly HttpClient _httpClient;
    private readonly AutoClipperOptions _options;
    private readonly ILogger<AzureVideoIndexerClient> _logger;
    private readonly bool _useArmAuth;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AzureVideoIndexerClient(
        HttpClient httpClient,
        IOptions<AutoClipperOptions> options,
        ILogger<AzureVideoIndexerClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Determine authentication mode
        _useArmAuth = _options.UseArmAuthentication;

        // Validate required configuration based on auth mode
        if (string.IsNullOrWhiteSpace(_options.AzureVideoIndexerAccountId))
            throw new ArgumentException("AzureVideoIndexerAccountId is required");

        if (_useArmAuth)
        {
            // ARM auth validation
            if (string.IsNullOrWhiteSpace(_options.AzureVideoIndexerSubscriptionId))
                throw new ArgumentException("AzureVideoIndexerSubscriptionId is required for ARM authentication");
            if (string.IsNullOrWhiteSpace(_options.AzureVideoIndexerResourceGroup))
                throw new ArgumentException("AzureVideoIndexerResourceGroup is required for ARM authentication");
            if (string.IsNullOrWhiteSpace(_options.AzureVideoIndexerAccountName))
                throw new ArgumentException("AzureVideoIndexerAccountName is required for ARM authentication");

            _logger.LogInformation("Video Indexer client initialized with ARM Authentication (Service Principal)");
        }
        else
        {
            // API Key auth validation
            if (string.IsNullOrWhiteSpace(_options.AzureVideoIndexerApiKey))
                throw new ArgumentException("AzureVideoIndexerApiKey is required when ARM authentication is not configured");

            _logger.LogInformation("Video Indexer client initialized with classic API Key authentication");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimestampedTranscript>> TranscribeAsync(
        string filePath,
        VideoIndexerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Media file not found", filePath);

        _logger.LogInformation("Starting Video Indexer transcription for {File} (Auth: {AuthMode})",
            filePath, _useArmAuth ? "ARM" : "API Key");

        // Step 1: Get access token
        var accessToken = await GetAccessTokenAsync(cancellationToken);
        _logger.LogDebug("Obtained access token");

        // Step 2: Upload video
        var videoId = await UploadVideoAsync(filePath, accessToken, request, cancellationToken);
        _logger.LogInformation("Video uploaded with ID: {VideoId}", videoId);

        try
        {
            // Step 3: Wait for processing to complete
            var indexJson = await WaitForProcessingAsync(videoId, accessToken, cancellationToken);
            _logger.LogInformation("Video processing completed");

            // Step 4: Parse transcript
            var segments = ParseTranscript(indexJson, request.IncludeSpeakerLabels);
            _logger.LogInformation("Parsed {Count} transcript segments", segments.Count);

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
        if (_useArmAuth)
        {
            return await GetAccessTokenViaArmAsync(cancellationToken);
        }
        else
        {
            return await GetAccessTokenViaApiKeyAsync(cancellationToken);
        }
    }

    private async Task<string> GetAccessTokenViaArmAsync(CancellationToken cancellationToken)
    {
        // 1. Authenticate using Service Principal to get ARM token
        var credential = new ClientSecretCredential(
            _options.AzureVideoIndexerArmTenantId,
            _options.AzureVideoIndexerArmClientId,
            _options.AzureVideoIndexerArmClientSecret);

        var requestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
        var armAccessToken = await credential.GetTokenAsync(requestContext, cancellationToken);

        // 2. Call the Video Indexer "Generate Access Token" endpoint via Azure Management API
        var managementUrl = $"https://management.azure.com/subscriptions/{_options.AzureVideoIndexerSubscriptionId}/resourceGroups/{_options.AzureVideoIndexerResourceGroup}/providers/Microsoft.VideoIndexer/accounts/{_options.AzureVideoIndexerAccountName}/generateAccessToken?api-version=2024-01-01";

        using var request = new HttpRequestMessage(HttpMethod.Post, managementUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", armAccessToken.Token);

        var jsonContent = new
        {
            permissionType = "Contributor",
            scope = "Account"
        };

        request.Content = new StringContent(JsonSerializer.Serialize(jsonContent), System.Text.Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to generate VI Access Token via ARM. Status: {Status}, Body: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Failed to get access token: {response.StatusCode} - {body}");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("AccessToken not found in response");
    }

    private async Task<string> GetAccessTokenViaApiKeyAsync(CancellationToken cancellationToken)
    {
        var url = $"{ApiBaseUrl}/Auth/{_options.AzureVideoIndexerLocation}/Accounts/{_options.AzureVideoIndexerAccountId}/AccessToken?allowEdit=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _options.AzureVideoIndexerApiKey);

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

        var url = $"{ApiBaseUrl}/{_options.AzureVideoIndexerLocation}/Accounts/{_options.AzureVideoIndexerAccountId}/Videos?{string.Join("&", queryParams)}";

        _logger.LogDebug("Uploading video to: {Url}", url.Split('?')[0]);

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
        var url = $"{ApiBaseUrl}/{_options.AzureVideoIndexerLocation}/Accounts/{_options.AzureVideoIndexerAccountId}/Videos/{videoId}/Index?accessToken={Uri.EscapeDataString(accessToken)}";
        var timeout = TimeSpan.FromMinutes(_options.AzureVideoIndexerTimeoutMinutes);
        var pollInterval = TimeSpan.FromSeconds(_options.AzureVideoIndexerPollingIntervalSeconds);
        var startTime = DateTime.UtcNow;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.UtcNow - startTime > timeout)
                throw new TimeoutException($"Video Indexer processing did not complete within {_options.AzureVideoIndexerTimeoutMinutes} minutes");

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Failed to get video index: {response.StatusCode} - {body}");

            using var doc = JsonDocument.Parse(body);
            var state = doc.RootElement.GetProperty("state").GetString();

            var elapsed = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            _logger.LogInformation("Video Indexer status: {State} ({Elapsed}s elapsed)", state, elapsed);

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

    private IReadOnlyList<TimestampedTranscript> ParseTranscript(string indexJson, bool includeSpeakerLabels)
    {
        var segments = new List<TimestampedTranscript>();

        using var doc = JsonDocument.Parse(indexJson);

        // Navigate to: videos[0].insights.transcript
        if (!doc.RootElement.TryGetProperty("videos", out var videos) ||
            videos.GetArrayLength() == 0)
        {
            _logger.LogWarning("No videos found in index response");
            return segments;
        }

        var video = videos[0];
        if (!video.TryGetProperty("insights", out var insights))
        {
            _logger.LogWarning("No insights found in video");
            return segments;
        }

        // Build speaker name map from faces/speakers if available
        var speakerNames = BuildSpeakerNameMap(insights);

        // Parse transcript
        if (!insights.TryGetProperty("transcript", out var transcript) ||
            transcript.ValueKind != JsonValueKind.Array)
        {
            _logger.LogWarning("No transcript found in insights");
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

            segments.Add(new TimestampedTranscript(start, end, text.Trim(), speakerId, speakerName));
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
            var url = $"{ApiBaseUrl}/{_options.AzureVideoIndexerLocation}/Accounts/{_options.AzureVideoIndexerAccountId}/Videos/{videoId}?accessToken={Uri.EscapeDataString(accessToken)}";
            using var response = await _httpClient.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Deleted video {VideoId} from Video Indexer", videoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete video {VideoId} from Video Indexer", videoId);
        }
    }
}
