import { IAuditColumnsModel, IReportInstanceContentModel, IUserModel } from '.';
import { ReportStatusName } from '../constants';

export interface IReportInstanceModel extends IAuditColumnsModel {
  id: number;
  reportId: number;
  ownerId?: number;
  owner?: IUserModel;
  publishedOn?: Date | string;
  sentOn?: Date | string;
  status: ReportStatusName;
  subject: string;
  body: string;
  response: any;
  content: IReportInstanceContentModel[];
}
