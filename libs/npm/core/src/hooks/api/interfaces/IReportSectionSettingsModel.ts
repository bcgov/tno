import { ReportSectionTypeName } from '..';

export interface IReportSectionSettingsModel {
  label: string;
  sectionType: ReportSectionTypeName;
  useAllContent: boolean;
  showHeadlines: boolean;
  showFullStory: boolean;
  showImage: boolean;
  direction: 'row' | 'column';
  removeDuplicates: boolean;
  hideEmpty: boolean;
  groupBy: string;
  sortBy: string;
}
