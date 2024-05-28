export interface IChartSectionSettingsModel {
  /** Width of the chart image. */
  width?: number;
  /** Height of the chart image. */
  height?: number;
  /** The chart type to generate [bar|line|pie|doughnut|scatter|bubble|radar|polararea] */
  chartType: string;
  /** The property to group results in [otherSource|series|mediaType|contentType|sentiment|sentimentSimple|byline|section] */
  groupBy: string;
  /** The property to separate datasets [otherSource|series|mediaType|contentType|byline|section] */
  dataset: string;
  /** The property to extract a value for the dataset [otherSource|series|mediaType|contentType|sentiment|sentimentSimple|byline|section] */
  datasetValue: string;
  /** Colours to use in the chart data sets. */
  datasetColors?: string[];
  /** Colours to use in the chart data labels. */
  dataLabelColors?: string[];

  /** Whether the chart is horizontal or vertical (flips x and y axis) */
  isHorizontal?: boolean;
  title?: string;
  titleFontSize?: number;
  subtitle?: string;
  subtitleFontSize?: number;
  /** Whether to display the legend */
  showLegend?: boolean;
  /** The chart legend title. */
  legendTitle?: string;
  legendTitleFontSize?: number;
  /** Legend position */
  legendPosition?: 'top' | 'left' | 'bottom' | 'right' | 'chartArea';
  legendAlign?: 'start' | 'center' | 'end';
  legendLabelFontSize?: number;
  /** Size of the dataset colour box legend */
  legendLabelBoxWidth?: number;
  /** The X axis legend title */
  xLegend?: string;
  xLegendFontSize?: number;
  /** The Y axis legend title */
  yLegend?: string;
  yLegendFontSize?: number;
  /** Whether to show the data value labels in the chart */
  showDataLabels?: boolean;
  dataLabelFontSize?: number;
  /** Whether to show the axis information on the chart */
  showAxis?: boolean;
  /** Whether to stack datasets (only works on some charts). */
  stacked?: boolean;
  /** Scale suggested minimum value. */
  scaleSuggestedMin?: number;
  /** Scale suggested maximum value. */
  scaleSuggestedMax?: number;
  /** Scale tick step size */
  scaleTicksStepSize?: number;
  /** Minimum width in pixels a bar must be */
  minBarLength?: number;

  /** Chart.JS configuration options */
  options: any;
}
