import { type ISortBy } from 'features/interfaces';
import { type UserAccountTypeName, type UserStatusName } from 'tno-core';

export interface IUserListFilter {
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSystemAccount?: boolean;
  accountType?: UserAccountTypeName;
  status?: UserStatusName;
  sort: ISortBy[];
  roleName?: string;
  keyword?: string;
  page: number;
  quantity: number;
}
