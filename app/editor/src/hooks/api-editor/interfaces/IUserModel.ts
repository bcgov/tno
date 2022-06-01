import { UserStatusName } from '../constants';
import { IAuditColumnsModel, IRoleModel } from '.';

export interface IUserModel extends IAuditColumnsModel {
  id: number;
  key: string;
  username: string;
  email: string;
  displayName: string;
  firstName: string;
  lastName: string;
  lastLoginOn?: Date;
  isEnabled: boolean;
  status: UserStatusName;
  emailVerified: boolean;
  isSystemAccount: boolean;
  note: string;
  roles?: IRoleModel[];
}
