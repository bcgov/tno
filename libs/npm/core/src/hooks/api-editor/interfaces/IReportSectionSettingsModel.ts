export interface IReportSectionSettingsModel {
  label: string;
  isSummary: boolean;
  showContent: boolean;
  showCharts: boolean;
  chartsOnTop: boolean;
  chartDirection: 'row' | 'column';
  removeDuplicates: boolean;
  sortBy: string;
}
