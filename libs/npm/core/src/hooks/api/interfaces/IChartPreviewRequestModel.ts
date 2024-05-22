import { IChartSectionSettingsModel, IContentModel } from '.';

export interface IChartPreviewRequestModel {
  /** Chart configuration options. */
  settings: IChartSectionSettingsModel;
  /** Razor template to generate Chart.JS JSON data. */
  template: string;
  /** Elasticsearch index to query if a filter is provided. */
  index?: string;
  /** Elasticsearch filter query. */
  filter?: object;
  /** Foreign key to the filter */
  filterId?: number;
  /** Foreign key to linked report to pull content from */
  linkedReportId?: number;
  /** Foreign key to the folder */
  folderId?: number;
  /** An array of content to include in the chart data. */
  content?: IContentModel[];
  /** The compiled Chart.JS JSON data.  This enables manually editing the data instead of dynamically generating the data. */
  chartData?: object;
}
