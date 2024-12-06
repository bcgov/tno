import { IChartRequestForm } from '../interfaces';
import { defaultChartJSOptions } from '.';

export const defaultChartRequestForm: IChartRequestForm = {
  settings: {
    width: 500,
    chartType: 'bar',
    groupBy: 'otherSource',
    groupByOrder: 'asc',
    dataset: '',
    datasetOrder: 'asc',
    datasetValue: 'count',
    excludeEmptyValues: false,
    options: defaultChartJSOptions,
  },
  template: '',
  content: [],
};
