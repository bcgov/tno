using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Azure;

/// <summary>
/// AzureSpeechTranscriptionService class, provides a way to send a file to Azure Speech service and get a transcription.
/// </summary>
public class AzureSpeechTranscriptionService : IAzureSpeechTranscriptionService
{
    private const string DefaultApiVersion = "v3.2";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AutoClipperOptions _options;
    private readonly ILogger<AzureSpeechTranscriptionService> _logger;
    private readonly HttpClient _httpClient;
    private readonly BlobContainerClient? _containerClient;

    public AzureSpeechTranscriptionService(HttpClient httpClient, IOptions<AutoClipperOptions> options, ILogger<AzureSpeechTranscriptionService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_options.AzureSpeechStorageConnectionString) && !string.IsNullOrWhiteSpace(_options.AzureSpeechStorageContainer))
        {
            _containerClient = new BlobContainerClient(_options.AzureSpeechStorageConnectionString, _options.AzureSpeechStorageContainer);
            Console.WriteLine($"Storage: {_containerClient?.AccountName}/{_containerClient?.Name}");
        }
    }

    public async Task<IReadOnlyList<TimestampedTranscript>> TranscribeAsync(string filePath, SpeechTranscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Language)) throw new ArgumentException("Speech recognition language must be provided.", nameof(request));
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) throw new FileNotFoundException("Audio file does not exist", filePath);
        if (string.IsNullOrWhiteSpace(_options.AzureSpeechKey)) throw new InvalidOperationException("Azure Speech key is required for batch transcription.");
        if (_containerClient == null) throw new InvalidOperationException("Azure Speech storage connection information must be configured for batch transcription.");

        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var blobClient = await UploadAudioAsync(filePath, cancellationToken).ConfigureAwait(false);
        try
        {
            var audioUrl = GenerateReadSasUri(blobClient);
            var transcriptionUri = await CreateTranscriptionAsync(audioUrl, Path.GetFileName(filePath), request, cancellationToken).ConfigureAwait(false);
            var finalState = await WaitForCompletionAsync(transcriptionUri, cancellationToken).ConfigureAwait(false);
            var transcriptUrl = await ResolveTranscriptFileAsync(finalState, cancellationToken).ConfigureAwait(false);
            var transcriptJson = await DownloadTranscriptAsync(transcriptUrl, cancellationToken).ConfigureAwait(false);
            var segments = ParseTranscript(transcriptJson);

            if (segments.Length == 0)
                _logger.LogWarning("Azure batch transcription produced no transcript entries for {File}", filePath);

            return segments;
        }
        finally
        {
            try
            {
                await blobClient.DeleteIfExistsAsync(cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to clean up temporary blob {Blob}", blobClient.Name);
            }
        }
    }

    private async Task<BlobClient> UploadAudioAsync(string filePath, CancellationToken cancellationToken)
    {
        var blobName = $"{Guid.NewGuid():N}-{Path.GetFileName(filePath)}";
        var blobClient = _containerClient!.GetBlobClient(blobName);
        await using var stream = File.OpenRead(filePath);
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Uploaded {File} to Azure blob {Blob}", filePath, blobName);
        return blobClient;
    }

    private Uri GenerateReadSasUri(BlobClient blobClient)
    {
        if (!blobClient.CanGenerateSasUri)
            throw new InvalidOperationException("Unable to generate SAS URI for Azure Speech batch transcription. Ensure the connection string includes an account key.");

        var minutes = _options.AzureSpeechStorageSasExpiryMinutes <= 0 ? 120 : _options.AzureSpeechStorageSasExpiryMinutes;
        var builder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(Math.Max(5, minutes))
        };
        builder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(builder);
    }

    private async Task<Uri> CreateTranscriptionAsync(Uri contentUri, string fileName, SpeechTranscriptionRequest request, CancellationToken cancellationToken)
    {
        var payload = BuildTranscriptionPayload(contentUri, fileName, request);
        var version = GetApiVersion();
        var uri = version.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? $"{GetSpeechEndpoint()}/speechtotext/{version}/transcriptions"
            : $"{GetSpeechEndpoint()}/speechtotext/transcriptions?api-version={version}";
        Console.WriteLine($"POST {uri}");
        Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri);
        ApplyHeaders(httpRequest);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Azure Speech batch transcription request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");

        Uri? location = response.Headers.Location;
        if (location == null)
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("self", out var node))
                Uri.TryCreate(node.GetString(), UriKind.Absolute, out location);
        }

        if (location == null)
            throw new InvalidOperationException("Azure Speech batch transcription response did not include an operation location.");

        _logger.LogInformation("Azure batch transcription created for {File}. Operation: {Operation}", fileName, location);
        return EnsureApiVersion(location);
    }

    private static object BuildTranscriptionPayload(Uri contentUri, string fileName, SpeechTranscriptionRequest request)
    {
        var properties = new Dictionary<string, object?>
        {
            ["wordLevelTimestampsEnabled"] = true,
            ["punctuationMode"] = "DictatedAndAutomatic",
            ["profanityFilterMode"] = "Masked"
        };

        if (request.EnableSpeakerDiarization)
        {
            properties["diarizationEnabled"] = true;
            if (request.SpeakerCount.HasValue && request.SpeakerCount > 0)
            {
                properties["diarization"] = new
                {
                    maxSpeakers = request.SpeakerCount.Value
                };
            }
        }

        return new
        {
            displayName = $"AutoClipper-{fileName}",
            description = "AutoClipper batch transcription",
            locale = request.Language,
            contentUrls = new[] { contentUri.ToString() },
            properties
        };
    }

    private async Task<BatchTranscription> WaitForCompletionAsync(Uri transcriptionUri, CancellationToken cancellationToken)
    {
        var pollInterval = TimeSpan.FromSeconds(Math.Max(5, _options.AzureSpeechBatchPollingIntervalSeconds <= 0 ? 10 : _options.AzureSpeechBatchPollingIntervalSeconds));
        var timeout = TimeSpan.FromMinutes(Math.Max(5, _options.AzureSpeechBatchTimeoutMinutes <= 0 ? 45 : _options.AzureSpeechBatchTimeoutMinutes));
        var expiry = DateTime.UtcNow + timeout;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var transcription = await GetTranscriptionAsync(transcriptionUri, cancellationToken).ConfigureAwait(false);
            if (transcription == null)
                throw new InvalidOperationException("Azure Speech returned an empty transcription status response.");

            var status = transcription.Status ?? string.Empty;
            if (status.Equals("succeeded", StringComparison.OrdinalIgnoreCase))
                return transcription;

            if (status.Equals("failed", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Azure batch transcription failed: {BuildErrorMessage(transcription.Error)}");

            if (DateTime.UtcNow >= expiry)
                throw new TimeoutException("Azure batch transcription did not complete before the configured timeout.");

            _logger.LogDebug("Azure batch transcription status for {Id}: {Status}", transcription.Uri ?? transcriptionUri, status);
            await Task.Delay(pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<BatchTranscription?> GetTranscriptionAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, EnsureApiVersion(uri));
        ApplyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Azure Speech status request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");

        var transcription = JsonSerializer.Deserialize<BatchTranscription>(body, SerializerOptions);
        return transcription;
    }

    private async Task<Uri> ResolveTranscriptFileAsync(BatchTranscription transcription, CancellationToken cancellationToken)
    {
        var filesLink = transcription.Links?.Files;
        if (filesLink == null)
            throw new InvalidOperationException("Azure Speech response did not include a files link.");

        using var request = new HttpRequestMessage(HttpMethod.Get, EnsureApiVersion(filesLink));
        ApplyHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Azure Speech file listing failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");

        var files = JsonSerializer.Deserialize<BatchTranscriptionFilesResponse>(body, SerializerOptions);
        var transcriptFile = files?.Values?.FirstOrDefault(f => string.Equals(f.Kind, "Transcription", StringComparison.OrdinalIgnoreCase) && f.Links?.ContentUrl != null);
        if (transcriptFile?.Links?.ContentUrl == null)
            throw new InvalidOperationException("Azure Speech file listing did not contain a transcription content URL.");

        return transcriptFile.Links.ContentUrl;
    }

    private async Task<string> DownloadTranscriptAsync(Uri transcriptUri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, transcriptUri);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private static TimestampedTranscript[] ParseTranscript(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<TimestampedTranscript>();

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("recognizedPhrases", out var phrases) || phrases.ValueKind != JsonValueKind.Array)
            return Array.Empty<TimestampedTranscript>();

        var results = new List<TimestampedTranscript>();
        foreach (var phrase in phrases.EnumerateArray())
        {
            var start = ReadTimeSpan(phrase, "offset");
            var duration = ReadTimeSpan(phrase, "duration");
            var end = duration > TimeSpan.Zero ? start + duration : start;
            var text = ReadPhraseText(phrase);
            if (string.IsNullOrWhiteSpace(text)) continue;
            if (end <= start) end = start + TimeSpan.FromMilliseconds(100);
            results.Add(new TimestampedTranscript(start, end, text.Trim()));
        }

        return results
            .OrderBy(r => r.Start)
            .ToArray();
    }

    private static string? ReadPhraseText(JsonElement phrase)
    {
        if (phrase.TryGetProperty("nBest", out var nBest) && nBest.ValueKind == JsonValueKind.Array)
        {
            foreach (var alt in nBest.EnumerateArray())
            {
                if (alt.ValueKind != JsonValueKind.Object) continue;
                if (alt.TryGetProperty("display", out var display) && display.ValueKind == JsonValueKind.String)
                    return display.GetString();
                if (alt.TryGetProperty("lexical", out var lexical) && lexical.ValueKind == JsonValueKind.String)
                    return lexical.GetString();
            }
        }

        if (phrase.TryGetProperty("displayText", out var displayText) && displayText.ValueKind == JsonValueKind.String)
            return displayText.GetString();
        if (phrase.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
            return text.GetString();

        return null;
    }

    private static TimeSpan ReadTimeSpan(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var node)) return TimeSpan.Zero;
        if (node.ValueKind == JsonValueKind.Number)
        {
            if (node.TryGetInt64(out var ticks)) return TimeSpan.FromTicks(ticks);
            if (node.TryGetDouble(out var seconds)) return TimeSpan.FromSeconds(seconds);
        }
        else if (node.ValueKind == JsonValueKind.String)
        {
            var value = node.GetString();
            if (string.IsNullOrWhiteSpace(value)) return TimeSpan.Zero;
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks)) return TimeSpan.FromTicks(ticks);
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)) return TimeSpan.FromSeconds(seconds);
            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var ts)) return ts;
            if (TryParseIsoDuration(value, out var isoTs)) return isoTs;
        }

        return TimeSpan.Zero;
    }

    private static bool TryParseIsoDuration(string value, out TimeSpan result)
    {
        try
        {
            result = XmlConvert.ToTimeSpan(value);
            return true;
        }
        catch (Exception)
        {
            result = TimeSpan.Zero;
            return false;
        }
    }

    private string BuildErrorMessage(BatchTranscriptionError? error)
    {
        if (error == null) return "Unknown error";
        var builder = new StringBuilder();
        AppendError(builder, error);
        return builder.ToString();
    }

    private void AppendError(StringBuilder builder, BatchTranscriptionError? error)
    {
        if (error == null) return;
        if (builder.Length > 0) builder.Append(" -> ");
        builder.Append(error.Code);
        if (!string.IsNullOrWhiteSpace(error.Message))
        {
            builder.Append(':').Append(' ').Append(error.Message);
        }
        if (error.InnerError != null) AppendError(builder, error.InnerError);
    }

    private void ApplyHeaders(HttpRequestMessage request)
    {
        request.Headers.Remove("Ocp-Apim-Subscription-Key");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _options.AzureSpeechKey);
    }

    private Uri EnsureApiVersion(Uri uri)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        if (!string.IsNullOrWhiteSpace(uri.Query) && uri.Query.Contains("api-version", StringComparison.OrdinalIgnoreCase))
            return uri;

        var builder = new UriBuilder(uri);
        var query = builder.Query.TrimStart('?');
        var version = $"api-version={GetApiVersion()}";
        builder.Query = string.IsNullOrWhiteSpace(query) ? version : $"{query}&{version}";
        return builder.Uri;
    }

    private string GetApiVersion() => string.IsNullOrWhiteSpace(_options.AzureSpeechBatchApiVersion) ? DefaultApiVersion : _options.AzureSpeechBatchApiVersion;

    private string GetSpeechEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(_options.AzureSpeechBatchEndpoint))
            return _options.AzureSpeechBatchEndpoint.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(_options.AzureSpeechRegion))
            return $"https://{_options.AzureSpeechRegion}.api.cognitive.microsoft.com";
        throw new InvalidOperationException("Azure Speech region or endpoint must be configured.");
    }

    private sealed record BatchTranscription(
        [property: JsonPropertyName("self")] Uri? Uri,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("links")] BatchTranscriptionLinks? Links,
        [property: JsonPropertyName("error")] BatchTranscriptionError? Error);

    private sealed record BatchTranscriptionLinks(
        [property: JsonPropertyName("files")] Uri? Files,
        [property: JsonPropertyName("content")] Uri? Content);

    private sealed record BatchTranscriptionError(
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("message")] string? Message,
        [property: JsonPropertyName("innerError")] BatchTranscriptionError? InnerError);

    private sealed record BatchTranscriptionFilesResponse(
        [property: JsonPropertyName("values")] IReadOnlyList<BatchTranscriptionFile>? Values);

    private sealed record BatchTranscriptionFile(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("links")] BatchTranscriptionFileLinks? Links);

    private sealed record BatchTranscriptionFileLinks(
        [property: JsonPropertyName("contentUrl")] Uri? ContentUrl);
}









