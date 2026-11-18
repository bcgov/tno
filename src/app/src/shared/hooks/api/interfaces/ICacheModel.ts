import { IAuditColumnsModel } from '.';

export interface ICacheModel extends IAuditColumnsModel {
  key: string;
  value?: string;
  description?: string;
}
