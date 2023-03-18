import { ISortBy } from 'features/content/list-view/interfaces';
import { UserStatusName } from 'tno-core';

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
