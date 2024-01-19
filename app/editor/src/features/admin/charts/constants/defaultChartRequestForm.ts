import { IChartRequestForm } from '../interfaces';
import { defaultChartJSOptions } from '.';

export const defaultChartRequestForm: IChartRequestForm = {
  settings: {
    width: 500,
    height: 500,
    chartType: 'bar',
    groupBy: 'mediaType',
    options: defaultChartJSOptions,
  },
  template: '',
  content: [],
};
