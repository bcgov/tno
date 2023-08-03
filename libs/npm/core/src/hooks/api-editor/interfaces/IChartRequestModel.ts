import { IChartSettingsModel, IContentModel } from '.';

export interface IChartRequestModel {
  /** Width of the chart image. */
  width?: number;
  /** Height of the chart image. */
  height?: number;
  /** Chart configuration options. */
  settings: IChartSettingsModel;
  /** Razor template to generate Chart.JS JSON data. */
  template: string;
  /** Elasticsearch index to query if a filter is provided. */
  index?: string;
  /** Elasticsearch filter query. */
  filter?: object;
  /** An array of content to include in the chart data. */
  content?: IContentModel[];
  /** The compiled Chart.JS JSON data.  This enables manually editing the data instead of dynamically generating the data. */
  chartData?: object;
}
