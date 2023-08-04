import { IChartPreviewRequestModel } from 'tno-core';

export interface IChartRequestForm extends IChartPreviewRequestModel {
  chartBase64?: string;
}
