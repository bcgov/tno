export interface IChartSectionSettingsModel {
  /** Width of the chart image. */
  width?: number;
  /** Height of the chart image. */
  height?: number;
  /** Automatically resize chart based on data. */
  autoResize?: boolean;
  /** Whether width and height should force aspect ratio */
  maintainAspectRatio?: boolean;
  /** The chart aspect ratio */
  aspectRatio?: number;
  /** The chart type to generate [bar|line|pie|doughnut|scatter|bubble|radar|polararea] */
  chartType: string;
  /** The property to group results in [otherSource|series|mediaType|contentType|sentiment|sentimentSimple|byline|section] */
  groupBy: string;
  /** The order of the group by. */
  groupByOrder: 'asc' | 'desc';
  /** The property to separate datasets [otherSource|series|mediaType|contentType|byline|section] */
  dataset: string;
  /** The order of the dataset. */
  datasetOrder: 'asc' | 'desc';
  /** The property to extract a value for the dataset [otherSource|series|mediaType|contentType|sentiment|sentimentSimple|byline|section] */
  datasetValue: string;
  /** Whether to exclude empty values in the returned dataset. */
  excludeEmptyValues: boolean;
  /** Whether to apply the dataset colour to each value instead of a dataset */
  applyColorToValue?: boolean;
  /** Colours to use in the chart data sets. */
  datasetColors?: string[];
  /** Colours to use in the chart data sets borders. */
  datasetBorderColors?: string[];

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
  /** Whether to show the axis information on the chart */
  xShowAxisLabels?: boolean;
  /** The X axis legend title */
  xLegend?: string;
  xLegendFontSize?: number;
  /** Whether to skip x axis values to save space */
  xAutoSkip?: boolean;
  /** Rotate the x axis labels */
  xMinRotation?: number;
  xMaxRotation?: number;
  /** Whether to show the axis information on the chart */
  yShowAxisLabels?: boolean;
  /** The Y axis legend title */
  yLegend?: string;
  yLegendFontSize?: number;
  /** Whether to skip x axis values to save space */
  yAutoSkip?: boolean;
  /** Rotate the y axis labels */
  yMinRotation?: number;
  yMaxRotation?: number;
  /** Whether to show the data value labels in the chart */
  showDataLabels?: boolean;
  dataLabelFontSize?: number;
  /** Colours to use in the chart data labels. */
  dataLabelColors?: string[];
  dataLabelBackgroundColors?: string[];
  dataLabelAnchors?: ('center' | 'start' | 'end')[];
  dataLabelAligns?: ('center' | 'start' | 'end' | 'right' | 'bottom' | 'left' | 'top')[];
  dataLabelOffsets?: number[];
  dataLabelClip?: boolean;
  dataLabelClamp?: boolean;
  /** Whether to stack datasets (only works on some charts). */
  stacked?: boolean;
  /** Scale suggested minimum value. */
  scaleSuggestedMin?: number;
  /** Scale suggested maximum value. */
  scaleSuggestedMax?: number;
  /** Scale add to max dataset size */
  scaleCalcMax?: number;
  /** Scale tick step size */
  scaleTicksStepSize?: number;
  /** Minimum width in pixels a bar must be */
  minBarLength?: number;

  /** Chart.JS configuration options */
  options: any;
}
