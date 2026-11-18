import { IAuditColumnsModel } from './IAuditColumnsModel';

export interface IContentLabelModel extends IAuditColumnsModel {
  id: number;
  contentId: number;
  key: string;
  value: string;
}
