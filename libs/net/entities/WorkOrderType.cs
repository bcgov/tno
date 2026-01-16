namespace TNO.Entities;

/// <summary>
/// Provides work types which identifies what the work order request is for.
/// </summary>
public enum WorkOrderType
{
    /// <summary>
    /// A request for content to te sent for transcription.
    /// </summary>
    Transcription = 0,

    /// <summary>
    /// A request for content to be sent for natural language processing.
    /// </summary>
    NaturalLanguageProcess = 1,

    /// <summary>
    /// A request for a remote file.
    /// </summary>
    FileRequest = 2,

    /// <summary>
    /// A request to process file with FFmpeg.
    /// </summary>
    FFmpeg = 3,

    /// <summary>
    /// A request to generate clips and transcripts via the auto clipper pipeline.
    /// </summary>
    AutoClip = 4,
}
