import { type UserAccountTypeName, type UserStatusName } from 'tno-core';

import { type ISortBy } from '@/editor/features/interfaces';

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
