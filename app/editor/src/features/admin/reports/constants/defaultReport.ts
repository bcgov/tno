import { IReportModel, ReportTypeName } from 'tno-core';

export const defaultReport: IReportModel = {
  id: 0,
  name: '',
  description: '',
  reportType: ReportTypeName.Custom,
  ownerId: 0,
  template: '',
  filter: {},
  settings: {},
  isEnabled: false,
  isPublic: false,
  sortOrder: 0,
  subscribers: [],
  instances: [],
};
