import { IAuditColumnsModel } from '.';

export interface IReportInstanceContentModel extends IAuditColumnsModel {
  reportInstanceId: number;
  contentId: number;
  sectionName: string;
}
