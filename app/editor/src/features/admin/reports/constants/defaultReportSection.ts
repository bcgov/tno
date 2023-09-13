import { IReportSectionModel, ReportSectionTypeName } from 'tno-core';

export const defaultReportSection = (reportId: number): IReportSectionModel => {
  return {
    id: 0,
    reportId: reportId,
    name: crypto.randomUUID(),
    description: '',
    sortOrder: 0,
    isEnabled: true,
    chartTemplates: [],
    settings: {
      label: '',
      sectionType: ReportSectionTypeName.Content,
      removeDuplicates: false,
      showHeadlines: true,
      showFullStory: false,
      showImage: false,
      hideEmpty: false,
      showCharts: false,
      chartsOnTop: false,
      chartDirection: 'row',
      groupBy: '',
      sortBy: '',
    },
  };
};
