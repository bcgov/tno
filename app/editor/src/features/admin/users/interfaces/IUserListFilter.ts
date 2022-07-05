import { ISortBy } from 'features/content/list-view/interfaces';

export interface IUserListFilter {
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSystemAccount?: boolean;
  status?: string;
  sort: ISortBy[];
  roles?: string[];
  keyword?: string;
}
