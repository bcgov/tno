import { ISortBy } from 'features/content/list-view/interfaces';

import { UserStatusName } from '../constants';

export interface IUserFilter {
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSystemAccount?: boolean;
  // status?: UserStatusName;
  status?: string;
  sort: any;
  role?: string;
  keyword?: string;
}
