namespace TNO.Services.AutoClipper.Azure;

/// <summary>
/// Client interface for Azure Video Indexer transcription service.
/// </summary>
public interface IAzureVideoIndexerClient
{
    /// <summary>
    /// Transcribes a media file using Azure Video Indexer.
    /// </summary>
    /// <param name="filePath">Path to the media file (video or audio).</param>
    /// <param name="request">Transcription request options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of transcript segments with optional speaker information.</returns>
    Task<IReadOnlyList<TimestampedTranscript>> TranscribeAsync(
        string filePath,
        VideoIndexerRequest request,
        CancellationToken cancellationToken = default);
}
