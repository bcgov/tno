import { UserStatusName } from 'tno-core';

import { ISortBy } from '@/subscriber/features/utils/interfaces';

export interface IUserListFilter {
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSystemAccount?: boolean;
  status?: UserStatusName;
  sort: ISortBy[];
  roleName?: string;
  keyword?: string;
  pageIndex: number;
  pageSize: number;
}
