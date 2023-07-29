import { IChartPreviewRequestForm } from '../interfaces';

export const defaultChartPreviewRequestForm: IChartPreviewRequestForm = {
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
