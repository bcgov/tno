import { IAuditColumnsModel } from './IAuditColumnsModel';
export interface ISortableModel<T extends string | number> extends IAuditColumnsModel {
  id: T;
  name: string;
  description: string;
  sortOrder: number;
  isEnabled: boolean;
}
