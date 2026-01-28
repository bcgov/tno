namespace AutoClipperHarness;

/// <summary>
/// Request options for Azure Video Indexer transcription.
/// </summary>
public class VideoIndexerRequest
{
    /// <summary>
    /// Language code for transcription (e.g., "en-US", "zh-CN").
    /// </summary>
    public string Language { get; init; } = "en-US";

    /// <summary>
    /// Optional Person Model ID for speaker identification.
    /// When provided, Video Indexer will attempt to identify known faces.
    /// </summary>
    public string? PersonModelId { get; init; }

    /// <summary>
    /// Whether to include speaker labels in the output.
    /// </summary>
    public bool IncludeSpeakerLabels { get; init; } = true;
}
