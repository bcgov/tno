import { UserStatusName } from '../constants';

export interface IRegisterModel {
  email: string;
  code: string;
  status: UserStatusName;
  message: string;
}
