export enum WorkOrderStatusName {
  /// <summary>
  /// A request for work has been submitted.
  /// </summary>
  Submitted = 'Submitted',

  /// <summary>
  /// Work is currently being processed.
  /// </summary>
  InProgress = 'InProgress',

  /// <summary>
  /// Work is completed
  /// </summary>
  Completed = 'Completed',

  /// <summary>
  /// Work has been cancelled.
  /// </summary>
  Cancelled = 'Cancelled',

  /// <summary>
  /// Work has failed and is not complete.
  /// </summary>
  Failed = 'Failed',
}
