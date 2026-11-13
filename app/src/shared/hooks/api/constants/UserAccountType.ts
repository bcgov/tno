export enum UserAccountType {
  /**
   * Direct users can login to the app.
   */
  Direct = 0,
  /**
   * Indirect users cannot login to the app, but can subscribe to notifications and reports.
   */
  Indirect = 1,
  /**
   * Provides a way to manage a distribution list of accounts subscribed to notifications and reports.
   */
  Distribution = 2,
  /**
   * System accounts cannot login to the app and are used by services.
   */
  SystemAccount = 3,
}
