using TNO.Services.AutoClipper.Azure;

namespace TNO.Services.AutoClipper.LLM;

public interface IClipSegmentationService
{
    Task<IReadOnlyList<ClipDefinition>> GenerateClipsAsync(IReadOnlyList<TimestampedTranscript> transcript, ClipSegmentationSettings? settings, CancellationToken cancellationToken);
}
