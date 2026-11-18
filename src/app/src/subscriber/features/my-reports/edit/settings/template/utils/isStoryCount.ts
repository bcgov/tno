import { IReportSectionChartTemplateModel } from '@/shared';

export const isStoryCount = (chart: IReportSectionChartTemplateModel) => {
  return chart.sectionSettings.datasetValue === 'count';
};
