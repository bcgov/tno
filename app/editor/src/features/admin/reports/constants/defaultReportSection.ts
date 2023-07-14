import { IReportSectionModel } from 'tno-core';

export const defaultReportSection = (name: string, reportId: number): IReportSectionModel => {
  return {
    id: 0,
    reportId: reportId,
    name: name,
    description: '',
    sortOrder: 0,
    isEnabled: true,
    chartTemplates: [],
    settings: {
      label: '',
      isSummary: false,
      showContent: true,
      showCharts: false,
      chartsOnTop: false,
      chartDirection: 'row',
      removeDuplicates: false,
      sortBy: '',
    },
  };
};
