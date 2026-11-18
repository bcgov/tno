namespace TNO.Entities;

/// <summary>
/// ResendOption enum, provides a way to determine when a notification or report should be resent.
/// </summary>
public enum ResendOption
{
    /// <summary>
    /// Never resend the same notification or report.
    /// </summary>
    Never = 0,
    /// <summary>
    /// Every time the content has been updated.
    /// </summary>
    Updated = 1,
    /// <summary>
    /// Only when the content has been unpublished and republished.
    /// </summary>
    Republished = 2,
    /// <summary>
    /// When a transcription has been approved.
    /// </summary>
    Transcribed = 3,
}
