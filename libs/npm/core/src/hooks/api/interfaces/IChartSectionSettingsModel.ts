export interface IChartSectionSettingsModel {
  /** Width of the chart image. */
  width?: number;
  /** Height of the chart image. */
  height?: number;
  /** The chart type to generate [bar|line|pie|doughnut|scatter|bubble|radar|polararea] */
  chartType: string;
  /** The property to group results in [otherSource|series|mediaType|contentType|sentiment] */
  groupBy: string;

  /** Whether the chart is horizontal or vertical (flips x and y axis) */
  isHorizontal?: boolean;
  /** Whether to show the legend on the chart */
  showLegend?: boolean;
  /** Whether to show the legend title on the chart */
  showLegendTitle?: boolean;
  /** Override the chart title. */
  title?: string;
  /** Whether to show the data value labels in the chart */
  showDataLabels?: boolean;
  /** Whether to show the axis information on the chart */
  showAxis?: boolean;

  /** Chart.JS configuration options */
  options: any;
}
