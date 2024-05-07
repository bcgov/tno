import { AccountAuthStateName, UserStatusName } from '../constants';
import { IUserPreferencesModel } from './IUserPreferencesModel';

export interface IUserInfoModel {
  id: number;
  key: string;
  username: string;
  email: string;
  preferredEmail: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  lastLoginOn?: Date;
  isEnabled: boolean;
  status: UserStatusName;
  mediaTypes: number[];
  authState: AccountAuthStateName;
  preferences?: IUserPreferencesModel;
  note?: string;
  roles: string[];
}
