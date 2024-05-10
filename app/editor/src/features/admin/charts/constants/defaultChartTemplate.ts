import { IChartTemplateModel } from 'tno-core';

import { defaultChartJSOptions } from '.';

export const defaultChartTemplate: IChartTemplateModel = {
  id: 0,
  name: '',
  description: '',
  template: '',
  isEnabled: true,
  sortOrder: 0,
  isPublic: false,
  settings: {
    chartTypes: [],
    groupBy: [],
    dataset: [],
    datasetValue: [],
    options: defaultChartJSOptions,
  },
};
