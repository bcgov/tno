export interface IReportSectionSettingsModel {
  label: string;
  useAllContent: boolean;
  showHeadlines: boolean;
  showFullStory: boolean;
  showImage: boolean;
  direction: 'row' | 'column';
  removeDuplicates: boolean;
  overrideExcludeHistorical: boolean;
  hideEmpty: boolean;
  groupBy: string;
  sortBy: string;
  sortDirection: '' | 'asc' | 'desc';
  datasetColors?: string[];
  dataLabelColors?: string[];
  url: string;
}
