import { IChartSectionSettingsModel } from '..';

export const defaultChartSectionSettings: IChartSectionSettingsModel = {
  chartType: 'bar',
  groupBy: 'otherSource',
  dataset: '',
  datasetValue: 'count',
  excludeEmptyValues: false,
  options: {},
};
