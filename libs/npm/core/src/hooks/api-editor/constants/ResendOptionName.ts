export enum ResendOptionName {
  /// <summary>
  /// Never resend the same notification or report.
  /// </summary>
  Never = 'Never',
  /// <summary>
  /// Every time the content has been updated.
  /// </summary>
  Updated = 'Updated',
  /// <summary>
  /// Only when the content has been unpublished and republished.
  /// </summary>
  Republished = 'Republished',
  /// <summary>
  /// When a transcription has been approved.
  /// </summary>
  Transcribed = 'Transcribed',
}
