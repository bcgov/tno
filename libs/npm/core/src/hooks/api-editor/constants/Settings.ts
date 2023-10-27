export const Settings = {
  ApiPath: process.env.REACT_APP_API_URL ?? '/api',

  // The follow keys are hardcoded setting names.
  // Use them to fetch values from the settings lookup.
  SubscriberUrl: 'SubscriberUrl',
  DefaultAlert: 'AlertId',
  TopStoryAlert: 'TopStoryAlertId',
  MorningReport: 'MorningReportId',
  FrontPageImagesReport: 'FrontPageImagesReportId',
  NewsRadioProductFilter: 'NewsRadioProductFilterId',
  FrontpageFilter: 'FrontpageFilterId',
};
