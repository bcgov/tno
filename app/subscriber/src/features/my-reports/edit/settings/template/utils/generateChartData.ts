import {
  IReportInstanceContentModel,
  IReportSectionChartTemplateModel,
  IReportSectionModel,
} from 'tno-core';

import { getDatasets } from './getDatasets';
import { groupData } from './groupData';
import { IChartData } from './IChartData';

export interface IGenerateChartDataOptions {
  labelValueWhenEmpty?: string;
}

export const generateChartData = (
  chart: IReportSectionChartTemplateModel,
  content: IReportInstanceContentModel[],
  sections: IReportSectionModel[],
  options?: IGenerateChartDataOptions,
): IChartData => {
  // Initialize default options.
  const params: IGenerateChartDataOptions = {
    ...Object.assign({ labelValueWhenEmpty: 'Unknown' }, options ?? {}),
    labelValueWhenEmpty: options?.labelValueWhenEmpty ?? 'Unknown',
  };

  let datasets = getDatasets(chart, content, params);
  let result = groupData(chart, datasets, sections, params);

  return result;
};
