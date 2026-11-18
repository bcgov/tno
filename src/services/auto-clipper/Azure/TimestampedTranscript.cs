using System;

namespace TNO.Services.AutoClipper.Azure;

/// <summary>
/// Represents a transcript segment with optional speaker identification.
/// </summary>
public record TimestampedTranscript(
    TimeSpan Start,
    TimeSpan End,
    string Text,
    int? SpeakerId = null,
    string? SpeakerName = null
);
