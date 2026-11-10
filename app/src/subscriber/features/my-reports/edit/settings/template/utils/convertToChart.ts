import {
  IReportInstanceContentModel,
  IReportSectionChartTemplateModel,
  IReportSectionModel,
} from 'tno-core';

import { getDatasets } from './getDatasets';
import { groupData } from './groupData';
import { IChartData } from './IChartData';

export interface IConvertToChartOptions {
  labelValueWhenEmpty?: string;
}

/**
 * Convert specified data into chart data for chart.js.
 * @param section The section the chart belongs in.
 * @param chart The chart to populate.
 * @param content An array of report instance content.
 * @param sections An array of sections in the report.
 * @param options The converter config options.
 * @returns Chart data.
 */
export const convertToChart = (
  section: IReportSectionModel,
  chart: IReportSectionChartTemplateModel,
  content: IReportInstanceContentModel[],
  sections: IReportSectionModel[],
  options?: IConvertToChartOptions,
): IChartData => {
  // Initialize default options.
  const params: IConvertToChartOptions = {
    ...Object.assign({ labelValueWhenEmpty: 'Unknown' }, options ?? {}),
    labelValueWhenEmpty: options?.labelValueWhenEmpty ?? 'Unknown',
  };

  let datasets = getDatasets(chart, content, params);
  let data = groupData(chart, datasets, sections, params);

  return data;
};
