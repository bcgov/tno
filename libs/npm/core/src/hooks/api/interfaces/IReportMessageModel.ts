import { IAuditColumnsModel, IUserModel, ReportStatusName } from '..';

export interface IReportMessageModel extends IAuditColumnsModel {
  id: number;
  reportId: number;
  subject: string;
  status: ReportStatusName;
  ownerId?: number;
  owner?: IUserModel;
}
