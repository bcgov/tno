import { IAuditColumnsModel } from './IAuditColumnsModel';

export interface IFileReferenceModel extends IAuditColumnsModel {
  contentId: number;
  contentType: string;
  fileName: string;
  path: string;
  size: number;
  runningTime: number;
  isUploaded: boolean;
}
