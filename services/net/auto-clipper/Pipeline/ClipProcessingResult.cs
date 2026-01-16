using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.LLM;

namespace TNO.Services.AutoClipper.Pipeline;

public record ClipProcessingResult(
    string NormalizedAudioPath,
    string Language,
    IReadOnlyList<TimestampedTranscript> Segments,
    IReadOnlyList<ClipDefinition> ClipDefinitions,
    ClipSegmentationSettings SegmentationSettings);
