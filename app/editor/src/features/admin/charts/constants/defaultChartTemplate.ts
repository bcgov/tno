import { IChartTemplateModel } from 'tno-core';

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
    isOverTime: false,
    graph: [],
    groupBy: [],
    groupBySection: [],
    options: {},
  },
};
