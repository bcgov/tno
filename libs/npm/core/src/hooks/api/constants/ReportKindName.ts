/// <summary>
/// Provides report kind types.
/// </summary>
export enum ReportKindName {
  /// <summary>
  /// This report requires the user to generate them manually.
  /// </summary>
  Manual = 'manual',

  /// <summary>
  /// This report will be automatically generated on a schedule.
  /// </summary>
  Auto = 'auto',

  /// <summary>
  /// This report will be automatically generated on a schedule and sent to subscribers immediately.
  /// </summary>
  AutoSend = 'autoSend',
}
