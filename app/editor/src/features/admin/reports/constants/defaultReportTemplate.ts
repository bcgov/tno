import { IReportTemplateModel } from 'tno-core';

export const defaultReportTemplate: IReportTemplateModel = {
  id: 0,
  name: '',
  description: '',
  subject: '',
  body: '',
  isEnabled: true,
  enableSections: false,
  enableSectionSummary: false,
  enableSummary: false,
  enableCharts: false,
  enableChartsOverTime: false,
  sortOrder: 0,
  chartTemplates: [],
};
