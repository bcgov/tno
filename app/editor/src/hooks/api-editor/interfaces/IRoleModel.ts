import { IAuditColumnsModel, IClaimModel } from '.';

export interface IRoleModel extends IAuditColumnsModel {
  id: number;
  key: string;
  name: string;
  description: string;
  isEnabled: boolean;
  claims?: IClaimModel[];
}
