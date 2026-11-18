export enum UserAccountTypeName {
  /**
   * Direct users can login to the app.
   */
  Direct = 'Direct',
  /**
   * Indirect users cannot login to the app, but can subscribe to notifications and reports.
   */
  Indirect = 'Indirect',
  /**
   * Provides a way to manage a distribution list of accounts subscribed to notifications and reports.
   */
  Distribution = 'Distribution',
  /**
   * System accounts cannot login to the app and are used by services.
   */
  SystemAccount = 'SystemAccount',
}
