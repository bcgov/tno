/// <summary>
/// Provides report distribution format options.
/// </summary>
export enum ReportDistributionFormat {
  /// <summary>
  /// Receive full report in email.
  /// </summary>
  FullText = 0,
  /// <summary>
  /// Receive an email that only contains a link to the report on the website.
  /// </summary>
  LinkOnly = 1,
  /// <summary>
  /// Receive both emails, one which contains the report, and one that only contains a link to the website.
  /// </summary>
  ReceiveBoth = 2,
}
