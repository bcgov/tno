import { ReportStatusName } from '../constants';
import { IAuditColumnsModel, IReportInstanceContentModel, IUserModel } from '.';

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
