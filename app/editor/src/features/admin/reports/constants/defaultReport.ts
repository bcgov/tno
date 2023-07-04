import { IReportModel, ReportTypeName } from 'tno-core';

import { defaultReportTemplate } from './defaultReportTemplate';

export const defaultReport: IReportModel = {
  id: 0,
  name: '',
  description: '',
  reportType: ReportTypeName.Custom,
  ownerId: 0,
  templateId: 0,
  template: defaultReportTemplate,
  filter: {},
  settings: {},
  isEnabled: false,
  isPublic: false,
  sortOrder: 0,
  subscribers: [],
  instances: [],
};
