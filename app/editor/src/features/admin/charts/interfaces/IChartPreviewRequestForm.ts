import { IChartPreviewRequestModel } from 'tno-core';

export interface IChartPreviewRequestForm extends IChartPreviewRequestModel {
  chartBase64?: string;
}
