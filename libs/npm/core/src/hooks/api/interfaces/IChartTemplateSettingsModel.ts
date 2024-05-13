export interface IChartTemplateSettingsModel {
  /** An array of chart types this template supports. */
  chartTypes: string[];
  /** An array of properties to group by that this template supports. */
  groupBy: string[];
  /** An array of dataset options this chart supports. */
  dataset: string[];
  /** An array of dataset value options this chart supports. */
  datasetValue: string[];
  /** Default Chart.JS configuration options. */
  options: any;
}
