import { type IChartPreviewRequestModel } from '@/shared';

export interface IChartRequestForm extends IChartPreviewRequestModel {
  chartBase64?: string;
}
