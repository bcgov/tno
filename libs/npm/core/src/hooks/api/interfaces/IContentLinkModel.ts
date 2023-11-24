import { IAuditColumnsModel } from './IAuditColumnsModel';

export interface IContentLinkModel extends IAuditColumnsModel {
  contentId: number;
  linkId: number;
}
