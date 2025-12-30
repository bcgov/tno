using System.Globalization;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.AutoClipper.Config;

namespace TNO.Services.AutoClipper.Azure;

public class AzureSpeechTranscriptionService : IAzureSpeechTranscriptionService
{
    private readonly AutoClipperOptions _options;
    private readonly ILogger<AzureSpeechTranscriptionService> _logger;

    public AzureSpeechTranscriptionService(IOptions<AutoClipperOptions> options, ILogger<AzureSpeechTranscriptionService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TimestampedTranscript>> TranscribeAsync(string filePath, SpeechTranscriptionRequest request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Audio file does not exist", filePath);
        if (string.IsNullOrWhiteSpace(_options.AzureSpeechKey) || string.IsNullOrWhiteSpace(_options.AzureSpeechRegion))
            throw new InvalidOperationException("Azure Speech configuration is missing.");

        var attempts = Math.Max(1, _options.AzureSpeechMaxRetries);
        var retryDelay = TimeSpan.FromSeconds(Math.Max(1, _options.AzureSpeechRetryDelaySeconds));
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (extension == ".wav")
                {
                    using var audioConfig = AudioConfig.FromWavFileInput(filePath);
                    return await RecognizeAsync(audioConfig, request, null, null, cts.Token).ConfigureAwait(false);
                }

                var format = AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.MP3);
                using var pushStream = AudioInputStream.CreatePushStream(format);
                using var streamAudioConfig = AudioConfig.FromStreamInput(pushStream);
                await using var stream = File.OpenRead(filePath);
                return await RecognizeAsync(streamAudioConfig, request, stream, pushStream, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastError = ex;
                if (attempt >= attempts) throw;
                _logger.LogWarning(ex, "Azure Speech transcription attempt {Attempt}/{Attempts} failed for {File}. Retrying in {Delay}...", attempt, attempts, filePath, retryDelay);
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        throw lastError ?? new InvalidOperationException("Azure Speech transcription failed unexpectedly.");
    }

    private SpeechConfig CreateSpeechConfig(SpeechTranscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Language))
            throw new ArgumentException("Speech recognition language must be provided.", nameof(request));

        var speechConfig = SpeechConfig.FromSubscription(_options.AzureSpeechKey, _options.AzureSpeechRegion);
        speechConfig.SpeechRecognitionLanguage = request.Language;
        speechConfig.OutputFormat = OutputFormat.Detailed;
        speechConfig.RequestWordLevelTimestamps();
        speechConfig.SetProfanity(ProfanityOption.Raw);

        if (request.EnableSpeakerDiarization)
        {
            speechConfig.SetServiceProperty("diarizationEnabled", "true", ServicePropertyChannel.UriQueryParameter);
            var diarizationMode = string.IsNullOrWhiteSpace(request.DiarizationMode) ? "online" : request.DiarizationMode;
            speechConfig.SetServiceProperty("diarizationMode", diarizationMode, ServicePropertyChannel.UriQueryParameter);
            if (request.SpeakerCount.HasValue && request.SpeakerCount > 0)
            {
                speechConfig.SetServiceProperty("diarizationSpeakerCount", request.SpeakerCount.Value.ToString(CultureInfo.InvariantCulture), ServicePropertyChannel.UriQueryParameter);
            }
        }

        return speechConfig;
    }

    private async Task<TimestampedTranscript[]> RecognizeAsync(AudioConfig audioConfig, SpeechTranscriptionRequest request, Stream? fileStream, PushAudioInputStream? pushStream, CancellationToken cancellationToken)
    {
        var transcripts = new List<TimestampedTranscript>();
        var speechConfig = CreateSpeechConfig(request);

        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Exception? recognitionError = null;
        var sessionStarted = false;
        var sessionStopped = false;

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrWhiteSpace(e.Result.Text))
            {
                var start = TimeSpan.FromTicks(e.Result.OffsetInTicks);
                var end = start + e.Result.Duration;
                transcripts.Add(new TimestampedTranscript(start, end, e.Result.Text));
                _logger.LogDebug("Speech transcription process. \"{text}...\"", e.Result.Text?[0..Math.Min(e.Result.Text.Length, 25)]);
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            if (e.Reason == CancellationReason.Error)
            {
                var message = string.IsNullOrWhiteSpace(e.ErrorDetails)
                    ? "Azure Speech cancellation reported an unknown error."
                    : e.ErrorDetails;
                recognitionError = new InvalidOperationException($"Azure Speech canceled recognition: {message}");
                _logger.LogWarning("Azure Speech canceled recognition: {Details}", message);
            }
            completion.TrySetResult(true);
        };

        recognizer.SessionStarted += (s, e) =>
        {
            sessionStarted = true;
            _logger.LogDebug("Azure Speech session started. SessionId: {SessionId}", e.SessionId);
        };

        recognizer.SessionStopped += (s, e) =>
        {
            sessionStopped = true;
            completion.TrySetResult(true);
            _logger.LogDebug("Azure Speech session stopped. SessionId: {SessionId}", e.SessionId);
        };

        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        if (fileStream != null && pushStream != null)
        {
            await WriteStreamAsync(pushStream, fileStream, cancellationToken).ConfigureAwait(false);
            pushStream.Close();
        }
        await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

        if (recognitionError != null) throw recognitionError;
        if (sessionStarted && !sessionStopped)
            throw new InvalidOperationException("Azure Speech session ended unexpectedly without a stop notification.");

        return [.. transcripts];
    }

    private static async Task WriteStreamAsync(PushAudioInputStream pushStream, Stream fileStream, CancellationToken cancellationToken)
    {
        var buffer = new byte[32 * 1024];
        int read;
        while ((read = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            var chunk = new byte[read];
            buffer.AsSpan(0, read).CopyTo(chunk);
            pushStream.Write(chunk);
        }
    }
}
