export interface IChartTemplateSettingsModel {
  /** An array of chart types this template supports. */
  chartTypes: string[];
  /** An array of properties to group by that this template supports. */
  groupBy: string[];
  /** Default Chart.JS configuration options. */
  options: any;
}
