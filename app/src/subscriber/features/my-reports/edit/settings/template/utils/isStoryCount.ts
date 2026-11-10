import { IReportSectionChartTemplateModel } from 'tno-core';

export const isStoryCount = (chart: IReportSectionChartTemplateModel) => {
  return chart.sectionSettings.datasetValue === 'count';
};
