export interface IReportSectionSettingsModel {
  label: string;
  useAllContent: boolean;
  showHeadlines: boolean;
  showFullStory: boolean;
  showImage: boolean;
  convertToBase64Image?: boolean;
  cacheData?: boolean;
  direction: 'row' | 'column';
  removeDuplicates: boolean;
  overrideExcludeHistorical: boolean;
  inTableOfContents?: boolean;
  hideEmpty: boolean;
  groupBy: string;
  sortBy: string;
  sortDirection: '' | 'asc' | 'desc';
  datasetColors?: string[];
  dataLabelColors?: string[];
  url: string;
  urlCache?: string;
  preload?: boolean;
  dataType?: string;
  dataProperty?: string;
  dataTemplate?: string;
}
