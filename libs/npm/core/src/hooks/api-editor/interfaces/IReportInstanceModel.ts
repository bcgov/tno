import { IAuditColumnsModel, IContentModel, IUserModel } from '.';

export interface IReportInstanceModel extends IAuditColumnsModel {
  id: number;
  reportId: number;
  ownerId?: number;
  owner?: IUserModel;
  publishedOn?: Date | string;
  response: any;
  content: IContentModel[];
}
