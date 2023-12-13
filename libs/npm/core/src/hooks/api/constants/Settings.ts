// The follow keys are hardcoded setting names.
// Use them to fetch values from the settings lookup.
export const Settings = {
  /** URL and path to the API */
  ApiPath: process.env.REACT_APP_API_URL ?? '/api',
  /** URL to the subscriber app */
  SubscriberUrl: 'SubscriberUrl',
  /** The default notification ID used for basic alerts. */
  DefaultAlert: 'AlertId',
  /** The notification ID used for top story alerts. */
  TopStoryAlert: 'TopStoryAlertId',
  /** The report ID used for the morning report. */
  MorningReport: 'MorningReportId',
  /** The report ID used for the front page images. */
  FrontPageImagesReport: 'FrontPageImagesReportId',
  /** The media type ID that identifies news radio. */
  NewsRadioMediaTypeFilter: 'NewsRadioMediaTypeFilterId',
  /** The filter ID used for the subscriber front page images page. */
  FrontpageFilter: 'FrontpageFilterId',
  /** The default license ID to use when adding custom content to reports. */
  DefaultSubscriberContentLicense: 'DefaultSubscriberContentLicenseId',
  /** The default media type ID to use when adding custom content to reports. */
  DefaultSubscriberContentMediaType: 'DefaultSubscriberContentMediaTypeId',
  /** A JSON object containing a dictionary of media type IDS for advanced search groupings. */
  AdvancedSearchMediaTypeGroups: 'AdvancedSearchMediaTypeGroups',
  /** The email address to send Product Subscription requests to. */
  ProductSubscriptionManagerEmail: 'ProductSubscriptionManagerEmail',
  /** The default report template ID to use when creating new reports. */
  DefaultReportTemplate: 'DefaultReportTemplateId',
  /** The folder ID used for event of the day content. */
  EventOfTheDayFolder: 'EventOfTheDayFolderId',
};
