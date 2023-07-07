import { IContentModel } from '.';

export interface IChartPreviewRequestModel {
  chartType?: string;
  width?: number;
  height?: number;
  template: string;
  filter?: object;
  content?: IContentModel[];
  chartData?: object;
}
