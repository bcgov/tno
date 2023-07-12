import { IReportTemplateModel } from 'tno-core';

export const defaultReportTemplate: IReportTemplateModel = {
  id: 0,
  name: '',
  description: '',
  subject: '',
  body: '',
  isEnabled: true,
  sortOrder: 0,
  chartTemplates: [],
  settings: {
    enableSummaryCharts: false,
    enableSectionCharts: false,
  },
};
