export interface IChartSectionSettingsModel {
  /** Width of the chart image. */
  width?: number;
  /** Height of the chart image. */
  height?: number;
  /** The chart type to generate [bar|line|pie|doughnut|scatter|bubble|radar|polararea] */
  chartType: string;
  /** The property to group results in [otherSource|series|mediaType|contentType|sentiment] */
  groupBy: string;
  /** Override the chart title. */
  title: string;
  /** Whether the chart is displayed horizontally. */
  isHorizontal: boolean;
  /** Whether the data value labels are visible. */
  showDataValues: boolean;
  /** Chart.JS configuration options */
  options: any;
}
