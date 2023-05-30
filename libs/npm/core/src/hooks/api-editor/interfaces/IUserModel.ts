import { UserStatusName } from '../constants';
import { IAuditColumnsModel } from '.';

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
  preferences?: any;
  note: string;
  roles?: string[];
}
