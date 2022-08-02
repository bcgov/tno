import { UserStatusName } from '../constants';

export interface IUserFilter {
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSystemAccount?: boolean;
  status?: UserStatusName;
  sort?: string[];
  roleName?: string;
  keyword?: string;
}
