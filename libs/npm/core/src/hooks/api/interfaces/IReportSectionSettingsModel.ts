import { ReportSectionTypeName } from '..';

export interface IReportSectionSettingsModel {
  label: string;
  sectionType: ReportSectionTypeName;
  showHeadlines: boolean;
  showFullStory: boolean;
  showImage: boolean;
  showCharts: boolean;
  chartsOnTop: boolean;
  chartDirection: 'row' | 'column';
  removeDuplicates: boolean;
  hideEmpty: boolean;
  groupBy: string;
  sortBy: string;
}
