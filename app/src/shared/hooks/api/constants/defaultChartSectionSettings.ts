import { IChartSectionSettingsModel } from '..';

export const defaultChartSectionSettings: IChartSectionSettingsModel = {
  chartType: 'bar',
  groupBy: 'otherSource',
  groupByOrder: 'asc',
  dataset: '',
  datasetOrder: 'asc',
  datasetValue: 'count',
  excludeEmptyValues: false,
  options: {},
};
