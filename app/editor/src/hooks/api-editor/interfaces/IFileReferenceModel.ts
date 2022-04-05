import { IAuditColumnsModel } from './IAuditColumnsModel';

export interface IFileReferenceModel extends IAuditColumnsModel {
  contentId: number;
  mimeType: string;
  path: string;
  size: number;
  runningTime: number;
}
