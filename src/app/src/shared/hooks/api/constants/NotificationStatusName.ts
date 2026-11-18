export enum NotificationStatusName {
  /// <summary>
  /// Notification is pending to be sent.
  /// </summary>
  Pending = 'Pending',

  /// <summary>
  /// Notification is accepted by CHES.
  /// </summary>
  Accepted = 'Accepted',

  /// <summary>
  /// Notification is sent.
  /// </summary>
  Completed = 'Completed',

  /// <summary>
  /// Notification was cancelled.
  /// </summary>
  Cancelled = 'Cancelled',

  /// <summary>
  /// Notification failed to send.
  /// </summary>
  Failed = 'Failed',
}
