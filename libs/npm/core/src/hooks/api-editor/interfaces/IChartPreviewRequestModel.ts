import { IChartSettingsModel, IContentModel } from '.';

export interface IChartPreviewRequestModel {
  width?: number;
  height?: number;
  settings: IChartSettingsModel;
  template: string;
  index?: string;
  filter?: object;
  content?: IContentModel[];
  chartData?: object;
}
