import { IAuditColumnsModel, IContentModel } from '.';

export interface IReportInstanceContentModel extends IAuditColumnsModel {
  reportInstanceId: number;
  contentId: number;
  content?: IContentModel;
  sectionName: string;
  sortOrder: number;
}
