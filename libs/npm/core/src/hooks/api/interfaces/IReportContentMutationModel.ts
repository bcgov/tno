import { ReportStatusName } from '../constants';
import { IReportInstanceContentModel } from '.';

export interface IReportContentMutationModel {
  reportId: number;
  instanceId: number;
  status: ReportStatusName;
  ownerId?: number;
  publishedOn?: string;
  sentOn?: string;
  subject: string;
  body: string;
  response?: any;
  added: IReportInstanceContentModel[];
  instanceCreated: boolean;
}
