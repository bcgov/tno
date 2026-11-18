namespace TNO.Services.AutoClipper.Azure;

public interface IAzureSpeechTranscriptionService
{
    Task<IReadOnlyList<TimestampedTranscript>> TranscribeAsync(string filePath, SpeechTranscriptionRequest request, CancellationToken cancellationToken);
}
