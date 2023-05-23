import { UserStatusName } from '../constants';

export interface IUserInfoModel {
  id: number;
  key: string;
  username: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  lastLoginOn?: Date;
  isEnabled: boolean;
  status: UserStatusName;
  preferences?: any;
  note?: string;
  roles: string[];
}
