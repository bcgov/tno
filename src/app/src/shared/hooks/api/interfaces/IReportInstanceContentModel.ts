import { IAuditColumnsModel, IContentModel } from '.';

export interface IReportInstanceContentModel extends IAuditColumnsModel {
  instanceId: number;
  contentId: number;
  content?: IContentModel;
  sectionName: string;
  sortOrder: number;
}
