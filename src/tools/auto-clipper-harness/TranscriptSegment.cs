namespace AutoClipperHarness;

/// <summary>
/// Represents a transcript segment with optional speaker identification.
/// This is the harness-local version; will be merged into TimestampedTranscript in phase 2.
/// </summary>
public record TranscriptSegment(
    TimeSpan Start,
    TimeSpan End,
    string Text,
    int? SpeakerId = null,
    string? SpeakerName = null
);
