import { IAuditColumnsModel } from '.';

export interface IRoleModel extends IAuditColumnsModel {
  id: string;
  key: string;
  name: string;
  description: string;
  isEnabled: boolean;
}
