import { IChartRequestForm } from '../interfaces';

export const defaultChartRequestForm: IChartRequestForm = {
  width: 500,
  height: 500,
  settings: {
    chartType: 'bar',
    groupBy: '',
    options: {},
  },
  template: '',
  content: [],
};
