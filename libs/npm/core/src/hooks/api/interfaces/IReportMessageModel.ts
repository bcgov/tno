import { IUserModel, ReportStatusName } from '..';

export interface IReportMessageModel {
  id: number;
  subject: string;
  status: ReportStatusName;
  ownerId?: number;
  owner?: IUserModel;
}
