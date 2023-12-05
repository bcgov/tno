/// <summary>
/// Provides report distribution format options.
/// </summary>
export enum ReportDistributionFormatName {
  /// <summary>
  /// Receive full report in email.
  /// </summary>
  FullText = 'FullText',
  /// <summary>
  /// Receive an email that only contains a link to the report on the website.
  /// </summary>
  LinkOnly = 'LinkOnly',
  /// <summary>
  /// Receive both emails, one which contains the report, and one that only contains a link to the website.
  /// </summary>
  ReceiveBoth = 'ReceiveBoth',
}
