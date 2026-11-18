namespace TNO.Services.AutoClipper.Azure;

public class SpeechTranscriptionRequest
{
    public string Language { get; init; } = "en-US";
    public bool EnableSpeakerDiarization { get; init; }
    public int? SpeakerCount { get; init; }
    public string? DiarizationMode { get; init; }
}
