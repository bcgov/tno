import { IReportTemplateModel, ReportTypeName } from 'tno-core';

export const defaultReportTemplate: IReportTemplateModel = {
  id: 0,
  name: '',
  description: '',
  reportType: ReportTypeName.Content,
  subject: '',
  body: '',
  isEnabled: true,
  sortOrder: 0,
  chartTemplates: [],
  settings: {},
};
