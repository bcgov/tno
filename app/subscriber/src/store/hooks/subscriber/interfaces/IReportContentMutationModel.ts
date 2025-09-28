import { IReportInstanceContentModel, ReportStatusName } from 'tno-core';

export interface IReportContentMutationModel {
  reportId: number;
  instanceId: number;
  ownerId?: number;
  status: ReportStatusName;
  publishedOn?: string;
  sentOn?: string;
  subject: string;
  body: string;
  response?: any;
  added?: IReportInstanceContentModel[];
  instanceCreated: boolean;
}
