export enum ResendOption {
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
