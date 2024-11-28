export interface IReportContentSettingsModel {
  clearFolders: boolean;
  onlyNewContent: boolean;
  excludeHistorical: boolean;
  excludeReports: number[];
  showLinkToStory: boolean;
  highlightKeywords: boolean;
  copyPriorInstance: boolean;
  clearOnStartNewReport: boolean;
  excludeContentInUnsentReport: boolean;
  omitBCUpdates: boolean;
}
