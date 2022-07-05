import { ISortBy } from 'features/content/list-view/interfaces';
import { UserStatusName } from 'hooks';

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
  roles?: string[];
  keyword?: string;
}
