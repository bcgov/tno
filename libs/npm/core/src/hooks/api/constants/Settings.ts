// The follow keys are hardcoded setting names.
// Use them to fetch values from the settings lookup.
export const Settings = {
  /** URL and path to the API */
  ApiPath: process.env.REACT_APP_API_URL ?? '/api',
  /** URL to the subscriber app */
  SubscriberUrl: 'SubscriberUrl',
  /** URL to the editor app */
  EditorUrl: 'EditorUrl',
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
  /** The email address to send Product Subscription requests to. */
  ProductSubscriptionManagerEmail: 'ProductSubscriptionManagerEmail',
  /** The default report template ID to use when creating new reports. */
  DefaultReportTemplate: 'DefaultReportTemplateId',
  /** The folder ID used for event of the day content. */
  EventOfTheDayFolder: 'EventOfTheDayFolderId',
  /** The report ID used for the Event of the Day. */
  EventOfTheDayReport: 'EventOfTheDayReportId',
  /** The front page image media type ID */
  FrontPageImageMediaType: 'FrontPageImageMediaTypeId',
  /** Media types that displays a new window on search results */
  SearchPageResultsNewWindow: 'SearchPageResultsNewWindow',
  /** Media Types not considering source_type_search_mapping table */
  MediaTypesAllSources: 'MediaTypesAllSources',
  /** The action id for featured content */
  FeaturedAction: 'FeaturedActionId',
  /** The action id for top story content */
  TopStoryAction: 'TopStoryActionId',
  /** The action id for commentary content */
  CommentaryAction: 'CommentaryActionId',
  /** The action id for alerting content */
  AlertAction: 'AlertActionId',
  /** The id's not to include byline in the content list view*/
  ExcludeBylineIds: 'ExcludeBylineIds',
  /** The id's not to include sources in the content list view*/
  ExcludeSourceIds: 'ExcludeSourceIds',
  /** The report ID used for the AM Analysis (Event of the Day) report. */
  EventOfTheDayReportId: 'EventOfTheDayReportId',
  /** The default template ID to Basic Alerts. */
  BasicAlertTemplateId: 'BasicAlertTemplateId',
};
