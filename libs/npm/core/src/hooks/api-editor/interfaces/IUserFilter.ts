import { UserStatusName } from '../constants';

export interface IUserFilter {
  includeUserId?: number;
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
  page?: number;
  quantity?: number;
}
