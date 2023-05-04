import { IAuditColumnsModel, IContentModel } from '.';

export interface IReportInstanceModel extends IAuditColumnsModel {
  id: number;
  reportId: number;
  publishedOn?: Date | string;
  response: any;
  content: IContentModel[];
}
