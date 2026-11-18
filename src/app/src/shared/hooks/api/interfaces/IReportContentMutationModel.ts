import { IReportInstanceContentModel } from '.';
import { ReportStatusName } from '../constants';

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
