/**
 * Report template settings provide a way to determine whether a configurable option is available to a report.
 */
export interface IReportTemplateSettingsModel {
  viewOnWebOnly: boolean;
  subject: {
    text: boolean;
    showTodaysDate: boolean;
  };
  headline: {
    showSource: boolean;
    showShortName: boolean;
    showPublishedOn: boolean;
    showSentiment: boolean;
  };
  content: {
    excludeHistorical: boolean;
    excludeReports: boolean;
    showLinkToStory: boolean;
    highlightKeywords: boolean;
  };
  sections: {
    enable: boolean;
    usePageBreaks: boolean;
  };
  enableCharts: boolean;
}
