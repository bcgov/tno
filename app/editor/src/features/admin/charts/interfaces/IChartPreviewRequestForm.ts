import { IChartRequestModel } from 'tno-core';

export interface IChartRequestForm extends IChartRequestModel {
  chartBase64?: string;
}
