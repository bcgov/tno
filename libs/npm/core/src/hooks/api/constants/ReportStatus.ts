export enum ReportStatus {
  /// <summary>
  /// Report is pending to be sent.
  /// </summary>
  Pending = 0,

  /// <summary>
  /// Report is accepted by CHES.
  /// </summary>
  Accepted = 1,

  /// <summary>
  /// Report is sent.
  /// </summary>
  Completed = 2,

  /// <summary>
  /// Report was cancelled.
  /// </summary>
  Cancelled = 3,

  /// <summary>
  /// Report failed to send.
  /// </summary>
  Failed = 4,
}
